using System.Text.Json;

namespace ImageContext.Components.Services;

public class NewsService(IConfiguration config, HttpClient httpClient)
{
    private readonly string? newsApiKey = config["NewsKey"];
    
    // public async Task<List<NewsArticle?>> GetNews(DateTime dateTaken, string address)
    // {
    //     List<string> searchTerms = AddressToSearchTerms(address);
    //
    //     foreach (string searchTerm in searchTerms)
    //     {
    //         var request = await httpClient.GetAsync(
    //             $"https://newsapi.org/v2/everything?q={searchTerm}&apiKey={newsApiKey}");
    //         
    //         if (request.IsSuccessStatusCode)
    //         {
    //             var jsonString = await request.Content.ReadAsStringAsync();
    //             List<NewsArticle?> newsArticles = new List<NewsArticle?>();
    //             
    //             using (JsonDocument doc = JsonDocument.Parse(jsonString))
    //             {
    //                 // Get the root element
    //                 JsonElement root = doc.RootElement;
    //
    //                 // Assuming "articles" is the name of the array in the JSON
    //                 JsonElement articles = root.GetProperty("articles");
    //
    //                 // Iterate through each article in the array
    //                 foreach (JsonElement article in articles.EnumerateArray())
    //                 {
    //                     NewsArticle newsArticle = new NewsArticle
    //                     {
    //                         url = GetJsonPropertyValue("url", article),
    //                         author = GetJsonPropertyValue("author", article),
    //                         title = GetJsonPropertyValue("title", article),
    //                         description = GetJsonPropertyValue("description", article),
    //                         content = GetJsonPropertyValue("content", article)
    //                     };
    //
    //                     newsArticles.Add(newsArticle);
    //                 }
    //             }
    //             
    //             Console.WriteLine("News API Request Successful");
    //             return newsArticles;
    //
    //             string GetJsonPropertyValue(string propertyName, JsonElement element)
    //             {
    //                 return element.TryGetProperty(propertyName, out JsonElement value) ? value.GetString() : null;
    //             }
    //         }
    //     }
    //     throw new Exception("News API Request Failure");
    // }

    // private List<string> AddressToSearchTerms(string address)
    // {
    //     
    // }

    private struct NewsArticle
    {
        public string url;
        public string author;
        public string title;
        public string description;
        public string content;
    }
}