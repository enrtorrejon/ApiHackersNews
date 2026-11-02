using ApiHackersNews.Services;
using ApiHackersNews.Endpoints;
using Serilog; 

var builder = WebApplication.CreateBuilder(args);

// 🌟 CONFIGURING SERILOG: Instructs .NET to output logs both to the Console and to a text file.
// A new log file will be generated automatically every day inside the "Logs" folder.
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger();

builder.Host.UseSerilog();

Log.Information("The application services are being configured...");

// Register Core Services
builder.Services.AddHttpClient();
builder.Services.AddMemoryCache();

// Register our typed Hacker News Service
builder.Services.AddScoped<IHackerNewsService, HackerNewsService>();

var app = builder.Build();

Log.Information("The HTTP pipeline is ready. Starting the host on port local...");

app.MapTopStories(); // Map the Top Stories endpoint

// Cleanly close and flush logs when the application stops
app.Lifetime.ApplicationStopping.Register(() => 
{
    Log.Warning("The application is shutting down...");
    Log.CloseAndFlush();
});

app.Run();




