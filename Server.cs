// This template file is auto-generated.
// GitHub Repository:https://github.com/Gwali-1/Swytch.git

using System.Net;
using System.Text.Json;
using Markdig;
using Markdig.Extensions.AutoIdentifiers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Site.Helpers;
using Site.Models;
using Site.Services;
using Swytch.App;
using Swytch.Extensions;
using Swytch.Structures;

ISwytchApp swytchApp = new SwytchApp(
    new SwytchConfig
    {
        EnableStaticFileServer = true,
        StaticCacheMaxAge = "1",
        PrecompileTemplates = true,
    }
);

//Enable request logging
swytchApp.AddLogging();

//Add datastore
swytchApp.AddDatastore("Data Source=blog.db; foreign keys=true", DatabaseProviders.SQLite);

//Set up service container
ServiceCollection serviceContainer = new ServiceCollection();

//Register services here
serviceContainer.AddSingleton<ISwytchApp>(swytchApp);
serviceContainer.AddScoped<IBlogPostService, BlogPostService>();
serviceContainer.AddScoped<IProjectsService, ProjectsService>();
serviceContainer.AddScoped<IRepoEventService, RepoEventService>();
serviceContainer.AddSingleton<IAuthenticationHelper, AuthenticationHelper>();

serviceContainer.AddLogging(builder =>
{
    builder.AddConsole();
    builder.SetMinimumLevel(LogLevel.Information);
});

//Build service provider
IServiceProvider serviceProvider = serviceContainer.BuildServiceProvider();

//Retrieving registered service
var logger = serviceProvider.GetRequiredService<ILogger<Program>>();

//configuration
var config = new ConfigurationBuilder()
    .AddJsonFile("config.json", reloadOnChange: true, optional: false)
    .Build();
var secret = config.GetValue<string>("Secret");

//Markdig
var pipeline = new MarkdownPipelineBuilder()
    .UseAutoIdentifiers(AutoIdentifierOptions.GitHub)
    .UseAdvancedExtensions()
    .Build();

//Routes and action registration
swytchApp.AddAction(
    "GET",
    "/",
    async (context) =>
    {
        using var scope = serviceProvider.CreateScope();
        var blogPostService = scope.ServiceProvider.GetRequiredService<IBlogPostService>();
        var projectsService = scope.ServiceProvider.GetRequiredService<IProjectsService>();

        var allBlogs = await blogPostService.GetBlogPostsAsync();
        var blogs = allBlogs.Take(3).ToList(); //this is bad - should have filtered in db

        var allProjects = await projectsService.GetProjectsAsync();
        var projects = allProjects.Take(3).ToList();

        var dataContext = new HomeDataContext { Blogs = blogs, Projects = projects };

        var e = await swytchApp.GenerateTemplate("Home", dataContext);
        Console.WriteLine(e);

        var htmxHeader = context.Request.Headers["HX-Request"];
        if (string.IsNullOrEmpty(htmxHeader))
        {
            await swytchApp.RenderTemplate<object>(context, "Home", dataContext);
            return;
        }

        var homeContent = await swytchApp.GenerateTemplate<HomeDataContext>(
            "HomeFragment",
            dataContext
        );
        await context.WriteTextToStream(homeContent, HttpStatusCode.OK);
    }
);
swytchApp.AddAction(
    "GET",
    "/blog",
    async (context) =>
    {
        using var scope = serviceProvider.CreateScope();

        var blogPostService = scope.ServiceProvider.GetRequiredService<IBlogPostService>();
        var allBlogs = await blogPostService.GetBlogPostsAsync();

        var htmxHeader = context.Request.Headers["HX-Request"];
        if (string.IsNullOrEmpty(htmxHeader))
        {
            await swytchApp.RenderTemplate<object>(context, "Blog", allBlogs);
            return;
        }

        var allBlogsContent = await swytchApp.GenerateTemplate<IReadOnlyList<BlogPost>>(
            "BlogFragment",
            allBlogs
        );

        await context.WriteTextToStream(allBlogsContent, HttpStatusCode.OK);
    }
);

swytchApp.AddAction(
    "GET",
    "/post/{slug}",
    async (context) =>
    {
        logger.LogInformation("Blog Post Request");

        using var scope = serviceProvider.CreateScope();
        var blogPostService = scope.ServiceProvider.GetRequiredService<IBlogPostService>();

        var blogPost = await blogPostService.GetBlogPostAsync(context.PathParams["slug"]);

        var mkdToHtml = Markdown.ToHtml(blogPost.Content, pipeline);
        blogPost.Content = mkdToHtml;

        var htmxHeader = context.Request.Headers["HX-Request"];
        if (string.IsNullOrEmpty(htmxHeader))
        {
            await swytchApp.RenderTemplate<object>(context, "BlogView", blogPost);
            return;
        }

        var blogPostView = await swytchApp.GenerateTemplate<BlogPost>("BlogViewFragment", blogPost);
        await context.WriteTextToStream(blogPostView, HttpStatusCode.OK);
    }
);

