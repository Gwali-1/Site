using Dapper;
using Swytch.App;
using Swytch.Structures;

namespace Site.Helpers;

public static class DatabaseHelper
{

    public static void CreateTablesIfNotExist(ISwytchApp app)
    {
        using var dbConnection = app.GetConnection(DatabaseProviders.SQLite);
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



};

