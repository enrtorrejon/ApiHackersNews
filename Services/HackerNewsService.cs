using Microsoft.Extensions.Caching.Memory;
using System.Net.Http.Json;
using ApiHackersNews.DataModel;
using Microsoft.Extensions.Configuration;

namespace ApiHackersNews.Services;

public class HackerNewsService(IHttpClientFactory clientFactory, IMemoryCache cache, IConfiguration configuration, ILogger<HackerNewsService> logger):IHackerNewsService
{
    private readonly string BaseUrl = configuration["ApiHackersNews:BaseUrl"]; // Base URL for the Hacker News API, loaded from configuration settings
    private readonly int topStoriesCacheDurationInMinutes = configuration.GetValue<int>("ApiHackersNews:CacheSettings:TopStoriesCacheDurationInMinutes");
    private readonly int itemCacheDurationInMinutes = configuration.GetValue<int>("ApiHackersNews:CacheSettings:ItemCacheDurationInMinutes");
    private const string CacheKey = "TopStoriesList";
    private readonly HttpClient _httpClient = clientFactory.CreateClient();

    public async Task<IEnumerable<StoryDto>> GetTopStoriesAsync(int limit)
    {
        try
        {            
            bool isFromCache = true;
            logger.LogInformation("[CACHE MISS] Request received to fetch top {Limit} stories from Hacker News.", limit);
            if (limit < 1 || limit > 50)
            {
                logger.LogWarning("Invalid limit {Limit} provided. Defaulting to 10.", limit);
                limit = 10; // Enforce reasonable boundaries for query parameters
            }
            // 1. Fetch or Cache the list of Top Story IDs (cached for 2 minutes, check ApplicationSettings for duration)
            var storyIds = await cache.GetOrCreateAsync(CacheKey, async entry =>
            {
                isFromCache = false;
                logger.LogInformation("Fetching top stories from Hacker News API and caching the result.");
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(topStoriesCacheDurationInMinutes);
                var response = await _httpClient.GetFromJsonAsync<List<int>>($"{BaseUrl}topstories.json");
                logger.LogInformation("Fetched {Count} top stories from Hacker News API.", response?.Count ?? 0);
                return response ?? [];
            });

            if (isFromCache)
            {
                logger.LogInformation("[CACHE HIT] Top stories fetched from cache.");
            }   

            if (storyIds == null || !storyIds.Any()) return [];

            // 1. Fetch details for all stories concurrently to prevent bottlenecking
            var tasks = storyIds.Select(GetStoryDetailsAsync);
            var stories = await Task.WhenAll(tasks);

            // 2. Order by score and take only the number of requested IDs
            var orderedStories = stories.OrderByDescending(x => x.Score).Take(limit);

            // Filter out nulls (failed requests) and return cleanly mapped DTOs
            return orderedStories.Where(s => s != null)!;      
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "❌ Error in service while fetching top stories from Hacker News API: {Message}", ex.Message);
            throw;
        }
    }

    /// <summary>
    /// Fetches the details of a single story by its ID, utilizing caching to minimize API calls. Individual stories are cached for 10 minutes, as their details rarely change after being posted. If the story cannot be fetched or is invalid, the method returns null, allowing the calling method to handle it gracefully without breaking the entire list of top stories.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    private async Task<StoryDto?> GetStoryDetailsAsync(int id)
    {
        string itemCacheKey = $"Story_{id}";
        bool isFromCache = true;
        // Cache individual stories for 10 minutes (check ApplicationSettings for duration) since old stories rarely change details
        var story = await cache.GetOrCreateAsync(itemCacheKey, async entry =>
        {
            isFromCache = false;
            //logger.LogInformation("Fetching story details for ID {StoryId} from Hacker News API and caching the result.", id);
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
                logger.LogWarning("Failed to fetch story details for ID {StoryId} from Hacker News API.", id);
            }
        });
        if (isFromCache)
        {
            //logger.LogInformation("Story details fetched from cache ID {StoryId}.", id);
        }
        return story;
    }
}
    