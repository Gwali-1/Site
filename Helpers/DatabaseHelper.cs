using System.Data;
using Dapper;
using Site.Models;
using Swytch.App;
using Swytch.Structures;

namespace Site.Helpers;

public static class DatabaseHelper
{

    public static void CreateTablesIfNotExist(ISwytchApp app)
    {
        using IDbConnection dbConnection = app.GetConnection(DatabaseProviders.SQLite);
        dbConnection.Open();

        // SQL script to create Playlist and Song tables
        string createTablesSql = @"
                -- Create BlogPosts table if it doesn't exist
                CREATE TABLE IF NOT EXISTS BlogPosts (
                  Id INTEGER PRIMARY KEY AUTOINCREMENT,
                  Title TEXT NOT NULL,
                  Tags TEXT,
                  Content TEXT NOT NULL,
                  Date TEXT NOT NULL,  -- e.g 2025-05-24
                  Slug TEXT NOT NULL UNIQUE
                );



                -- Create Projects table if it doesn't exist
                CREATE TABLE IF NOT EXISTS Projects (
                  Id INTEGER PRIMARY KEY AUTOINCREMENT,
                  Name TEXT NOT NULL,
                  Description TEXT,
                  Url TEXT,
                  Image TEXT 
                );

            ";

        dbConnection.Execute(createTablesSql);
    }


    public static void InsertSampleTestDataInDatabase(ISwytchApp app)
    {

        var sampleBlogPosts = new List<BlogPost>
        {
          new BlogPost
          {
              Id = 1,
              Title = "Getting Started with SQLite in C#",
              Tags = "csharp,database,sqlite",
              Content = """
                # getting started with sqlite in c#

                - lightweight and serverless
                - easy integration with c#
                - great for small apps and tools
                """,
              Date = "2025-06-24",
              Slug = "getting-started-with-sqlite-in-csharp"
          },
          new BlogPost
          {
              Id = 2,
              Title = "Why I Built My Own Web Framework",
              Tags = "dotnet,web,framework",
              Content = """
                # How I Built a Minimal Web Framework in C#

                - Minimal routing system
                - Fast and easy to use
                - Designed for full control
                """,
              Date = "2025-06-15",
              Slug = "why-i-built-my-own-web-framework"
          },
          new BlogPost
          {
              Id = 3,
              Title = "Top 5 Tips for Clean C# Code",
              Tags = "csharp,clean-code,practices",
              Content = """
                # Markdown-Based Blogging with SQLite

                - Write posts in `.md` files
                - Store metadata in SQLite
                - Render Markdown to HTML on the fly
                """,
              Date = "2025-05-30",
              Slug = "top-5-tips-for-clean-csharp-code"
          },
        };


        var sampleProjects = new List<Project>
        {
          new Project
          {
              Id = 1,
              Name = "Swytch",
              Description = "A lightweight and fun alternative web framework written in C#.",
              Url = "https://github.com/yourusername/swytch",
              Image = "swytch-banner.png"
          },
          new Project
          {
              Id = 2,
              Name = "Markdown Blog Engine",
              Description = "A minimal blog engine that converts markdown files into a static-like blog powered by SQLite.",
              Url = "https://github.com/yourusername/md-blog-engine",
              Image = "blog-engine-cover.png"
          },
          new Project
          {
              Id = 3,
              Name = "DevNotes",
              Description = "A simple developer note-taking app with support for syntax highlighting and Git-backed storage.",
              Url = "https://github.com/yourusername/devnotes",
              Image = "devnotes-preview.png"
          }
        };


        // Insert data into the database
        using IDbConnection dbConnection = app.GetConnection(DatabaseProviders.SQLite);
        dbConnection.Open();

        // Check if the Playlist table is empty
        int BlogPostsCount = dbConnection.ExecuteScalar<int>("SELECT COUNT(*) FROM BlogPosts;");
        if (BlogPostsCount == 0)
        {
            // Insert playlists
            foreach (var blog in sampleBlogPosts)
            {
                dbConnection.Execute(
                    @"INSERT INTO BlogPosts (Title, Tags, Content, Date, Slug)
                      VALUES (@Title, @Tags, @Content, @Date, @Slug);", blog);
            }

            // Insert songs
            foreach (var project in sampleProjects)
            {
                dbConnection.Execute(
                    @"INSERT INTO Projects (Name, Description, Url, Image)
                      VALUES (@Name, @Description, @Url, @Image);", project);
            }
        }


    }
};

