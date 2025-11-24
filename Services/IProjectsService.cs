using Site.Models;

namespace Site.Services;

public interface IProjectsService
{
    Task<IReadOnlyList<Project>> GetProjectsAsync();
    Task<bool> InsertProjectAsync(Project project);
    Task<bool> UpdateProjectAsync(Project project);
}
