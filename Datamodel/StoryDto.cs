using System.Text.Json.Serialization;

namespace ApiHackersNews.DataModel;

// --- Data Models (Using C# Records for immutability) ---
/// <summary>
/// Represents a simplified Hacker News story DTO (Data Transfer Object) used for API responses. This record is designed to be immutable and provides a clean structure for the data returned by the Hacker News service.
/// </summary>
/// <param name="Title"></param>
/// <param name="Uri"></param>
/// <param name="PostedBy"></param>
/// <param name="Time"></param>
/// <param name="Score"></param>
/// <param name="CommentCount"></param>
public record StoryDto(
    [property: JsonPropertyName("title")] string Title, 
    [property: JsonPropertyName("uri")] string Uri, 
    [property: JsonPropertyName("postedBy")] string PostedBy, 
    [property: JsonPropertyName("time")] DateTime Time, 
    [property: JsonPropertyName("score")] int Score, 
    [property: JsonPropertyName("commentCount")] int CommentCount
);
