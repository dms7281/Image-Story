using System;
using System.Linq;
using Microsoft.Bing.NewsSearch;

namespace ImageContext.Components.Services;

public class NewsService(IConfiguration config, HttpClient httpClient)
{
    private readonly string? _bingApiKey = config["BingKey"];
    
    public async Task<List<Article>> GetNews(DateTime dateTaken, List<string?>? searchTerms)
    {
        var year = dateTaken.Year;
        var month = dateTaken.Month;
        var day = dateTaken.Day;

        var date = $"{year}-{month}-{day}";
        
        var client = new NewsSearchClient(new ApiKeyServiceClientCredentials(_bingApiKey));

        List<Article> articles = new List<Article>();
        
        foreach (string? searchTerm in searchTerms)
        {
            var newsResults = await client.News.SearchAsync(query: searchTerm, count: 100, sortBy: "Date");
            
            if (newsResults.Value.Count < 5) continue;
            
            Console.WriteLine("Search Term: " + searchTerm);
            Console.WriteLine($"TotalEstimatedMatches value: {newsResults.TotalEstimatedMatches}");
            Console.WriteLine($"News result count: {newsResults.Value.Count}");

            foreach (var newsResult in newsResults.Value)
            {
                if (!newsResult.DatePublished.Contains(date)) continue;
                
                Article article = new Article();

                article.url = newsResult.Url;
                article.provider = newsResult.Provider[0].Name;
                article.title = newsResult.Name;
                article.description = newsResult.Description;
                
                articles.Add(article);
            }
            return articles;
        }
        throw new Exception("News API Request Failure");
    }

    public struct Article
    {
        public string url;
        public string provider;
        public string title;
        public string description;
    }
}