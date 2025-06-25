// This template file is auto-generated.
// GitHub Repository:https://github.com/Gwali-1/Swytch.git 

using System.Net;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Site.Helpers;
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



//register the blog psot repo here  -- remember for scoped sevices , create secondary scope to use 
//register the projects repo here 
//register the repoevent repo here
//
//


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

    await context.WriteHtmlToStream(
        "<a href='/posts'>posts </a><br><a href='/post/top-5-tips-for-clean-csharp-code'>post/top-5-tips-for-clean-csharp-code</a>",
        HttpStatusCode.OK);
});

swytchApp.AddAction("GET", "/posts", async (context) =>
{
    logger.LogInformation("blog posts request");

    using var scope = serviceProvider.CreateScope();
    var blogPostService = scope.ServiceProvider.GetRequiredService<IBlogPostService>();
    var r = await blogPostService.GetBlogPostsAsync();

    await context.ToOk(r);
});

swytchApp.AddAction("GET", "/post/{slug}", async (context) =>
{

    logger.LogInformation("Blog Post Request");

    using var scope = serviceProvider.CreateScope();
    var blogPostService = scope.ServiceProvider.GetRequiredService<IBlogPostService>();
    var r = await blogPostService.GetBlogPostAsync(context.PathParams["slug"]);

    await context.ToOk(r);
});

swytchApp.AddAction("GET", "/projects", async (context) =>
{
    await context.WriteTextToStream("projects", HttpStatusCode.OK);
});

swytchApp.AddAction("GET", "/repoevent", async (context) =>
{
    await context.WriteTextToStream("repoevent", HttpStatusCode.OK);
});



//create tables
DatabaseHelper.CreateTablesIfNotExist(swytchApp);
DatabaseHelper.InsertSampleTestDataInDatabase(swytchApp);

//Start app
await swytchApp.Listen();
