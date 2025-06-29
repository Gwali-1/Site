using Site.Models;

namespace Site.Services;

public interface IBlogPostService
{
    Task<List<BlogPost>> GetBlogPostsAsync();
    Task<BlogPost> GetBlogPostAsync(string slug);
    Task<bool> InsertBlogPostAsync(BlogPost blogPost);
}
