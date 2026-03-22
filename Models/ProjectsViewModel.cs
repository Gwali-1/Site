namespace Site.Models;

public record ProjectsViewModel : BaseViewModel
{
    public IReadOnlyList<Project> Projects { get; set; } = new List<Project>();
}
