using System.Text.Json;
using System.Text.Json.Serialization;
using JsonException = System.Text.Json.JsonException;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace ImageContext.Components.Services;

public class GeocodingService
{
    private async Task<HashSet<string?>> GetSearchTerms(string jsonRequestString)
    {
        HashSet<string> searchTerms = new HashSet<string>();
        using (JsonDocument doc = JsonDocument.Parse(jsonRequestString))
        {
            // Get the root element
            JsonElement root = doc.RootElement;

            JsonElement results = root.GetProperty("results");

            // Iterate through each result in the array
            foreach (JsonElement result in results.EnumerateArray())
            {
                JsonElement addressComponents = result.GetProperty("address_components");
                foreach (JsonElement addressComponent in addressComponents.EnumerateArray())
                {
                    JsonElement types = addressComponent.GetProperty("types");

                    foreach (var type in addressComponent.GetProperty("types").EnumerateArray())
                    {
                        if (!type.ToString().Contains("street_number") &&
                            !type.ToString().Contains("intersection") && !type.ToString().Contains("plus_code") &&
                            !type.ToString().Contains("postal_code_suffix"))
                        {
                            searchTerms.Add(addressComponent.GetProperty("long_name").ToString());
                        }
                    }
                }
            }
            Console.WriteLine("Google Geocoding API: Returned Search Terms");
            
            return searchTerms;
        }
    }

    private async Task<List<string?>> GetFormatedAddresses(string jsonRequestString)
    {
        List<string?> formatedAddresses = new List<string?>();
        using (JsonDocument doc = JsonDocument.Parse(jsonRequestString))
        {
            JsonElement root = doc.RootElement;
            JsonElement results = root.GetProperty("results");
            foreach (JsonElement result in results.EnumerateArray())
            {
                JsonElement formatedAddress = result.GetProperty("formatted_address");
                formatedAddresses.Add(formatedAddress.ToString());
            }
            Console.WriteLine("Google Geocoding API: Returned Formatted Addresses");
            
            return formatedAddresses;
        }
    }
    
    public async Task<(HashSet<string?>, List<string?>)> RequestGeocodingApi (IConfiguration config, HttpClient httpClient, (double, double)? coordinates)
    {
        var key = config["GoogleServicesKey"];
        var request = await httpClient.GetAsync(
            $"https://maps.googleapis.com/maps/api/geocode/json?latlng={coordinates.Value.Item1},{coordinates.Value.Item2}&key={key}");

        if (request.IsSuccessStatusCode)
        {
            var requestJsonString = await request.Content.ReadAsStringAsync();
            var searchTerms = await GetSearchTerms(requestJsonString);
            var formatedAddresses = await GetFormatedAddresses(requestJsonString);
            
            Console.WriteLine("Google Geocoding API: Request Successful");

            return (searchTerms, formatedAddresses);
        }
        throw new Exception("Google Geocoding API: Request Failure");
    }
}

    