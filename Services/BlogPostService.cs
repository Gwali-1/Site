using Dapper;
using Microsoft.Extensions.Logging;
using Site.Models;
using Site.Services;
using Swytch.App;
using Swytch.Structures;

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
    public async Task<IReadOnlyList<BlogPost>> GetBlogPostsAsync()
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
            string query =
                "SELECT Title, Tags, Date, Content FROM BlogPosts Where Slug  = @Slug LIMIT 1";

            var result = await dbcontext.QueryFirstOrDefaultAsync<BlogPost>(
                query,
                new { Slug = slug }
            );
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
            string query =
                "INSERT INTO BlogPosts (Title, Tags, Date, Content, Slug) VALUES (@Title, @Tags, @Date, @Content, @Slug)";

            var result = await dbcontext.ExecuteAsync(
                query,
                new
                {
                    Title = blogPost.Title,
                    Tags = blogPost.Tags,
                    Content = blogPost.Content,
                    Slug = blogPost.Slug,
                    Date = blogPost.Date,
                }
            );
            return 1 == result;
        }
        catch (System.Exception e)
        {
            _logger.LogError(e, "Exception occured when inserting a blog post");
            return false;
        }
    }

    //update a blog
    public async Task<bool> UpdateBlogPostAsync(BlogPost blogPost)
    {
        try
        {
            using var dbcontext = _app.GetConnection(DatabaseProviders.SQLite);
            string query = "UPDATE BlogPosts SET Title = @Title, Tags = @Tags, Date = @Date, Slug = @Slug WHERE Slug = @Slug";

            var result = await dbcontext.ExecuteAsync(
                query,
                new {
                    Title = blogPost.Title,
                    Tags = blogPost.Tags,
                    Date = blogPost.Date,
                    Slug = blogPost.Slug
                }
            );
            return 1 == result;
        }
        catch (System.Exception e)
        {
            _logger.LogError(e, "Exception occured when updating a blog post");
            return false;
        }
    }
    
     // Fetch blog post from disk using JSON frontmatter
    public async Task<BlogPost> GetBlogPostFromDiskAsync(string slug)
    {
        // Directory where markdown posts are stored
        string postsDir = Path.Combine(AppContext.BaseDirectory, "Posts");
        // Prevent directory traversal
        string safeSlug = slug.Replace("..", "").Replace("/", "").Replace("\\", "");
        string filePath = Path.Combine(postsDir, safeSlug + ".md");
        // Ensure file is within postsDir
        if (!filePath.StartsWith(postsDir))
        {
            _logger.LogWarning($"Unsafe slug attempted: {slug}");
            return new BlogPost();
        }
        if (!File.Exists(filePath))
        {
            _logger.LogWarning($"Blog post file not found: {filePath}");
            return new BlogPost();
        }
        try
        {
            using var reader = new StreamReader(filePath);
            string line;
            string frontmatter = "";
            bool inFrontmatter = false;
            List<string> contentLines = new();
            while ((line = await reader.ReadLineAsync()) != null)
            {
                if (!inFrontmatter && line.Trim() == "---")
                {
                    inFrontmatter = true;
                    continue;
                }
                if (inFrontmatter && line.Trim() == "---")
                {
                    inFrontmatter = false;
                    continue;
                }
                if (inFrontmatter)
                {
                    frontmatter += line + "\n";
                }
                else
                {
                    contentLines.Add(line);
                }
            }
            BlogPost blogPost = new BlogPost();
            if (!string.IsNullOrWhiteSpace(frontmatter))
            {
                try
                {
                    var meta = System.Text.Json.JsonSerializer.Deserialize<MetaData>(frontmatter);
                    if (meta != null)
                    {
                        blogPost.Title = meta.Title;
                        blogPost.Tags = string.Join(",", meta.Tags);
                        blogPost.Date = meta.Date;
                        blogPost.Slug = meta.Slug;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Failed to parse JSON frontmatter for {slug}");
                }
            }
            blogPost.Content = string.Join("\n", contentLines);
            return blogPost;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error reading blog post file: {filePath}");
            return new BlogPost();
        }
    }
}
