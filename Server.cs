// This template file is auto-generated.
// GitHub Repository:https://github.com/Gwali-1/Swytch.git

using System.Net;
using Markdig;
using Markdig.Extensions.AutoIdentifiers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
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
        var blogs = allBlogs.Take(3).ToList(); //this is bad - should have filtered in db but whatever

        var allProjects = await projectsService.GetProjectsAsync();
        var projects = allProjects.Take(3).ToList();

        var dataContext = new HomeDataContext { Blogs = blogs, Projects = projects };

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
    "POST",
    "/add-blog-metadata",
    async (context) =>
    {
        using var scope = serviceProvider.CreateScope();
        var blogPostService = scope.ServiceProvider.GetRequiredService<IBlogPostService>();

        // Read JSON body as MetaData
        var meta = context.ReadJsonBody<MetaData>();
        if (meta == null || string.IsNullOrWhiteSpace(meta.Slug))
        {
            await context.WriteTextToStream("Invalid metadata", HttpStatusCode.BadRequest);
            return;
        }

        var blogPost = new BlogPost
        {
            Title = meta.Title,
            Tags = string.Join(",", meta.Tags),
            Date = meta.Date,
            Slug = meta.Slug,
            Content = string.Empty // Content is stored in markdown file
        };

        var result = await blogPostService.InsertBlogPostAsync(blogPost);
        if (result)
            await context.ToOk("Blog Added");
        else
            await context.WriteTextToStream("Failed to add blog metadata", HttpStatusCode.InternalServerError);
    }
);
swytchApp.AddAction(
    "POST",
    "/update-blog-metadata",
    async (context) =>
    {
        using var scope = serviceProvider.CreateScope();
        var blogPostService = scope.ServiceProvider.GetRequiredService<IBlogPostService>();

        // Read JSON body as MetaData
        var meta = context.ReadJsonBody<MetaData>();
        if (meta == null || string.IsNullOrWhiteSpace(meta.Slug))
        {
            await context.ToInternalError("Failed To Add");
            return;
        }

        var blogPost = new BlogPost
        {
            Title = meta.Title,
            Tags = string.Join(",", meta.Tags),
            Date = meta.Date,
            Slug = meta.Slug,
            Content = string.Empty // Content is stored in markdown file
        };

        var result = await blogPostService.UpdateBlogPostAsync(blogPost);
        if (result)
            await context.ToOk("Updated");
        else
            await context.ToInternalError("Update Failed");
    }
);


swytchApp.AddAction(
    "GET",
    "/post/{slug}",
    async (context) =>
    {
        using var scope = serviceProvider.CreateScope();
        var blogPostService = scope.ServiceProvider.GetRequiredService<IBlogPostService>();

        var blogPost = await blogPostService.GetBlogPostFromDiskAsync(context.PathParams["slug"]);
        // If blog post is empty, redirect to home
        if (string.IsNullOrWhiteSpace(blogPost.Title) && string.IsNullOrWhiteSpace(blogPost.Content))
        {
            await context.ToRedirect("/");
            return;
        }

        blogPost.Content = Markdown.ToHtml(blogPost.Content, pipeline);

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

//Start app
await swytchApp.Listen("http://+:8080/");