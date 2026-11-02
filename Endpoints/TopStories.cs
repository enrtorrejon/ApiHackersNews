using Microsoft.AspNetCore.Mvc;
using ApiHackersNews.Services;

namespace ApiHackersNews.Endpoints;
// Map the Endpoint using .NET 10 Minimal APIs
public static class TopStories
{
    public static void MapTopStories(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/top-stories/{limit}", async (int limit,  IHackerNewsService hnService) =>
        {

        // Enforce reasonable boundaries for query parameters
        if (limit is < 1 or > 50) limit = 10;

        try
        {
            var stories = await hnService.GetTopStoriesAsync(limit);
            return Results.Ok(stories);
        }
        catch (Exception ex)
        {
            // Real applications should use structured ILogger logging here
            return Results.Problem($"Failed to fetch data from Hacker News: {ex.Message}");
        }
        });
  }
}
