using System.Text.Json;
using System.Text.RegularExpressions;
using Newtonsoft.Json;

namespace ImageContext.Components.Services;

public class PlacesService(IConfiguration config, HttpClient httpClient)
{
    private readonly string? _googleGeocodingApiKey = config["GoogleServicesKey"];
    
    public async Task<string?> GetExplicitPlace(string? placeId)
    {
        // Convert placeId into it's "Place" and if the place is named, return the named place.
        var place = await GetPlace(placeId);
        
        if (IsExplicitName(place))
        {
            return place;
        }
        
        // If none of the places were named
        return null;
    }

    public async Task<string?> GetPlace(string? placeId)
    {
        var request = await httpClient.GetAsync(
            $"https://places.googleapis.com/v1/places/{placeId}?fields=displayName&key={_googleGeocodingApiKey}");
        
        if (request.IsSuccessStatusCode)
        {
            Console.WriteLine("Google Places API Request Successful");
            var jsonString = await request.Content.ReadAsStringAsync();
            string? placeName;
            
            using (JsonDocument doc = JsonDocument.Parse(jsonString))
            {
                JsonElement root = doc.RootElement;
                placeName= root.GetProperty("displayName").GetProperty("text").GetString();
            }
            
            return placeName;
        }
        throw new Exception("Google Places API Request Failure");
    }
    
    private bool IsExplicitName(string? placeName)
    {
        if (string.IsNullOrWhiteSpace(placeName))
        {
            return false;
        }

        // Check if the name starts with a number (likely an address)
        if (Regex.IsMatch(placeName, @"^\d+.*"))
        {
            return false;
        }

        // Check if the name contains a postal code or plus code
        if (Regex.IsMatch(placeName, @"\b\d{5}\b") || Regex.IsMatch(placeName, @"[A-Z0-9]{4,}\+\w+"))
        {
            return false;
        }
        
        // Check if place is "unnamed"
        if (placeName.Contains("Unnamed"))
        {
            return false;
        }

        return true;
    }
}