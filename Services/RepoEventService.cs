
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using Site.Models;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using Flurl.Http;

namespace Site.Services
{
    public class RepoEventService : IRepoEventService
    {
        private readonly ILogger<RepoEventService> _logger;
        private readonly IBlogPostService _blogPostService;
        private const string Owner = "Gwali-1";
        private const string Repo = "Blog-Files";

        public RepoEventService(ILogger<RepoEventService> logger, IBlogPostService blogPostService)
        {
            _logger = logger;
            _blogPostService = blogPostService;
        }

        public async Task HandleAddedAsync(string filePath, string branch)
        {
            _logger.LogInformation("Handling added file: {FilePath} in branch: {Branch}", filePath, branch);
            try
            {
                //get content
                var stringContent = await GetBlogPostFromGithub(filePath, branch);
                //parse the content
                var blogPost = ParseFrontMatter(stringContent);
                //insert into db
                await _blogPostService.InsertBlogPostAsync(blogPost);
                _logger.LogInformation("Successfully added blog post: {Title}", blogPost.Title);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling added file: {FilePath}", filePath);
            }
        }

        public async Task HandleModifiedAsync(string filePath, string branch)
        {
            _logger.LogInformation("Handling modified file: {FilePath} in branch: {Branch}", filePath, branch);
            try
            {
                //get content
                var stringContent = await GetBlogPostFromGithub(filePath, branch);
                //parse the content
                var blogPost = ParseFrontMatter(stringContent);
                //insert into db
                await _blogPostService.InsertBlogPostAsync(blogPost);  //change to modified
                _logger.LogInformation("Successfully modified blog post: {Title}", blogPost.Title);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling modified file: {FilePath}", filePath);
            }
        }

        private async Task<string> GetBlogPostFromGithub(string filePath, string branch)
        {
            var rawUrl = $"https://raw.githubusercontent.com/{Owner}/{Repo}/{branch}/{filePath}";

            var fileContent = await rawUrl.GetStringAsync();

            return fileContent;

        }

        private BlogPost ParseFrontMatter(string fileContent)
        {
            var match = Regex.Match(fileContent, @"^---\s*\n(.*?)\n---\s*\n(.*)", RegexOptions.Singleline);
            if (match.Success)
            {
                var frontMatter = match.Groups[1].Value;
                var markdownBody = match.Groups[2].Value;


                var deserializer = new DeserializerBuilder()
                   .WithNamingConvention(CamelCaseNamingConvention.Instance)
                    .Build();

                var metaData = deserializer.Deserialize<MetaData>(frontMatter);

                var newBlogPost = new BlogPost
                {
                    Tags = string.Join(",", metaData.Tags),
                    Content = markdownBody,
                    Title = metaData.Title,
                    Date = metaData.Date,
                    Slug = metaData.Slug
                };

                return newBlogPost;

            }
            throw new Exception("Could not parse front matter");
        }
    }
}
