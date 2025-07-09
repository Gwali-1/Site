namespace Site.Services;

public interface IRepoEventService
{
    Task HandleAddedAsync(string filePath, string branch);
    Task HandleModifiedBlogAsync(string filePath, string branch);
}
