using System.Text.Json.Serialization;
using JsonException = System.Text.Json.JsonException;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace ImageContext.Components.Services;

public class GeocodingService(IConfiguration config, HttpClient httpClient)
{
    private readonly string? _googleGeocodingApiKey = config["GoogleServicesKey"];

    public async Task<(List<string>, string)> GetLocationData((double, double)? coordinates)
    {
        var request = await httpClient.GetAsync(
            $"https://maps.googleapis.com/maps/api/geocode/json?latlng={coordinates.Value.Item1},{coordinates.Value.Item2}&key={_googleGeocodingApiKey}");

        if (request.IsSuccessStatusCode)
        {
            Console.WriteLine("Google Geocoding API Request: Successful");
            var jsonString = await request.Content.ReadAsStringAsync();
            var json = JsonSerializer.Deserialize<GeocodingJson>(jsonString);
            
            var compoundCode = json?.PlusCodes?.CompoundCode;

            //var addressList = new List<string?>();

            List<string> places = new List<string>();

            foreach (Result result in json.Results)
            {
                var place = await new PlacesService(config, httpClient).GetExplicitPlace(result.PlaceId);
                if(place != null) places.Add(place);
            }

            if (places.Count > 0)
            {
                return (places, compoundCode);
            }
            
            return (places, compoundCode);
            
            
            Console.WriteLine("Google Geocoding API Response: Successful");
            
            //var firstAddress = await new PlacesService(config, httpClient).GetPlace(json.Results[0].PlaceId);
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