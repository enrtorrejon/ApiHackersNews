namespace ApiHackersNews.DataModel;

/// <summary>
/// Represents a Hacker News item fetched from the API. This record is used to deserialize JSON responses from the Hacker News API.
/// </summary>
/// <param name="Title"></param>
/// <param name="Url"></param>
/// <param name="By"></param>
/// <param name="Time"></param>
/// <param name="Score"></param>
/// <param name="Descendants"></param>
public record HackerNewsItem(
    string Title, 
    string Url, 
    string By, 
    long Time, 
    int Score, 
    int Descendants
);