swytchApp.AddAction(
    "GET",
    "/projects",
    async (context) =>
    {
        using var scope = serviceProvider.CreateScope();

        var projectsService = scope.ServiceProvider.GetRequiredService<IProjectsService>();
        var allProjects = await projectsService.GetProjectsAsync();

        var htmxHeader = context.Request.Headers["HX-Request"];
        if (string.IsNullOrEmpty(htmxHeader))
        {
            await swytchApp.RenderTemplate<object>(context, "Projects", allProjects);
            return;
        }
        var projectsView = await swytchApp.GenerateTemplate<IReadOnlyList<Project>>(
            "ProjectsFragment",
            allProjects
        );
        await context.WriteTextToStream(projectsView, HttpStatusCode.OK);
    }
);

swytchApp.AddAction(
    "GET",
    "/about",
    async (context) =>
    {
        var htmxHeader = context.Request.Headers["HX-Request"];
        if (string.IsNullOrEmpty(htmxHeader))
        {
            await swytchApp.RenderTemplate<object>(context, "About", null);
            return;
        }
        var AboutView = await swytchApp.GenerateTemplate<object>("AboutFragment", null!);
        await context.WriteTextToStream(AboutView, HttpStatusCode.OK);
    }
);

swytchApp.AddAction(
    "POST",
    "/repoevent",
    async (context) =>
    {
        logger.LogInformation("Received a repo webhook event from github");

        //authenticate the request is from github
        logger.LogInformation("Authenticating the request");

        var hashHeader = context.Request.Headers["X-Hub-Signature-256"];
        if (string.IsNullOrEmpty(hashHeader))
        {
            logger.LogInformation(
                "X-Hub-Signature-256 header is missing from request, ending process"
            );
            return;
        }

        if (string.IsNullOrEmpty(secret))
        {
            logger.LogInformation("Secret value from config is null or empty, ending processs");
            return;
        }

        var requestBody = context.ReadJsonBody();

        //compute hash
        var authHelper = serviceProvider.GetRequiredService<IAuthenticationHelper>();
        var computedHash = authHelper.GetHashofBody(secret, requestBody);

        //compare the hashes
        var headerHashSignature = authHelper.StringToByteArray(hashHeader.Split("=")[1]);
        var isAuthenticatedRequest = authHelper.CompareHashes(headerHashSignature, computedHash);

        if (!isAuthenticatedRequest)
        {
            logger.LogInformation(
                "Webhook request not authenticated as request from github, ending process"
            );
            return;
        }

        logger.LogInformation("Event authenticated successfully, will proceed to handle");

        //handle event
        var eventBody = JsonSerializer.Deserialize<GitHubPushEvent>(requestBody);
        if (eventBody?.Commits is null)
        {
            logger.LogInformation("Commits in repo event is null, ending process");
            return;
        }

        using var scope = serviceProvider.CreateScope();
        var repoService = scope.ServiceProvider.GetRequiredService<IRepoEventService>();

        var commit = eventBody.Commits[0];
        var added = commit.Added.Count == 0 ? null : commit.Added[0];
        var modified = commit.Modified.Count == 0 ? null : commit.Modified[0];

        //added post
        if (added is not null && added.StartsWith("Posts/"))
        {
            logger.LogInformation("Adding new blog entry => {name}", added);
            await repoService.HandleAddedBlogAsync(added, "main");
        }

        //modified post
        if (modified is not null && modified.StartsWith("Posts/"))
        {
            logger.LogInformation("Updating content of existing blog entry => {name}", modified);
            await repoService.HandleModifiedBlogAsync(modified, "main");
        }

        //added project
        if (added is not null && added.StartsWith("Projects/"))
        {
            logger.LogInformation("Adding new  project entry entry => {name}", modified);
            await repoService.HandleAddedProjectAsync(added, "main");
        }

        //modified project
        if (modified is not null && modified.StartsWith("Projects/"))
        {
            logger.LogInformation("Updating details of existing project => {name}", modified);
            await repoService.HandleModifiedProjectAsync(modified, "main");
        }

        logger.LogInformation("Done handling repo event hook from github");
    }
);

//create tables
DatabaseHelper.CreateTablesIfNotExist(swytchApp);
DatabaseHelper.InsertSampleTestDataInDatabase(swytchApp);

//Start app
await swytchApp.Listen("http://+:8080/");
