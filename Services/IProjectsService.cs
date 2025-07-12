using Site.Models;

namespace Site.Services;

public interface IProjectsService
{
    Task<List<Project>> GetProjectsAsync();
    Task<bool> InsertProjectAsync(Project project);
    Task<bool> UpdateProjectAsync(Project project);
}
