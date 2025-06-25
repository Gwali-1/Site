namespace Site.Models;

public class BlogPost
{
    public int Id { get; set; }             // Primary key
    public string Title { get; set; } = string.Empty;
    public string Tags { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string Date { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
}

