using Dapper;
using Microsoft.Extensions.Logging;
using Site.Models;
using Swytch.App;
using Swytch.Structures;
using Site.Services;

public class BlogPostService : IBlogPostService
{
    private readonly ILogger<BlogPostService> _logger;
    private readonly ISwytchApp _app;

    public BlogPostService(ILogger<BlogPostService> logger, ISwytchApp swychApp)
    {

        _logger = logger;
        _app = swychApp;

    }



    //get all blogs
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


    //get a particular blog
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



    //insert a blog
    public async Task<bool> InsertBlogPostAsync(BlogPost blogPost)
    {
        try
        {
            using var dbcontext = _app.GetConnection(DatabaseProviders.SQLite);
            string query = "INSERT INTO BlogPosts (Title, Tags, Date, Content, Slug) VALUES (@Title, @Tags, @Date, @Content, @Slug)";

            var result = await dbcontext.ExecuteAsync(
                query,
                new { Title = blogPost.Title, Tags = blogPost.Title, Content = blogPost.Content, Slug = blogPost.Slug, Date = blogPost.Date });
            return 1 == result;

        }
        catch (System.Exception e)
        {
            _logger.LogError(e, "Exception occured when inserting a blog post");
            return false;
        }
    }


}

