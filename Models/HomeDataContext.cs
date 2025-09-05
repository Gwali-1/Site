
namespace Site.Models;

public record HomeDataContext
{
    public List<BlogPost> Blogs { get; set; } = new();
    public List<Project> Projects { get; set; } = new();
}
