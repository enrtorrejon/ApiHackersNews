using Microsoft.Extensions.Caching.Memory;
using System.Net.Http.Json;
using ApiHackersNews.DataModel;
using Microsoft.Extensions.Configuration;

namespace ApiHackersNews.Services;
// --- Core Service Implementation ---
// Utilizing C# Primary Constructors for clean Dependency Injection
public class HackerNewsService(IHttpClientFactory clientFactory, IMemoryCache cache, IConfiguration configuration):IHackerNewsService
{
    private readonly string BaseUrl = configuration["ApiHackersNews:BaseUrl"]; // Base URL for the Hacker News API, loaded from configuration settings
    private readonly int topStoriesCacheDurationInMinutes = configuration.GetValue<int>("ApiHackersNews:CacheSettings:TopStoriesCacheDurationInMinutes");
    private readonly int itemCacheDurationInMinutes = configuration.GetValue<int>("ApiHackersNews:CacheSettings:ItemCacheDurationInMinutes");
    private const string CacheKey = "TopStoriesList";
    private readonly HttpClient _httpClient = clientFactory.CreateClient();

    public async Task<IEnumerable<StoryDto>> GetTopStoriesAsync(int limit)
    {
        // 1. Fetch or Cache the list of Top Story IDs (cached for 2 minutes, check ApplicationSettings for duration)
        var storyIds = await cache.GetOrCreateAsync(CacheKey, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(topStoriesCacheDurationInMinutes);
            var response = await _httpClient.GetFromJsonAsync<List<int>>($"{BaseUrl}topstories.json");
            return response ?? [];
        });

        if (storyIds == null || !storyIds.Any()) return [];

        // 1. Fetch details for all stories concurrently to prevent bottlenecking
        var tasks = storyIds.Select(GetStoryDetailsAsync);
        var stories = await Task.WhenAll(tasks);

        // 2. Order by score and take only the number of requested IDs
        var orderedStories = stories.OrderByDescending(x => x.Score).Take(limit);

        // Filter out nulls (failed requests) and return cleanly mapped DTOs
        return orderedStories.Where(s => s != null)!;
    }

    private async Task<StoryDto?> GetStoryDetailsAsync(int id)
    {
        string itemCacheKey = $"Story_{id}";

        // Cache individual stories for 10 minutes (check ApplicationSettings for duration) since old stories rarely change details
        return await cache.GetOrCreateAsync(itemCacheKey, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(itemCacheDurationInMinutes);
            try
            {
                var item = await _httpClient.GetFromJsonAsync<HackerNewsItem>($"{BaseUrl}item/{id}.json");
                if (item == null || string.IsNullOrEmpty(item.Title)) return null;

                // Convert Unix timestamp to readable DateTime
                var dateTime = DateTimeOffset.FromUnixTimeSeconds(item.Time).UtcDateTime;

                return new StoryDto(item.Title, item.Url, item.By, dateTime, item.Score, item.Descendants);
            }
            catch
            {
                // Soft fail an individual item request so it doesn't break the entire list
                return null;
            }
        });
    }
}
    