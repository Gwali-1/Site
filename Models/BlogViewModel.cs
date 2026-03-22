namespace Site.Models;

public record BlogViewModel : BaseViewModel
{
    public IReadOnlyList<BlogPost> Blogs { get; set; } = new List<BlogPost>();
}
