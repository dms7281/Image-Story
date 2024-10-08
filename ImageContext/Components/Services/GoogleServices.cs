using System.Text.Json;
using System.Text.Json.Serialization;
using System.Web;
using Microsoft.AspNetCore.Http.Features;
using JsonException = System.Text.Json.JsonException;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace ImageContext.Components.Services;

public class GoogleServices(IHttpClientFactory httpClientFactory, IConfiguration configuration)
{
    private readonly string? _key = configuration["GoogleServicesKey"];
    private readonly string? _googleSearchEngine = configuration["GoogleSearchEngine"];

    // private async Task<List<string?>> GetSearchTerms(string jsonRequestString)
    // {
    //     List<string> searchTerms = new List<string>();
    //     List<string> excludedTypes = new List<string>{"street_number", "route", "intersection", "plus_code", "postal_code", "postal_code_suffix", "country"};
    //     string concatenatedTerms = string.Empty;
    //     using (JsonDocument doc = JsonDocument.Parse(jsonRequestString))
    //     {
    //         // Get the root element
    //         JsonElement root = doc.RootElement;
    //
    //         JsonElement results = root.GetProperty("results");
    //         
    //         JsonElement addressComponents = results[0].GetProperty("address_components");
    //
    //         var addressCompArray = addressComponents.EnumerateArray().ToArray();
    //         
    //         //for (int i = addressCompArray.Length - 1; i >= 0; i--)
    //         for (int i = 0; i < addressCompArray.Length; i++)
    //         {
    //             JsonElement types = addressComponents[i].GetProperty("types");
    //         
    //             foreach (var type in types.EnumerateArray())
    //             {
    //                 // Code currently excludes routes, work on implementing them with cities in the future
    //                 if (excludedTypes.Contains(type.ToString())) break;
    //                 
    //                 var addressComp = addressComponents[i].GetProperty("long_name").ToString();
    //
    //                 if (searchTerms.Contains(addressComp)) break;
    //                 
    //                 searchTerms.Add(addressComp);
    //
    //                 // if (concatenatedTerms.Contains(addressComp)) break;
    //                 //
    //                 // concatenatedTerms = string.IsNullOrEmpty(concatenatedTerms) 
    //                 //     ? $"\"{addressComp}\"" 
    //                 //     : $"\"{addressComp}\" {concatenatedTerms}";
    //                 //
    //                 // // Add the new concatenated string to the search terms list
    //                 // searchTerms.Insert(0, concatenatedTerms);
    //             }
    //         }
    //         
    //         Console.WriteLine("Google Geocoding API: Returned Search Terms");
    //         
    //         return searchTerms;
    //     }
    // }

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
    
    public async Task<List<string?>> GeocodingApi ((double,double) coordinates)
    {
        var httpClient = httpClientFactory.CreateClient("GoogleGeocoding");

        var request = await httpClient.GetAsync($"json?latlng={coordinates.Item1},{coordinates.Item2}&key={_key}");

        if (request.IsSuccessStatusCode)
        {
            var requestJsonString = await request.Content.ReadAsStringAsync();
            //var searchTerms = await GetSearchTerms(requestJsonString);
            var formatedAddresses = await GetFormatedAddresses(requestJsonString);
            
            Console.WriteLine("Google Geocoding API: Request Successful");

            return formatedAddresses;
        }
        throw new Exception("Google Geocoding API: Request Failure");
    }

    public async Task<List<(string title, string link, string snippit, string? thumbnailUrl)>> SearchLocationTime(string searchPhrase, DateTime date)
    {
        string afterDate = date.AddDays(-1).ToString("yyyy-MM-dd");
        string beforeDate = date.AddDays(1).ToString("yyyy-MM-dd");
        
        var httpClient = httpClientFactory.CreateClient("GoogleSearch");

        List<(string title, string link, string snippit, string? thumbnailUrl)> webResults = new List<(string, string, string, string?)>();
            
        var request = await httpClient.GetAsync($"v1?cx={_googleSearchEngine}&q={searchPhrase}+after:{afterDate}+before:{beforeDate}&key={_key}");

        if (request.IsSuccessStatusCode)
        {
            var jsonString = await request.Content.ReadAsStringAsync();
            
            using (JsonDocument doc = JsonDocument.Parse(jsonString))
            {
                JsonElement root = doc.RootElement;
                JsonElement items = root.GetProperty("items");
                foreach (JsonElement item in items.EnumerateArray())
                {
                    string? thumbnailUrl = null;
                    
                    if (item.TryGetProperty("pagemap", out JsonElement pagemap))
                    {
                        //Console.WriteLine(pagemap.ToString());
                        if (pagemap.TryGetProperty("cse_thumbnail", out JsonElement thumbnail))
                        {
                            var thumnailLink = thumbnail.EnumerateArray().ToArray()[0].GetProperty("src").ToString();
                            thumbnailUrl = thumnailLink;
                        }
                    }
                    
                    (string title, string link, string snippit, string? thumbnailUrl) webResult = (
                        item.GetProperty("title").ToString(),
                        item.GetProperty("link").ToString(),
                        item.GetProperty("snippet").ToString(),
                        thumbnailUrl
                    );
                    
                    webResults.Add(webResult);
                }
                Console.WriteLine("Google Geocoding API: Returned Formatted Addresses");
            }

            return webResults;
        }
        throw new Exception("Google Custom Search API: Request Failure");
    }
    
    public async Task<List<(string, string, string, string?)>> SearchLocation(string searchPhrase)
    {
        var httpClient = httpClientFactory.CreateClient("GoogleSearch");

        List<(string, string, string, string?)> webResults = new List<(string, string, string, string?)>();
            
        var request = await httpClient.GetAsync($"v1?cx={_googleSearchEngine}&q={searchPhrase}&key={_key}");

        if (request.IsSuccessStatusCode)
        {
            var jsonString = await request.Content.ReadAsStringAsync();
            
            using (JsonDocument doc = JsonDocument.Parse(jsonString))
            {
                JsonElement root = doc.RootElement;
                JsonElement items = root.GetProperty("items");
                foreach (JsonElement item in items.EnumerateArray())
                {
                    string? thumbnailUrl = null;
                    
                    if (item.TryGetProperty("pagemap", out JsonElement pagemap))
                    {
                        //Console.WriteLine(pagemap.ToString());
                        if (pagemap.TryGetProperty("cse_thumbnail", out JsonElement thumbnail))
                        {
                            var thumnailLink = thumbnail.EnumerateArray().ToArray()[0].GetProperty("src").ToString();
                            thumbnailUrl = thumnailLink;
                        }
                    }
                    
                    (string, string, string, string?) webResult = (
                        item.GetProperty("title").ToString(),
                        item.GetProperty("link").ToString(),
                        item.GetProperty("snippet").ToString(),
                        thumbnailUrl
                    );
                    
                    webResults.Add(webResult);
                }
                Console.WriteLine("Google Geocoding API: Returned Formatted Addresses");
            }

            return webResults;
        }
        throw new Exception("Google Custom Search API: Request Failure");
    }
}