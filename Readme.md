# ApiHackersNews

An optimized and highly scalable ASP.NET Core Minimal API that integrates with the Hacker News API. This project demonstrates clean code architectural practices by decoupling endpoint routing from `Program.cs`, implementing non-blocking asynchronous logging using Serilog, and employing memory caching strategies to reduce downstream API load.

## 🚀 Key Features

- **Decoupled Architecture:** Endpoints are extracted from `Program.cs` into distinct structural modules using `IEndpointRouteBuilder` extension methods.
- **Asynchronous File Logging:** Powered by **Serilog** and `Serilog.Sinks.Async` to drop file I/O overhead down to zero, ensuring maximum HTTP throughput.
- **Fully Data-Driven Configuration:** Logging targets, rollover intervals, and minimum retention levels are managed directly via `appsettings.json`.
- **Smart Memory Caching:** Implements `IMemoryCache` via `GetOrCreateAsync` with explicit telemetry trackers to easily identify cache hits and misses in production.
- **Defensive Parameter Control:** Enforces strict boundary checks on dynamic path inputs (`limit`) before kicking off pipeline routines.

---

## ⚡ Performance & Optimization Decisions

### 🚨 Deactivation of Story Details Logging
During development and load testing, we identified a performance bottleneck caused by high-frequency logging loops. 
* **Action Taken:** Logging has been explicitly **deactivated / commented out** for the individual story details retrieval process (`GetStoryDetails`).
* **Rationale:** Writing log entries for dozens of parallel child tasks (fetching metadata for every individual story ID) severely degraded API throughput and increased CPU overhead. Removing these verbose entries allows the application to process story lists instantly without being bottlenecked by memory or disk logging contention.

---

## 🛠️ Prerequisites

Make sure you have the following installed on your machine:
- **.NET 10.0 SDK** (or later)
- A text editor or IDE (e.g., VS Code, Visual Studio, JetBrains Rider)

---

## 📦 Project Dependencies

The project relies on the following NuGet packages for its advanced routing and logging capabilities:

```bash
dotnet add package Serilog.AspNetCore
dotnet add package Serilog.Sinks.Async
```

---

## ⚙️ Configuration (`appsettings.json`)

All runtime variables and logging parameters are central and declared declaratively inside the configuration file.

```json
{
  "AllowedHosts": "*",
  "HackerNewsApi": {
    "BaseUrl": "https://firebaseio.com"
  },
  "Serilog": {
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "Microsoft.AspNetCore": "Warning"
      }
    },
    "WriteTo": [
      { "Name": "Console" },
      {
        "Name": "Async",
        "Args": {
          "configure": [
            {
              "Name": "File",
              "Args": {
                "path": "Logs/log-.txt",
                "rollingInterval": "Day"
              }
            }
          ]
        }
      }
    ]
  }
}
```

---

## 🏢 Architecture Overview

### 1. Program.cs
The initialization pipeline boots up Serilog configurations explicitly tied to your JSON files, registers required dependencies via standard scopes, and clean-maps endpoints without cluttering the runtime entrypoint.

```csharp
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger();

builder.Host.UseSerilog();
// ... Core services registration ...
var app = builder.Build();
app.MapTopStories(); // Clean extension method routing
```

### 2. Endpoints/TopStories.cs
The endpoint is cleanly decoupled using a static extension class, relying on proper framework resolution to inject your dependencies without ad-hoc parameter guesswork.

```csharp
public static class TopStories
{
    public static void MapTopStories(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/top-stories/{limit}", async (int limit, [FromServices] IHackerNewsService hnService) =>
        {
            var stories = await hnService.GetTopStoriesAsync(limit);
            return Results.Ok(stories);
        });
    }
}
```

---

## 🏃 Getting Started

1. Clone or navigate to the source directory root:
   ```bash
   cd ApiHackersNews
   ```

2. Clean, restore, and spin up the runtime server:
   ```bash
   dotnet clean
   dotnet restore
   dotnet run
   ```

3. Trigger the endpoint via your browser, curl, or Postman:
   ```http
   GET http://localhost:5000/api/top-stories/10
   ```

---

## 📂 Telemetry and Logs Location

When running the application, Serilog will start populating files instantly. Go to your file manager or explorer and check the project folder root:

```text
ApiHackersNews/
│
├── Endpoints/
│   └── TopStories.cs
├── Logs/
│   └── log-20261102.txt  <-- Daily rolled log files generated here
├── Services/
│   ├── IHackerNewsService.cs
│   └── HackerNewsService.cs
├── Program.cs
└── appsettings.json
```

Inside the text logs, look for the following cache tracking keys to monitor performance metrics:
- `🌐 [CACHE MISS]` — Triggered only when memory data is missing; fires an out-of-band network thread to load live resources.
- `📦 [CACHE HIT]` — Instant runtime retrieval from local RAM. 