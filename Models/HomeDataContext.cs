namespace Site.Models;

public record HomeDataContext : BaseViewModel
{
    public List<BlogPost> Blogs { get; set; } = new();
    public List<Project> Projects { get; set; } = new();
}
