using Dapper;
using Microsoft.Extensions.Logging;
using Site.Models;
using Swytch.App;
using Swytch.Structures;

namespace Site.Services;

public class ProjectsService : IProjectsService
{
    private readonly ILogger<ProjectsService> _logger;
    private readonly ISwytchApp _app;

    public ProjectsService(ILogger<ProjectsService> logger, ISwytchApp swychApp)
    {
        _logger = logger;
        _app = swychApp;
    }

    public async Task<IReadOnlyList<Project>> GetProjectsAsync()
    {
        try
        {
            using var dbcontext = _app.GetConnection(DatabaseProviders.SQLite);
            string query = "SELECT Id, Name, Description, Url, Image FROM Projects";

            var result = await dbcontext.QueryAsync<Project>(query);
            return result.AsList();
        }
        catch (System.Exception e)
        {
            _logger.LogError(e, "Exception occured when fetching projects");
            return new List<Project>();
        }
    }

    public async Task<bool> InsertProjectAsync(Project project)
    {
        try
        {
            using var dbcontext = _app.GetConnection(DatabaseProviders.SQLite);
            string query =
                "INSERT INTO Projects (Name, Description, Url, Image) VALUES (@Name, @Description, @Url, @Image)";

            var result = await dbcontext.ExecuteAsync(
                query,
                new
                {
                    Name = project.Name,
                    Description = project.Description,
                    Url = project.Url,
                    Image = project.Image,
                }
            );

            return 1 == result;
        }
        catch (System.Exception e)
        {
            _logger.LogError(e, "Exception occured when inserting a project in project table");
            return false;
        }
    }

    //update a prject
    public async Task<bool> UpdateProjectAsync(Project project)
    {
        try
        {
            using var dbcontext = _app.GetConnection(DatabaseProviders.SQLite);
            string query =
                "UPDATE Projects SET Url = @Url, Image =@Image, Description = @Description WHERE Name= @Name";

            var result = await dbcontext.ExecuteAsync(
                query,
                new
                {
                    Url = project.Url,
                    Image = project.Image,
                    Description = project.Description,
                    Name = project.Name,
                }
            );
            return 1 == result;
        }
        catch (System.Exception e)
        {
            _logger.LogError(e, "Exception occured when updating a  project");
            return false;
        }
    }
}
