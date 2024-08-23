using System.Text.Json.Serialization;
using JsonException = System.Text.Json.JsonException;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace ImageContext.Components.Services;

public class GeocodingService(IConfiguration config, HttpClient httpClient)
{
    private readonly string? _googleGeocodingApiKey = config["GeocodingService:GoogleGeocodingApiKey"];

    public async Task<(string?, List<string?>?, List<string?>?)> GetLocationData((double, double)? coordinates)
    {
        var request = await httpClient.GetAsync(
            $"https://maps.googleapis.com/maps/api/geocode/json?latlng={coordinates.Value.Item1},{coordinates.Value.Item2}&key={_googleGeocodingApiKey}");

        if (request.IsSuccessStatusCode)
        {
            Console.WriteLine("Google Geocoding API Request: Successful");
            var jsonString = await request.Content.ReadAsStringAsync();
            var json = JsonSerializer.Deserialize<GeocodingJson>(jsonString);
            
            var compoundCode = json?.PlusCodes?.CompoundCode;

            var addressList = new List<string?>();

            var placeList = new List<string?>();

            foreach (Result result in json.Results)
            {
                addressList.Add(result.FormattedAddress);
                placeList.Add(result.PlaceId);
            }
            
            Console.WriteLine("Google Geocoding API Response: Successful");

            return (compoundCode, addressList, placeList);
        }
        throw new Exception("Google Geocoding API Request Failure");
    }
    
    private class GeocodingJson
    { 
        [JsonPropertyName("plus_code")]
        public PlusCode? PlusCodes { get; set; }
    
        [JsonPropertyName("results")]
        public List<Result>? Results { get; set; }
    }

    private class PlusCode
    {
        [JsonPropertyName("compound_code")]
        public string? CompoundCode { get; set; }
    
        // [JsonPropertyName("global_code")]
        // public string? GlobalCode { get; set; }
    }
    private class Result
    {
        [JsonPropertyName("formatted_address")]
        public string? FormattedAddress { get; set; }
    
        [JsonPropertyName("place_id")]
        public string? PlaceId { get; set; }
    
        [JsonPropertyName("types")]
        public List<string?>? Types { get; set; }
    }
}