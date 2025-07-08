using System.Text.Json.Serialization;

namespace Site.Models;

public record BlogPost
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Tags { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string Date { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
}

public record MetaData
{

    [JsonPropertyName("date")]
    public string Date { get; set; } = string.Empty;
    [JsonPropertyName("tags")]
    public List<string> Tags { get; set; } = new List<string>();
    [JsonPropertyName("slug")]
    public string Slug { get; set; } = string.Empty;
    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;
}

