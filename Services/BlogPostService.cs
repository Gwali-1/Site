using Dapper;
using Microsoft.Extensions.Logging;
using Site.Models;
using Swytch.App;
using Swytch.Structures;

namespace Site.Services;

public interface IBlogPostService
{
    Task<List<BlogPost>> GetBlogPostsAsync();
    Task<BlogPost> GetBlogPostAsync(string slug);
}

public class BlogPostService : IBlogPostService
{
    private readonly ILogger<BlogPostService> _logger;
    private readonly ISwytchApp _app;

    public BlogPostService(ILogger<BlogPostService> logger, ISwytchApp swychApp)
    {

        _logger = logger;
        _app = swychApp;

    }



    public async Task<List<BlogPost>> GetBlogPostsAsync()
    {
        try
        {
            using var dbcontext = _app.GetConnection(DatabaseProviders.SQLite);
            string query = "SELECT Title, Tags, Date, Slug FROM BlogPosts ORDER BY Date DESC";

            var result = await dbcontext.QueryAsync<BlogPost>(query);
            return result.AsList();
        }
        catch (System.Exception e)
        {
            _logger.LogError(e, "Exception occured when fetching blog posts");
            return new List<BlogPost>();
        }
    }
    public async Task<BlogPost> GetBlogPostAsync(string slug)
    {
        try
        {
            using var dbcontext = _app.GetConnection(DatabaseProviders.SQLite);
            string query = "SELECT Title, Tags, Date, Content FROM BlogPosts Where Slug  = @Slug LIMIT 1";

            var result = await dbcontext.QueryFirstOrDefaultAsync<BlogPost>(query, new { Slug = slug });
            return result is null ? new BlogPost() : result;

        }
        catch (System.Exception e)
        {
            _logger.LogError(e, "Exception occured when fetching a blog post");
            return new BlogPost();
        }
    }


    //get a blog content
    //
    //
    //


    //insert a blog


}

