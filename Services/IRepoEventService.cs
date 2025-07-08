namespace Site.Services;

public interface IRepoEventService
{
    Task HandleAddedAsync(string filePath, string branch);
    Task HandleModifiedAsync(string filePath, string branch);
}
