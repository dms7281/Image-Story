using System.Text.Json;
using System.Text.Json.Serialization;
using JsonException = System.Text.Json.JsonException;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace ImageContext.Components.Services;

public class GeocodingService
{
    private async Task<List<string?>> GetSearchTerms(string jsonRequestString)
    {
        List<string> searchTerms = new List<string>();
        List<string> excludedTypes = new List<string>{"street_number", "route", "intersection", "plus_code", "postal_code", "postal_code_suffix", "country"};
        string concatenatedTerms = string.Empty;
        using (JsonDocument doc = JsonDocument.Parse(jsonRequestString))
        {
            // Get the root element
            JsonElement root = doc.RootElement;

            JsonElement results = root.GetProperty("results");
            
            JsonElement addressComponents = results[0].GetProperty("address_components");

            var addressCompArray = addressComponents.EnumerateArray().ToArray();
            
            for (int i = addressCompArray.Length - 1; i >= 0; i--)
            {
                JsonElement types = addressComponents[i].GetProperty("types");
            
                foreach (var type in types.EnumerateArray())
                {
                    // Code currently excludes routes, work on implementing them with cities in the future
                    if (excludedTypes.Contains(type.ToString())) break;
                    
                    var addressComp = addressComponents[i].GetProperty("long_name").ToString();
            
                    if (concatenatedTerms.Contains(addressComp)) break;
                    
                    concatenatedTerms = string.IsNullOrEmpty(concatenatedTerms) 
                        ? $"\"{addressComp}\"" 
                        : $"\"{addressComp}\" {concatenatedTerms}";
        
                    // Add the new concatenated string to the search terms list
                    searchTerms.Insert(0, concatenatedTerms);
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
    
    public async Task<(List<string?>, List<string?>)> RequestGeocodingApi (IConfiguration config, HttpClient httpClient, (double, double)? coordinates)
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

    