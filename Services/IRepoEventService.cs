namespace Site.Services;

public interface IRepoEventService
{
    Task HandleAddedBlogAsync(string filePath, string branch);
    Task HandleAddedProjectAsync(string filePath, string branch);
    Task HandleModifiedBlogAsync(string filePath, string branch);
    Task HandleModifiedProjectAsync(string filePath, string branch);
}
