
using System.Text.Json.Serialization;

public class GitHubPushEvent
{
    [JsonPropertyName("ref")]
    public string? Ref { get; set; }
    [JsonPropertyName("repository")]
    public Repository? Repository { get; set; }
    [JsonPropertyName("commits")]
    public List<Commit>? Commits { get; set; }
}

public class Repository
{
    [JsonPropertyName("full_name")]
    public string? Full_Name { get; set; }
    [JsonPropertyName("name")]
    public string? Name { get; set; }
}

public class Commit
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }
    [JsonPropertyName("added")]
    public List<string> Added { get; set; } = new List<string>();
    [JsonPropertyName("modified")]
    public List<string> Modified { get; set; } = new List<string>();
    [JsonPropertyName("removed")]
    public List<string> Removed { get; set; } = new List<string>();
}




