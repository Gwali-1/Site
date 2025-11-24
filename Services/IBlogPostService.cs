using Site.Models;

namespace Site.Services;

public interface IBlogPostService
{
    Task<IReadOnlyList<BlogPost>> GetBlogPostsAsync();
    Task<BlogPost> GetBlogPostAsync(string slug);
    Task<bool> InsertBlogPostAsync(BlogPost blogPost);
    Task<bool> UpdateBlogPostAsync(BlogPost blogPost);
}
