using ApiHackersNews.DataModel;

namespace ApiHackersNews.Services;

// interface for the Hacker News service, defining the contract for fetching top stories. This interface allows for dependency injection and easier testing of the service.
public interface IHackerNewsService
{
    /// <summary>
    /// Fetches the top stories from Hacker News, ordered by score, and returns a limited number of stories as DTOs. The service utilizes caching to minimize API calls and improve performance.
    /// The list of top story IDs is cached for 2 minutes, while individual story details are cached for 10 minutes. This approach ensures that the service remains responsive and efficient, even under high load or when the Hacker News API is slow to respond.
    /// </summary>
    /// <param name="limit"></param>
    /// <returns></returns>
    Task<IEnumerable<StoryDto>> GetTopStoriesAsync(int limit);
}