using System.Text.Json;

namespace ImageContext.Components.Services;

public class WeatherService(IHttpClientFactory httpClientFactory, IConfiguration configuration)
{
    private readonly string? _key = configuration["OpenWeatherMapKey"];

    public async Task<WeatherResult> GetWeatherDateLocation(string lat, string lng, DateTime date)
    {
        var httpClient = httpClientFactory.CreateClient("HistoricalWeatherData");

        var unixDate = new DateTimeOffset(date.ToUniversalTime()).ToUnixTimeSeconds();
        

        var request = await httpClient.GetAsync($"timemachine?lat={lat}&lon={lng}&dt={unixDate}&units=imperial&appid={_key}");
        
        if (request.IsSuccessStatusCode)
        {
            var requestJsonString = await request.Content.ReadAsStringAsync();
            
            Console.WriteLine(requestJsonString);
            
            WeatherResult weatherResult = new WeatherResult();

            using (JsonDocument doc = JsonDocument.Parse(requestJsonString))
            {
                JsonElement root = doc.RootElement;

                JsonElement data = root.GetProperty("data").EnumerateArray().ToArray()[0];

                weatherResult.temp = data.GetProperty("temp").GetDouble();
                weatherResult.feelsLikeTemp = data.GetProperty("feels_like").GetDouble();
                weatherResult.humidity = data.GetProperty("humidity").GetDouble();
                weatherResult.windSpeed = data.GetProperty("wind_speed").GetDouble();
                
                weatherResult.description = data.GetProperty("weather").EnumerateArray().ToArray()[0]
                    .GetProperty("description").ToString();
            }
            Console.WriteLine("Open Weather Map API: Request Successful");
            return weatherResult;
        }
        throw new Exception("Open Weather Map API: Request Failure");
    }
}

public struct WeatherResult
{
    public double temp;
    public double feelsLikeTemp;
    public double humidity;
    public double windSpeed;
    public string description;
}