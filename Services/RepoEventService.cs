using System.Text.Json;
using System.Text.RegularExpressions;
using Flurl.Http;
using Microsoft.Extensions.Logging;
using Site.Models;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Site.Services
{
    public class RepoEventService : IRepoEventService
    {
        private readonly ILogger<RepoEventService> _logger;
        private readonly IBlogPostService _blogPostService;
        private readonly IProjectsService _projectsService;
        private const string Owner = "Gwali-1";
        private const string Repo = "Blog-Files";

        public RepoEventService(
            ILogger<RepoEventService> logger,
            IBlogPostService blogPostService,
            IProjectsService projectsService
        )
        {
            _logger = logger;
            _blogPostService = blogPostService;
            _projectsService = projectsService;
        }

        public async Task HandleAddedBlogAsync(string filePath, string branch)
        {
            _logger.LogInformation(
                "Handling added file: {FilePath} in branch: {Branch}",
                filePath,
                branch
            );
            try
            {
                //get content
                var stringContent = await GetFileContentFromGithub(filePath, branch);
                //parse the content
                var blogPost = ParseFrontMatter(stringContent);
                //insert into db
                var inserted = await _blogPostService.InsertBlogPostAsync(blogPost);
                if (!inserted)
                {
                    _logger.LogInformation("Could not add new blog post: {Title}", blogPost.Title);
                    return;
                }
                _logger.LogInformation("Successfully added blog post: {Title}", blogPost.Title);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling added file: {FilePath}", filePath);
            }
        }

        public async Task HandleModifiedBlogAsync(string filePath, string branch)
        {
            _logger.LogInformation(
                "Handling modified file: {FilePath} in branch: {Branch}",
                filePath,
                branch
            );
            try
            {
                //get content
                var stringContent = await GetFileContentFromGithub(filePath, branch);
                //parse the content
                var blogPost = ParseFrontMatter(stringContent);
                //insert into db
                var updated = await _blogPostService.UpdateBlogPostAsync(blogPost);
                if (!updated)
                {
                    _logger.LogInformation("Could not modify blog post: {Title}", blogPost.Title);
                    return;
                }
                _logger.LogInformation("Successfully modified blog post: {Title}", blogPost.Title);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling modified file: {FilePath}", filePath);
            }
        }

        public async Task HandleAddedProjectAsync(string filePath, string branch)
        {
            _logger.LogInformation(
                "Handling added project: {FilePath} in branch: {Branch}",
                filePath,
                branch
            );
            try
            {
                var stringContent = await GetFileContentFromGithub(filePath, branch);

                var project = ParseProjectFile(stringContent);
                if (string.IsNullOrEmpty(project.Name))
                {
                    _logger.LogInformation("Could not parse the project file");
                    return;
                }

                var inserted = await _projectsService.InsertProjectAsync(project);
                if (!inserted)
                {
                    _logger.LogInformation("Could not add new project: {Name}", project.Name);
                    return;
                }

                _logger.LogInformation("Successfully added project: {Name}", project.Name);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling added project: {FilePath}", filePath);
            }
        }

        public async Task HandleModifiedProjectAsync(string filePath, string branch)
        {
            _logger.LogInformation(
                "Handling modified project: {FilePath} in branch: {Branch}",
                filePath,
                branch
            );
            try
            {
                var stringContent = await GetFileContentFromGithub(filePath, branch);

                var project = ParseProjectFile(stringContent);
                if (string.IsNullOrEmpty(project.Name))
                {
                    _logger.LogInformation("Could not parse the project file");
                    return;
                }

                var inserted = await _projectsService.UpdateProjectAsync(project);
                if (!inserted)
                {
                    _logger.LogInformation("Error handling modified project: {Name}", project.Name);
                    return;
                }

                _logger.LogInformation("Successfully modified project: {Name}", project.Name);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling modified project: {FilePath}", filePath);
            }
        }

        private async Task<string> GetFileContentFromGithub(string filePath, string branch)
        {
            var rawUrl = $"https://raw.githubusercontent.com/{Owner}/{Repo}/{branch}/{filePath}";

            var fileContent = await rawUrl.GetStringAsync();

            return fileContent;
        }

        private BlogPost ParseFrontMatter(string fileContent)
        {
            var match = Regex.Match(
                fileContent,
                @"^\s*---\s*\r?\n([\s\S]*?)\r?\n---\s*\r?\n([\s\S]*)$",
                RegexOptions.Singleline
            );
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
                    Slug = metaData.Slug,
                };

                return newBlogPost;
            }
            throw new Exception("Could not parse front matter");
        }

        private Project ParseProjectFile(string fileContent)
        {
            var project = JsonSerializer.Deserialize<Project>(
                fileContent,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
            );
            return project ?? new Project();
        }
    }
}
