// This template file is auto-generated.
// GitHub Repository:https://github.com/Gwali-1/Swytch.git 

using System.Net;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Site.Helpers;
using Site.Models;
using Site.Services;
using Swytch.App;
using Swytch.Extensions;
using Swytch.Structures;


ISwytchApp swytchApp = new SwytchApp(new SwytchConfig
{
    EnableStaticFileServer = true,
    StaticCacheMaxAge = "10"
});

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






//Routes and action registration
swytchApp.AddAction("GET", "/", async (context) =>
{
    await context.ServeFile("index.html", HttpStatusCode.OK);
});



swytchApp.AddAction("GET", "/posts", async (context) =>
{
    logger.LogInformation("blog posts request");

    using var scope = serviceProvider.CreateScope();
    var blogPostService = scope.ServiceProvider.GetRequiredService<IBlogPostService>();

    var posts = await blogPostService.GetBlogPostsAsync();
    await context.ToOk(posts);
});



swytchApp.AddAction("GET", "/post/{slug}", async (context) =>
{

    logger.LogInformation("Blog Post Request");

    using var scope = serviceProvider.CreateScope();
    var blogPostService = scope.ServiceProvider.GetRequiredService<IBlogPostService>();

    var blogPost = await blogPostService.GetBlogPostAsync(context.PathParams["slug"]);
    await context.ToOk(blogPost);
});



swytchApp.AddAction("GET", "/projects", async (context) =>
{

    logger.LogInformation("Projects request");

    using var scope = serviceProvider.CreateScope();
    var projectService = scope.ServiceProvider.GetRequiredService<IProjectsService>();

    var projects = await projectService.GetProjectsAsync();
    await context.ToOk(projects);
});



swytchApp.AddAction("POST", "/project", async (context) =>
{
    logger.LogInformation("Add Project Request");

    using var scope = serviceProvider.CreateScope();
    var projectService = scope.ServiceProvider.GetRequiredService<IProjectsService>();

    var newProject = context.ReadJsonBody<Project>();
    logger.LogInformation("deserialized => {req}", JsonSerializer.Serialize(newProject));

    await projectService.InsertProjectAsync(newProject!);
    await context.ToOk(new { message = "Project added successfully" });
});



swytchApp.AddAction("POST", "/post", async (context) =>
{
    logger.LogInformation("Add blog post Request");

    using var scope = serviceProvider.CreateScope();
    var blogService = scope.ServiceProvider.GetRequiredService<IBlogPostService>();


    var newBlog = context.ReadJsonBody<BlogPost>();
    logger.LogInformation("deserialized => {req}", JsonSerializer.Serialize(newBlog));

    await blogService.InsertBlogPostAsync(newBlog!);
    await context.ToOk(new { message = "Blog post added successfully" });
});



swytchApp.AddAction("POST", "/repoevent", async (context) =>
{
    await context.WriteTextToStream("repoevent", HttpStatusCode.OK);
});





//create tables
DatabaseHelper.CreateTablesIfNotExist(swytchApp);
DatabaseHelper.InsertSampleTestDataInDatabase(swytchApp);

//Start app
await swytchApp.Listen();
