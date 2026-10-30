using ApiHackersNews.Services;

var builder = WebApplication.CreateBuilder(args);

// Register Core Services
builder.Services.AddHttpClient();
builder.Services.AddMemoryCache();

// Register our typed Hacker News Service
builder.Services.AddScoped<HackerNewsService>();

var app = builder.Build();

// Map the Endpoint using .NET 10 Minimal APIs
app.MapGet("/api/top-stories/{limit}", async (HackerNewsService hnService, int limit) =>
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

app.Run();




