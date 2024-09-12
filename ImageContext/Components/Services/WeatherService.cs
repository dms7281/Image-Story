using System.Text.Json;

namespace ImageContext.Components.Services;

public class WeatherService(IHttpClientFactory httpClientFactory, IConfiguration configuration)
{
    private readonly string? _key = configuration["OpenWeatherMapKey"];

    public async Task<(double, double, double, string)> GetWeatherDateLocation((double,double) coordinates, DateTime date)
    {
        var httpClient = httpClientFactory.CreateClient("HistoricalWeatherData");

        var unixDate = new DateTimeOffset(date.ToUniversalTime()).ToUnixTimeSeconds();

        (double, double, double, string) weatherResult;
        

        var request = await httpClient.GetAsync($"timemachine?lat={coordinates.Item1}&lon={coordinates.Item2}&dt={unixDate}&units=imperial&appid={_key}");
        
        if (request.IsSuccessStatusCode)
        {
            var requestJsonString = await request.Content.ReadAsStringAsync();
            
            Console.WriteLine(requestJsonString);

            using (JsonDocument doc = JsonDocument.Parse(requestJsonString))
            {
                JsonElement root = doc.RootElement;

                JsonElement data = root.GetProperty("data").EnumerateArray().ToArray()[0];

                weatherResult = (
                    data.GetProperty("temp").GetDouble(),
                    data.GetProperty("humidity").GetDouble(),
                    data.GetProperty("wind_speed").GetDouble(),
                    data.GetProperty("weather").EnumerateArray().ToArray()[0].GetProperty("description").ToString()
                );
            }
            Console.WriteLine("Open Weather Map API: Request Successful");
            return weatherResult;
        }
        throw new Exception("Open Weather Map API: Request Failure");
    }
}