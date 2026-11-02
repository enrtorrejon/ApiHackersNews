using ApiHackersNews.Services;
using ApiHackersNews.Endpoints;

var builder = WebApplication.CreateBuilder(args);

// Register Core Services
builder.Services.AddHttpClient();
builder.Services.AddMemoryCache();

// Register our typed Hacker News Service
builder.Services.AddScoped<IHackerNewsService, HackerNewsService>();

var app = builder.Build();

app.MapTopStories(); // Map the Top Stories endpoint

app.Run();




