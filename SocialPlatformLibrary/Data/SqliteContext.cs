using System;
using System.Data;
using Microsoft.Data.Sqlite;

namespace SocialNetworkPlatform.Data
{
    /// <summary>
    /// SQLite database context for managing database connections and initialization.
    /// </summary>
    public class SqliteContext : IDisposable
    {
        private readonly string _connectionString;
        private SqliteConnection? _connection;

        public SqliteContext(string databasePath)
        {
            _connectionString = $"Data Source={databasePath};";
        }

        public SqliteConnection GetConnection()
        {
            if (_connection == null)
            {
                _connection = new SqliteConnection(_connectionString);
                _connection.Open();
            }
            return _connection;
        }

        public void InitializeDatabase()
        {
            var connection = GetConnection();
            using var command = connection.CreateCommand();

            // Create all tables
            command.CommandText = GetCreateTablesScript();
            command.ExecuteNonQuery();
        }

        private string GetCreateTablesScript()
        {
            return @"
                -- Users table
                CREATE TABLE IF NOT EXISTS Users (
                    Id TEXT PRIMARY KEY,
                    Username TEXT NOT NULL UNIQUE,
                    DisplayName TEXT NOT NULL,
                    Age INTEGER NOT NULL,
                    CreatedAt TEXT NOT NULL
                );

                -- User Friends (many-to-many)
                CREATE TABLE IF NOT EXISTS UserFriends (
                    UserId TEXT NOT NULL,
                    FriendId TEXT NOT NULL,
                    PRIMARY KEY (UserId, FriendId),
                    FOREIGN KEY (UserId) REFERENCES Users(Id),
                    FOREIGN KEY (FriendId) REFERENCES Users(Id)
                );

                -- Posts table
                CREATE TABLE IF NOT EXISTS Posts (
                    Id TEXT PRIMARY KEY,
                    AuthorId TEXT NOT NULL,
                    CreatedAt TEXT NOT NULL,
                    Content TEXT NOT NULL,
                    Visibility INTEGER NOT NULL,
                    SharedFrom TEXT,
                    FOREIGN KEY (AuthorId) REFERENCES Users(Id)
                );

                -- Post Comments (many-to-many)
                CREATE TABLE IF NOT EXISTS PostComments (
                    PostId TEXT NOT NULL,
                    CommentId TEXT NOT NULL,
                    PRIMARY KEY (PostId, CommentId),
                    FOREIGN KEY (PostId) REFERENCES Posts(Id),
                    FOREIGN KEY (CommentId) REFERENCES Comments(Id)
                );

                -- Post Reactions (many-to-many)
                CREATE TABLE IF NOT EXISTS PostReactions (
                    PostId TEXT NOT NULL,
                    ReactionId TEXT NOT NULL,
                    PRIMARY KEY (PostId, ReactionId),
                    FOREIGN KEY (PostId) REFERENCES Posts(Id),
                    FOREIGN KEY (ReactionId) REFERENCES Reactions(Id)
                );

                -- Comments table
                CREATE TABLE IF NOT EXISTS Comments (
                    Id TEXT PRIMARY KEY,
                    AuthorId TEXT NOT NULL,
                    CreatedAt TEXT NOT NULL,
                    Text TEXT NOT NULL,
                    TargetId TEXT NOT NULL,
                    FOREIGN KEY (AuthorId) REFERENCES Users(Id)
                );

                -- Comment Reactions (many-to-many)
                CREATE TABLE IF NOT EXISTS CommentReactions (
                    CommentId TEXT NOT NULL,
                    ReactionId TEXT NOT NULL,
                    PRIMARY KEY (CommentId, ReactionId),
                    FOREIGN KEY (CommentId) REFERENCES Comments(Id),
                    FOREIGN KEY (ReactionId) REFERENCES Reactions(Id)
                );

                -- Reactions table
                CREATE TABLE IF NOT EXISTS Reactions (
                    Id TEXT PRIMARY KEY,
                    AuthorId TEXT NOT NULL,
                    CreatedAt TEXT NOT NULL,
                    TargetId TEXT NOT NULL,
                    Type INTEGER NOT NULL,
                    FOREIGN KEY (AuthorId) REFERENCES Users(Id)
                );

                -- Reaction Reactions (nested reactions, many-to-many)
                CREATE TABLE IF NOT EXISTS ReactionReactions (
                    ReactionId TEXT NOT NULL,
                    NestedReactionId TEXT NOT NULL,
                    PRIMARY KEY (ReactionId, NestedReactionId),
                    FOREIGN KEY (ReactionId) REFERENCES Reactions(Id),
                    FOREIGN KEY (NestedReactionId) REFERENCES Reactions(Id)
                );

                -- Reels table
                CREATE TABLE IF NOT EXISTS Reels (
                    Id TEXT PRIMARY KEY,
                    AuthorId TEXT NOT NULL,
                    CreatedAt TEXT NOT NULL,
                    MediaUrl TEXT NOT NULL,
                    Duration TEXT NOT NULL,
                    FOREIGN KEY (AuthorId) REFERENCES Users(Id)
                );

                -- Reel Views (many-to-many)
                CREATE TABLE IF NOT EXISTS ReelViews (
                    ReelId TEXT NOT NULL,
                    UserId TEXT NOT NULL,
                    PRIMARY KEY (ReelId, UserId),
                    FOREIGN KEY (ReelId) REFERENCES Reels(Id),
                    FOREIGN KEY (UserId) REFERENCES Users(Id)
                );

                -- Reel Comments (many-to-many)
                CREATE TABLE IF NOT EXISTS ReelComments (
                    ReelId TEXT NOT NULL,
                    CommentId TEXT NOT NULL,
                    PRIMARY KEY (ReelId, CommentId),
                    FOREIGN KEY (ReelId) REFERENCES Reels(Id),
                    FOREIGN KEY (CommentId) REFERENCES Comments(Id)
                );

                -- Reel Reactions (many-to-many)
                CREATE TABLE IF NOT EXISTS ReelReactions (
                    ReelId TEXT NOT NULL,
                    ReactionId TEXT NOT NULL,
                    PRIMARY KEY (ReelId, ReactionId),
                    FOREIGN KEY (ReelId) REFERENCES Reels(Id),
                    FOREIGN KEY (ReactionId) REFERENCES Reactions(Id)
                );

                -- Stories table
                CREATE TABLE IF NOT EXISTS Stories (
                    Id TEXT PRIMARY KEY,
                    AuthorId TEXT NOT NULL,
                    CreatedAt TEXT NOT NULL,
                    MediaUrl TEXT NOT NULL,
                    ExpiresAt TEXT NOT NULL,
                    FOREIGN KEY (AuthorId) REFERENCES Users(Id)
                );

                -- Story Views (many-to-many)
                CREATE TABLE IF NOT EXISTS StoryViews (
                    StoryId TEXT NOT NULL,
                    UserId TEXT NOT NULL,
                    PRIMARY KEY (StoryId, UserId),
                    FOREIGN KEY (StoryId) REFERENCES Stories(Id),
                    FOREIGN KEY (UserId) REFERENCES Users(Id)
                );

                -- Story Comments (many-to-many)
                CREATE TABLE IF NOT EXISTS StoryComments (
                    StoryId TEXT NOT NULL,
                    CommentId TEXT NOT NULL,
                    PRIMARY KEY (StoryId, CommentId),
                    FOREIGN KEY (StoryId) REFERENCES Stories(Id),
                    FOREIGN KEY (CommentId) REFERENCES Comments(Id)
                );

                -- Story Reactions (many-to-many)
                CREATE TABLE IF NOT EXISTS StoryReactions (
                    StoryId TEXT NOT NULL,
                    ReactionId TEXT NOT NULL,
                    PRIMARY KEY (StoryId, ReactionId),
                    FOREIGN KEY (StoryId) REFERENCES Stories(Id),
                    FOREIGN KEY (ReactionId) REFERENCES Reactions(Id)
                );

                -- Pages table
                CREATE TABLE IF NOT EXISTS Pages (
                    Id TEXT PRIMARY KEY,
                    OwnerId TEXT NOT NULL,
                    Name TEXT NOT NULL,
                    Description TEXT NOT NULL,
                    FOREIGN KEY (OwnerId) REFERENCES Users(Id)
                );

                -- Page Followers (many-to-many)
                CREATE TABLE IF NOT EXISTS PageFollowers (
                    PageId TEXT NOT NULL,
                    UserId TEXT NOT NULL,
                    PRIMARY KEY (PageId, UserId),
                    FOREIGN KEY (PageId) REFERENCES Pages(Id),
                    FOREIGN KEY (UserId) REFERENCES Users(Id)
                );

                -- Page Posts (many-to-many)
                CREATE TABLE IF NOT EXISTS PagePosts (
                    PageId TEXT NOT NULL,
                    PostId TEXT NOT NULL,
                    PRIMARY KEY (PageId, PostId),
                    FOREIGN KEY (PageId) REFERENCES Pages(Id),
                    FOREIGN KEY (PostId) REFERENCES Posts(Id)
                );

                -- Page Events table
                CREATE TABLE IF NOT EXISTS PageEvents (
                    Id TEXT PRIMARY KEY,
                    PageId TEXT NOT NULL,
                    Title TEXT NOT NULL,
                    Description TEXT NOT NULL,
                    StartsAt TEXT NOT NULL,
                    EndsAt TEXT NOT NULL,
                    Location TEXT NOT NULL,
                    FOREIGN KEY (PageId) REFERENCES Pages(Id)
                );

                -- Page Event Comments (many-to-many)
                CREATE TABLE IF NOT EXISTS PageEventComments (
                    PageEventId TEXT NOT NULL,
                    CommentId TEXT NOT NULL,
                    PRIMARY KEY (PageEventId, CommentId),
                    FOREIGN KEY (PageEventId) REFERENCES PageEvents(Id),
                    FOREIGN KEY (CommentId) REFERENCES Comments(Id)
                );

                -- Page Event Reactions (many-to-many)
                CREATE TABLE IF NOT EXISTS PageEventReactions (
                    PageEventId TEXT NOT NULL,
                    ReactionId TEXT NOT NULL,
                    PRIMARY KEY (PageEventId, ReactionId),
                    FOREIGN KEY (PageEventId) REFERENCES PageEvents(Id),
                    FOREIGN KEY (ReactionId) REFERENCES Reactions(Id)
                );
            ";
        }

        public void Dispose()
        {
            _connection?.Dispose();
        }
    }
}
