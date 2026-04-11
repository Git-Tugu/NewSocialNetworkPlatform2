using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Data.Sqlite;
using SocialNetworkPlatform.Data;
using SocialNetworkPlatform.Models;
using SocialNetworkPlatform.Enums;

namespace SocialNetworkPlatform.Repositories
{
    /// <summary>
    /// SQLite repository implementation for User entities.
    /// </summary>
    public class SqliteUserRepository : IRepository<User>
    {
        protected readonly SqliteContext _context;

        public SqliteUserRepository(SqliteContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public virtual User? Get(Guid id)
        {
            var connection = _context.GetConnection();
            using var command = connection.CreateCommand();

            command.CommandText = "SELECT * FROM Users WHERE Id = @id";
            command.Parameters.AddWithValue("@id", id.ToString());

            using var reader = command.ExecuteReader();
            if (!reader.Read()) return null;

            var username = reader.GetString(reader.GetOrdinal("Username"));
            var displayName = reader.GetString(reader.GetOrdinal("DisplayName"));
            var age = (byte)reader.GetInt32(reader.GetOrdinal("Age"));

            var user = new User(username, displayName, age) { Id = id };
            LoadUserFriends(user);
            return user;
        }

        public virtual IEnumerable<User> GetAll()
        {
            var connection = _context.GetConnection();
            using var command = connection.CreateCommand();

            command.CommandText = "SELECT * FROM Users";

            using var reader = command.ExecuteReader();
            var results = new List<User>();
            while (reader.Read())
            {
                var id = Guid.Parse(reader.GetString(reader.GetOrdinal("Id")));
                var username = reader.GetString(reader.GetOrdinal("Username"));
                var displayName = reader.GetString(reader.GetOrdinal("DisplayName"));
                var age = (byte)reader.GetInt32(reader.GetOrdinal("Age"));

                var user = new User(username, displayName, age) { Id = id };
                LoadUserFriends(user);
                results.Add(user);
            }
            return results;
        }

        public virtual void Add(User item)
        {
            if (item == null) throw new ArgumentNullException(nameof(item));

            var connection = _context.GetConnection();
            using var command = connection.CreateCommand();

            command.CommandText = @"INSERT OR REPLACE INTO Users (Id, Username, DisplayName, Age, CreatedAt)
                                    VALUES (@id, @username, @displayName, @age, @createdAt)";
            command.Parameters.AddWithValue("@id", item.Id.ToString());
            command.Parameters.AddWithValue("@username", item.Username);
            command.Parameters.AddWithValue("@displayName", item.DisplayName);
            command.Parameters.AddWithValue("@age", item.Age);
            command.Parameters.AddWithValue("@createdAt", DateTime.UtcNow.ToString("O"));
            command.ExecuteNonQuery();

            AddUserFriends(item);
        }

        public virtual void Remove(Guid id)
        {
            var connection = _context.GetConnection();
            using var command = connection.CreateCommand();

            command.CommandText = "DELETE FROM Users WHERE Id = @id";
            command.Parameters.AddWithValue("@id", id.ToString());
            command.ExecuteNonQuery();
        }

        private void AddUserFriends(User user)
        {
            var connection = _context.GetConnection();
            foreach (var friendId in user.FriendIds)
            {
                using var command = connection.CreateCommand();
                command.CommandText = @"INSERT OR IGNORE INTO UserFriends (UserId, FriendId)
                                        VALUES (@userId, @friendId)";
                command.Parameters.AddWithValue("@userId", user.Id.ToString());
                command.Parameters.AddWithValue("@friendId", friendId.ToString());
                command.ExecuteNonQuery();
            }
        }

        private void LoadUserFriends(User user)
        {
            var connection = _context.GetConnection();
            using var command = connection.CreateCommand();
            command.CommandText = "SELECT FriendId FROM UserFriends WHERE UserId = @userId";
            command.Parameters.AddWithValue("@userId", user.Id.ToString());

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                var friendId = Guid.Parse(reader.GetString(reader.GetOrdinal("FriendId")));
                user.AddFriend(friendId);
            }
        }
    }

    /// <summary>
    /// SQLite repository implementation for Post entities.
    /// </summary>
    public class SqlitePostRepository : IRepository<Post>
    {
        protected readonly SqliteContext _context;

        public SqlitePostRepository(SqliteContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public virtual Post? Get(Guid id)
        {
            var connection = _context.GetConnection();
            using var command = connection.CreateCommand();

            command.CommandText = "SELECT * FROM Posts WHERE Id = @id";
            command.Parameters.AddWithValue("@id", id.ToString());

            using var reader = command.ExecuteReader();
            if (!reader.Read()) return null;

            return MapPost(reader);
        }

        public virtual IEnumerable<Post> GetAll()
        {
            var connection = _context.GetConnection();
            using var command = connection.CreateCommand();

            command.CommandText = "SELECT * FROM Posts";

            using var reader = command.ExecuteReader();
            var results = new List<Post>();
            while (reader.Read())
            {
                var post = MapPost(reader);
                if (post != null) results.Add(post);
            }
            return results;
        }

        public virtual void Add(Post item)
        {
            if (item == null) throw new ArgumentNullException(nameof(item));

            var connection = _context.GetConnection();
            using var command = connection.CreateCommand();

            command.CommandText = @"INSERT OR REPLACE INTO Posts (Id, AuthorId, CreatedAt, Content, Visibility, SharedFrom)
                                    VALUES (@id, @authorId, @createdAt, @content, @visibility, @sharedFrom)";
            command.Parameters.AddWithValue("@id", item.Id.ToString());
            command.Parameters.AddWithValue("@authorId", item.AuthorId.ToString());
            command.Parameters.AddWithValue("@createdAt", item.CreatedAt.ToString("O"));
            command.Parameters.AddWithValue("@content", item.Content ?? "");
            command.Parameters.AddWithValue("@visibility", (int)item.Visibility);
            command.Parameters.AddWithValue("@sharedFrom", item.SharedFrom?.ToString() ?? (object)DBNull.Value);
            command.ExecuteNonQuery();

            AddPostComments(item);
            AddPostReactions(item);
        }

        public virtual void Remove(Guid id)
        {
            var connection = _context.GetConnection();
            using var command = connection.CreateCommand();

            command.CommandText = "DELETE FROM Posts WHERE Id = @id";
            command.Parameters.AddWithValue("@id", id.ToString());
            command.ExecuteNonQuery();
        }

        private Post? MapPost(SqliteDataReader reader)
        {
            var id = Guid.Parse(reader.GetString(reader.GetOrdinal("Id")));
            var authorId = Guid.Parse(reader.GetString(reader.GetOrdinal("AuthorId")));
            var content = reader.GetString(reader.GetOrdinal("Content"));
            var visibility = (Visibility)reader.GetInt32(reader.GetOrdinal("Visibility"));

            var post = new Post { Id = id, AuthorId = authorId, Content = content, Visibility = visibility };

            var sharedFromOrd = reader.GetOrdinal("SharedFrom");
            if (!reader.IsDBNull(sharedFromOrd))
            {
                var sharedFromStr = reader.GetString(sharedFromOrd);
                if (Guid.TryParse(sharedFromStr, out var sharedFrom))
                    post.SharedFrom = sharedFrom;
            }

            LoadPostComments(post);
            LoadPostReactions(post);

            return post;
        }

        private void AddPostComments(Post post)
        {
            var connection = _context.GetConnection();
            foreach (var commentId in post.CommentIds)
            {
                using var command = connection.CreateCommand();
                command.CommandText = @"INSERT OR IGNORE INTO PostComments (PostId, CommentId)
                                        VALUES (@postId, @commentId)";
                command.Parameters.AddWithValue("@postId", post.Id.ToString());
                command.Parameters.AddWithValue("@commentId", commentId.ToString());
                command.ExecuteNonQuery();
            }
        }

        private void LoadPostComments(Post post)
        {
            var connection = _context.GetConnection();
            using var command = connection.CreateCommand();
            command.CommandText = "SELECT CommentId FROM PostComments WHERE PostId = @postId";
            command.Parameters.AddWithValue("@postId", post.Id.ToString());

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                var commentId = Guid.Parse(reader.GetString(reader.GetOrdinal("CommentId")));
                post.CommentIds.Add(commentId);
            }
        }

        private void AddPostReactions(Post post)
        {
            var connection = _context.GetConnection();
            foreach (var reactionId in post.ReactionIds)
            {
                using var command = connection.CreateCommand();
                command.CommandText = @"INSERT OR IGNORE INTO PostReactions (PostId, ReactionId)
                                        VALUES (@postId, @reactionId)";
                command.Parameters.AddWithValue("@postId", post.Id.ToString());
                command.Parameters.AddWithValue("@reactionId", reactionId.ToString());
                command.ExecuteNonQuery();
            }
        }

        private void LoadPostReactions(Post post)
        {
            var connection = _context.GetConnection();
            using var command = connection.CreateCommand();
            command.CommandText = "SELECT ReactionId FROM PostReactions WHERE PostId = @postId";
            command.Parameters.AddWithValue("@postId", post.Id.ToString());

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                var reactionId = Guid.Parse(reader.GetString(reader.GetOrdinal("ReactionId")));
                post.ReactionIds.Add(reactionId);
            }
        }
    }

    /// <summary>
    /// SQLite repository implementation for Comment entities.
    /// </summary>
    public class SqliteCommentRepository : IRepository<Comment>
    {
        protected readonly SqliteContext _context;

        public SqliteCommentRepository(SqliteContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public virtual Comment? Get(Guid id)
        {
            var connection = _context.GetConnection();
            using var command = connection.CreateCommand();

            command.CommandText = "SELECT * FROM Comments WHERE Id = @id";
            command.Parameters.AddWithValue("@id", id.ToString());

            using var reader = command.ExecuteReader();
            if (!reader.Read()) return null;

            return MapComment(reader);
        }

        public virtual IEnumerable<Comment> GetAll()
        {
            var connection = _context.GetConnection();
            using var command = connection.CreateCommand();

            command.CommandText = "SELECT * FROM Comments";

            using var reader = command.ExecuteReader();
            var results = new List<Comment>();
            while (reader.Read())
            {
                var comment = MapComment(reader);
                if (comment != null) results.Add(comment);
            }
            return results;
        }

        public virtual void Add(Comment item)
        {
            if (item == null) throw new ArgumentNullException(nameof(item));

            var connection = _context.GetConnection();
            using var command = connection.CreateCommand();

            command.CommandText = @"INSERT OR REPLACE INTO Comments (Id, AuthorId, CreatedAt, Text, TargetId)
                                    VALUES (@id, @authorId, @createdAt, @text, @targetId)";
            command.Parameters.AddWithValue("@id", item.Id.ToString());
            command.Parameters.AddWithValue("@authorId", item.AuthorId.ToString());
            command.Parameters.AddWithValue("@createdAt", item.CreatedAt.ToString("O"));
            command.Parameters.AddWithValue("@text", item.Text ?? "");
            command.Parameters.AddWithValue("@targetId", item.TargetId.ToString());
            command.ExecuteNonQuery();

            AddCommentReactions(item);
        }

        public virtual void Remove(Guid id)
        {
            var connection = _context.GetConnection();
            using var command = connection.CreateCommand();

            command.CommandText = "DELETE FROM Comments WHERE Id = @id";
            command.Parameters.AddWithValue("@id", id.ToString());
            command.ExecuteNonQuery();
        }

        private Comment? MapComment(SqliteDataReader reader)
        {
            var id = Guid.Parse(reader.GetString(reader.GetOrdinal("Id")));
            var authorId = Guid.Parse(reader.GetString(reader.GetOrdinal("AuthorId")));
            var text = reader.GetString(reader.GetOrdinal("Text"));
            var targetId = Guid.Parse(reader.GetString(reader.GetOrdinal("TargetId")));

            var comment = new Comment { Id = id, AuthorId = authorId, Text = text, TargetId = targetId };

            LoadCommentReactions(comment);

            return comment;
        }

        private void AddCommentReactions(Comment comment)
        {
            var connection = _context.GetConnection();
            foreach (var reactionId in comment.ReactionIds)
            {
                using var command = connection.CreateCommand();
                command.CommandText = @"INSERT OR IGNORE INTO CommentReactions (CommentId, ReactionId)
                                        VALUES (@commentId, @reactionId)";
                command.Parameters.AddWithValue("@commentId", comment.Id.ToString());
                command.Parameters.AddWithValue("@reactionId", reactionId.ToString());
                command.ExecuteNonQuery();
            }
        }

        private void LoadCommentReactions(Comment comment)
        {
            var connection = _context.GetConnection();
            using var command = connection.CreateCommand();
            command.CommandText = "SELECT ReactionId FROM CommentReactions WHERE CommentId = @commentId";
            command.Parameters.AddWithValue("@commentId", comment.Id.ToString());

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                var reactionId = Guid.Parse(reader.GetString(reader.GetOrdinal("ReactionId")));
                comment.ReactionIds.Add(reactionId);
            }
        }
    }

    /// <summary>
    /// SQLite repository implementation for Reaction entities.
    /// </summary>
    public class SqliteReactionRepository : IRepository<Reaction>
    {
        protected readonly SqliteContext _context;

        public SqliteReactionRepository(SqliteContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public virtual Reaction? Get(Guid id)
        {
            var connection = _context.GetConnection();
            using var command = connection.CreateCommand();

            command.CommandText = "SELECT * FROM Reactions WHERE Id = @id";
            command.Parameters.AddWithValue("@id", id.ToString());

            using var reader = command.ExecuteReader();
            if (!reader.Read()) return null;

            return MapReaction(reader);
        }

        public virtual IEnumerable<Reaction> GetAll()
        {
            var connection = _context.GetConnection();
            using var command = connection.CreateCommand();

            command.CommandText = "SELECT * FROM Reactions";

            using var reader = command.ExecuteReader();
            var results = new List<Reaction>();
            while (reader.Read())
            {
                var reaction = MapReaction(reader);
                if (reaction != null) results.Add(reaction);
            }
            return results;
        }

        public virtual void Add(Reaction item)
        {
            if (item == null) throw new ArgumentNullException(nameof(item));

            var connection = _context.GetConnection();
            using var command = connection.CreateCommand();

            command.CommandText = @"INSERT OR REPLACE INTO Reactions (Id, AuthorId, CreatedAt, TargetId, Type)
                                    VALUES (@id, @authorId, @createdAt, @targetId, @type)";
            command.Parameters.AddWithValue("@id", item.Id.ToString());
            command.Parameters.AddWithValue("@authorId", item.AuthorId.ToString());
            command.Parameters.AddWithValue("@createdAt", item.CreatedAt.ToString("O"));
            command.Parameters.AddWithValue("@targetId", item.TargetId.ToString());
            command.Parameters.AddWithValue("@type", (int)item.Type);
            command.ExecuteNonQuery();

            AddReactionReactions(item);
        }

        public virtual void Remove(Guid id)
        {
            var connection = _context.GetConnection();
            using var command = connection.CreateCommand();

            command.CommandText = "DELETE FROM Reactions WHERE Id = @id";
            command.Parameters.AddWithValue("@id", id.ToString());
            command.ExecuteNonQuery();
        }

        private Reaction? MapReaction(SqliteDataReader reader)
        {
            var id = Guid.Parse(reader.GetString(reader.GetOrdinal("Id")));
            var authorId = Guid.Parse(reader.GetString(reader.GetOrdinal("AuthorId")));
            var targetId = Guid.Parse(reader.GetString(reader.GetOrdinal("TargetId")));
            var type = (ReactionType)reader.GetInt32(reader.GetOrdinal("Type"));

            var reaction = new Reaction { Id = id, AuthorId = authorId, TargetId = targetId, Type = type };

            LoadReactionReactions(reaction);

            return reaction;
        }

        private void AddReactionReactions(Reaction reaction)
        {
            var connection = _context.GetConnection();
            foreach (var nestedReactionId in reaction.ReactionIds)
            {
                using var command = connection.CreateCommand();
                command.CommandText = @"INSERT OR IGNORE INTO ReactionReactions (ReactionId, NestedReactionId)
                                        VALUES (@reactionId, @nestedReactionId)";
                command.Parameters.AddWithValue("@reactionId", reaction.Id.ToString());
                command.Parameters.AddWithValue("@nestedReactionId", nestedReactionId.ToString());
                command.ExecuteNonQuery();
            }
        }

        private void LoadReactionReactions(Reaction reaction)
        {
            var connection = _context.GetConnection();
            using var command = connection.CreateCommand();
            command.CommandText = "SELECT NestedReactionId FROM ReactionReactions WHERE ReactionId = @reactionId";
            command.Parameters.AddWithValue("@reactionId", reaction.Id.ToString());

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                var nestedReactionId = Guid.Parse(reader.GetString(reader.GetOrdinal("NestedReactionId")));
                reaction.ReactionIds.Add(nestedReactionId);
            }
        }
    }

    /// <summary>
    /// SQLite repository implementation for Reel entities.
    /// </summary>
    public class SqliteReelRepository : IRepository<Reel>
    {
        protected readonly SqliteContext _context;

        public SqliteReelRepository(SqliteContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public virtual Reel? Get(Guid id)
        {
            var connection = _context.GetConnection();
            using var command = connection.CreateCommand();

            command.CommandText = "SELECT * FROM Reels WHERE Id = @id";
            command.Parameters.AddWithValue("@id", id.ToString());

            using var reader = command.ExecuteReader();
            if (!reader.Read()) return null;

            return MapReel(reader);
        }

        public virtual IEnumerable<Reel> GetAll()
        {
            var connection = _context.GetConnection();
            using var command = connection.CreateCommand();

            command.CommandText = "SELECT * FROM Reels";

            using var reader = command.ExecuteReader();
            var results = new List<Reel>();
            while (reader.Read())
            {
                var reel = MapReel(reader);
                if (reel != null) results.Add(reel);
            }
            return results;
        }

        public virtual void Add(Reel item)
        {
            if (item == null) throw new ArgumentNullException(nameof(item));

            var connection = _context.GetConnection();
            using var command = connection.CreateCommand();

            command.CommandText = @"INSERT OR REPLACE INTO Reels (Id, AuthorId, CreatedAt, MediaUrl, Duration)
                                    VALUES (@id, @authorId, @createdAt, @mediaUrl, @duration)";
            command.Parameters.AddWithValue("@id", item.Id.ToString());
            command.Parameters.AddWithValue("@authorId", item.AuthorId.ToString());
            command.Parameters.AddWithValue("@createdAt", item.CreatedAt.ToString("O"));
            command.Parameters.AddWithValue("@mediaUrl", item.MediaUrl ?? "");
            command.Parameters.AddWithValue("@duration", item.Duration.ToString());
            command.ExecuteNonQuery();

            AddReelViews(item);
            AddReelComments(item);
            AddReelReactions(item);
        }

        public virtual void Remove(Guid id)
        {
            var connection = _context.GetConnection();
            using var command = connection.CreateCommand();

            command.CommandText = "DELETE FROM Reels WHERE Id = @id";
            command.Parameters.AddWithValue("@id", id.ToString());
            command.ExecuteNonQuery();
        }

        private Reel? MapReel(SqliteDataReader reader)
        {
            var id = Guid.Parse(reader.GetString(reader.GetOrdinal("Id")));
            var authorId = Guid.Parse(reader.GetString(reader.GetOrdinal("AuthorId")));
            var mediaUrl = reader.GetString(reader.GetOrdinal("MediaUrl"));
            var durationStr = reader.GetString(reader.GetOrdinal("Duration"));
            var duration = TimeSpan.Parse(durationStr);

            var reel = new Reel { Id = id, AuthorId = authorId, MediaUrl = mediaUrl, Duration = duration };

            LoadReelViews(reel);
            LoadReelComments(reel);
            LoadReelReactions(reel);

            return reel;
        }

        private void AddReelViews(Reel reel)
        {
            var connection = _context.GetConnection();
            foreach (var userId in reel.ViewedBy)
            {
                using var command = connection.CreateCommand();
                command.CommandText = @"INSERT OR IGNORE INTO ReelViews (ReelId, UserId)
                                        VALUES (@reelId, @userId)";
                command.Parameters.AddWithValue("@reelId", reel.Id.ToString());
                command.Parameters.AddWithValue("@userId", userId.ToString());
                command.ExecuteNonQuery();
            }
        }

        private void LoadReelViews(Reel reel)
        {
            var connection = _context.GetConnection();
            using var command = connection.CreateCommand();
            command.CommandText = "SELECT UserId FROM ReelViews WHERE ReelId = @reelId";
            command.Parameters.AddWithValue("@reelId", reel.Id.ToString());

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                var userId = Guid.Parse(reader.GetString(reader.GetOrdinal("UserId")));
                reel.ViewedBy.Add(userId);
            }
        }

        private void AddReelComments(Reel reel)
        {
            var connection = _context.GetConnection();
            foreach (var commentId in reel.CommentIds)
            {
                using var command = connection.CreateCommand();
                command.CommandText = @"INSERT OR IGNORE INTO ReelComments (ReelId, CommentId)
                                        VALUES (@reelId, @commentId)";
                command.Parameters.AddWithValue("@reelId", reel.Id.ToString());
                command.Parameters.AddWithValue("@commentId", commentId.ToString());
                command.ExecuteNonQuery();
            }
        }

        private void LoadReelComments(Reel reel)
        {
            var connection = _context.GetConnection();
            using var command = connection.CreateCommand();
            command.CommandText = "SELECT CommentId FROM ReelComments WHERE ReelId = @reelId";
            command.Parameters.AddWithValue("@reelId", reel.Id.ToString());

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                var commentId = Guid.Parse(reader.GetString(reader.GetOrdinal("CommentId")));
                reel.CommentIds.Add(commentId);
            }
        }

        private void AddReelReactions(Reel reel)
        {
            var connection = _context.GetConnection();
            foreach (var reactionId in reel.ReactionIds)
            {
                using var command = connection.CreateCommand();
                command.CommandText = @"INSERT OR IGNORE INTO ReelReactions (ReelId, ReactionId)
                                        VALUES (@reelId, @reactionId)";
                command.Parameters.AddWithValue("@reelId", reel.Id.ToString());
                command.Parameters.AddWithValue("@reactionId", reactionId.ToString());
                command.ExecuteNonQuery();
            }
        }

        private void LoadReelReactions(Reel reel)
        {
            var connection = _context.GetConnection();
            using var command = connection.CreateCommand();
            command.CommandText = "SELECT ReactionId FROM ReelReactions WHERE ReelId = @reelId";
            command.Parameters.AddWithValue("@reelId", reel.Id.ToString());

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                var reactionId = Guid.Parse(reader.GetString(reader.GetOrdinal("ReactionId")));
                reel.ReactionIds.Add(reactionId);
            }
        }
    }

    /// <summary>
    /// SQLite repository implementation for Story entities.
    /// </summary>
    public class SqliteStoryRepository : IRepository<Story>
    {
        protected readonly SqliteContext _context;

        public SqliteStoryRepository(SqliteContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public virtual Story? Get(Guid id)
        {
            var connection = _context.GetConnection();
            using var command = connection.CreateCommand();

            command.CommandText = "SELECT * FROM Stories WHERE Id = @id";
            command.Parameters.AddWithValue("@id", id.ToString());

            using var reader = command.ExecuteReader();
            if (!reader.Read()) return null;

            return MapStory(reader);
        }

        public virtual IEnumerable<Story> GetAll()
        {
            var connection = _context.GetConnection();
            using var command = connection.CreateCommand();

            command.CommandText = "SELECT * FROM Stories";

            using var reader = command.ExecuteReader();
            var results = new List<Story>();
            while (reader.Read())
            {
                var story = MapStory(reader);
                if (story != null) results.Add(story);
            }
            return results;
        }

        public virtual void Add(Story item)
        {
            if (item == null) throw new ArgumentNullException(nameof(item));

            var connection = _context.GetConnection();
            using var command = connection.CreateCommand();

            command.CommandText = @"INSERT OR REPLACE INTO Stories (Id, AuthorId, CreatedAt, MediaUrl, ExpiresAt)
                                    VALUES (@id, @authorId, @createdAt, @mediaUrl, @expiresAt)";
            command.Parameters.AddWithValue("@id", item.Id.ToString());
            command.Parameters.AddWithValue("@authorId", item.AuthorId.ToString());
            command.Parameters.AddWithValue("@createdAt", item.CreatedAt.ToString("O"));
            command.Parameters.AddWithValue("@mediaUrl", item.MediaUrl ?? "");
            command.Parameters.AddWithValue("@expiresAt", item.ExpiresAt.ToString("O"));
            command.ExecuteNonQuery();

            AddStoryViews(item);
            AddStoryComments(item);
            AddStoryReactions(item);
        }

        public virtual void Remove(Guid id)
        {
            var connection = _context.GetConnection();
            using var command = connection.CreateCommand();

            command.CommandText = "DELETE FROM Stories WHERE Id = @id";
            command.Parameters.AddWithValue("@id", id.ToString());
            command.ExecuteNonQuery();
        }

        private Story? MapStory(SqliteDataReader reader)
        {
            var id = Guid.Parse(reader.GetString(reader.GetOrdinal("Id")));
            var authorId = Guid.Parse(reader.GetString(reader.GetOrdinal("AuthorId")));
            var mediaUrl = reader.GetString(reader.GetOrdinal("MediaUrl"));
            var expiresAt = DateTime.Parse(reader.GetString(reader.GetOrdinal("ExpiresAt")));

            var story = new Story { Id = id, AuthorId = authorId, MediaUrl = mediaUrl, ExpiresAt = expiresAt };

            LoadStoryViews(story);
            LoadStoryComments(story);
            LoadStoryReactions(story);

            return story;
        }

        private void AddStoryViews(Story story)
        {
            var connection = _context.GetConnection();
            foreach (var userId in story.ViewedBy)
            {
                using var command = connection.CreateCommand();
                command.CommandText = @"INSERT OR IGNORE INTO StoryViews (StoryId, UserId)
                                        VALUES (@storyId, @userId)";
                command.Parameters.AddWithValue("@storyId", story.Id.ToString());
                command.Parameters.AddWithValue("@userId", userId.ToString());
                command.ExecuteNonQuery();
            }
        }

        private void LoadStoryViews(Story story)
        {
            var connection = _context.GetConnection();
            using var command = connection.CreateCommand();
            command.CommandText = "SELECT UserId FROM StoryViews WHERE StoryId = @storyId";
            command.Parameters.AddWithValue("@storyId", story.Id.ToString());

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                var userId = Guid.Parse(reader.GetString(reader.GetOrdinal("UserId")));
                story.ViewedBy.Add(userId);
            }
        }

        private void AddStoryComments(Story story)
        {
            var connection = _context.GetConnection();
            foreach (var commentId in story.CommentIds)
            {
                using var command = connection.CreateCommand();
                command.CommandText = @"INSERT OR IGNORE INTO StoryComments (StoryId, CommentId)
                                        VALUES (@storyId, @commentId)";
                command.Parameters.AddWithValue("@storyId", story.Id.ToString());
                command.Parameters.AddWithValue("@commentId", commentId.ToString());
                command.ExecuteNonQuery();
            }
        }

        private void LoadStoryComments(Story story)
        {
            var connection = _context.GetConnection();
            using var command = connection.CreateCommand();
            command.CommandText = "SELECT CommentId FROM StoryComments WHERE StoryId = @storyId";
            command.Parameters.AddWithValue("@storyId", story.Id.ToString());

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                var commentId = Guid.Parse(reader.GetString(reader.GetOrdinal("CommentId")));
                story.CommentIds.Add(commentId);
            }
        }

        private void AddStoryReactions(Story story)
        {
            var connection = _context.GetConnection();
            foreach (var reactionId in story.ReactionIds)
            {
                using var command = connection.CreateCommand();
                command.CommandText = @"INSERT OR IGNORE INTO StoryReactions (StoryId, ReactionId)
                                        VALUES (@storyId, @reactionId)";
                command.Parameters.AddWithValue("@storyId", story.Id.ToString());
                command.Parameters.AddWithValue("@reactionId", reactionId.ToString());
                command.ExecuteNonQuery();
            }
        }

        private void LoadStoryReactions(Story story)
        {
            var connection = _context.GetConnection();
            using var command = connection.CreateCommand();
            command.CommandText = "SELECT ReactionId FROM StoryReactions WHERE StoryId = @storyId";
            command.Parameters.AddWithValue("@storyId", story.Id.ToString());

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                var reactionId = Guid.Parse(reader.GetString(reader.GetOrdinal("ReactionId")));
                story.ReactionIds.Add(reactionId);
            }
        }
    }

    /// <summary>
    /// SQLite repository implementation for Page entities.
    /// </summary>
    public class SqlitePageRepository : IRepository<Page>
    {
        protected readonly SqliteContext _context;

        public SqlitePageRepository(SqliteContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public virtual Page? Get(Guid id)
        {
            var connection = _context.GetConnection();
            using var command = connection.CreateCommand();

            command.CommandText = "SELECT * FROM Pages WHERE Id = @id";
            command.Parameters.AddWithValue("@id", id.ToString());

            using var reader = command.ExecuteReader();
            if (!reader.Read()) return null;

            return MapPage(reader);
        }

        public virtual IEnumerable<Page> GetAll()
        {
            var connection = _context.GetConnection();
            using var command = connection.CreateCommand();

            command.CommandText = "SELECT * FROM Pages";

            using var reader = command.ExecuteReader();
            var results = new List<Page>();
            while (reader.Read())
            {
                var page = MapPage(reader);
                if (page != null) results.Add(page);
            }
            return results;
        }

        public virtual void Add(Page item)
        {
            if (item == null) throw new ArgumentNullException(nameof(item));

            var connection = _context.GetConnection();
            using var command = connection.CreateCommand();

            command.CommandText = @"INSERT OR REPLACE INTO Pages (Id, OwnerId, Name, Description)
                                    VALUES (@id, @ownerId, @name, @description)";
            command.Parameters.AddWithValue("@id", item.Id.ToString());
            command.Parameters.AddWithValue("@ownerId", item.OwnerId.ToString());
            command.Parameters.AddWithValue("@name", item.Name ?? "");
            command.Parameters.AddWithValue("@description", item.Description ?? "");
            command.ExecuteNonQuery();

            AddPageFollowers(item);
            AddPagePosts(item);
        }

        public virtual void Remove(Guid id)
        {
            var connection = _context.GetConnection();
            using var command = connection.CreateCommand();

            command.CommandText = "DELETE FROM Pages WHERE Id = @id";
            command.Parameters.AddWithValue("@id", id.ToString());
            command.ExecuteNonQuery();
        }

        private Page? MapPage(SqliteDataReader reader)
        {
            var id = Guid.Parse(reader.GetString(reader.GetOrdinal("Id")));
            var ownerId = Guid.Parse(reader.GetString(reader.GetOrdinal("OwnerId")));
            var name = reader.GetString(reader.GetOrdinal("Name"));
            var description = reader.GetString(reader.GetOrdinal("Description"));

            var page = new Page { Id = id, Name = name, Description = description, OwnerId = ownerId };

            LoadPageFollowers(page);
            LoadPagePosts(page);

            return page;
        }

        private void AddPageFollowers(Page page)
        {
            var connection = _context.GetConnection();
            foreach (var userId in page.FollowerIds)
            {
                using var command = connection.CreateCommand();
                command.CommandText = @"INSERT OR IGNORE INTO PageFollowers (PageId, UserId)
                                        VALUES (@pageId, @userId)";
                command.Parameters.AddWithValue("@pageId", page.Id.ToString());
                command.Parameters.AddWithValue("@userId", userId.ToString());
                command.ExecuteNonQuery();
            }
        }

        private void LoadPageFollowers(Page page)
        {
            var connection = _context.GetConnection();
            using var command = connection.CreateCommand();
            command.CommandText = "SELECT UserId FROM PageFollowers WHERE PageId = @pageId";
            command.Parameters.AddWithValue("@pageId", page.Id.ToString());

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                var userId = Guid.Parse(reader.GetString(reader.GetOrdinal("UserId")));
                page.FollowerIds.Add(userId);
            }
        }

        private void AddPagePosts(Page page)
        {
            var connection = _context.GetConnection();
            foreach (var postId in page.PostIds)
            {
                using var command = connection.CreateCommand();
                command.CommandText = @"INSERT OR IGNORE INTO PagePosts (PageId, PostId)
                                        VALUES (@pageId, @postId)";
                command.Parameters.AddWithValue("@pageId", page.Id.ToString());
                command.Parameters.AddWithValue("@postId", postId.ToString());
                command.ExecuteNonQuery();
            }
        }

        private void LoadPagePosts(Page page)
        {
            var connection = _context.GetConnection();
            using var command = connection.CreateCommand();
            command.CommandText = "SELECT PostId FROM PagePosts WHERE PageId = @pageId";
            command.Parameters.AddWithValue("@pageId", page.Id.ToString());

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                var postId = Guid.Parse(reader.GetString(reader.GetOrdinal("PostId")));
                page.PostIds.Add(postId);
            }
        }
    }

    /// <summary>
    /// SQLite repository implementation for PageEvent entities.
    /// </summary>
    public class SqlitePageEventRepository : IRepository<PageEvent>
    {
        protected readonly SqliteContext _context;

        public SqlitePageEventRepository(SqliteContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public virtual PageEvent? Get(Guid id)
        {
            var connection = _context.GetConnection();
            using var command = connection.CreateCommand();

            command.CommandText = "SELECT * FROM PageEvents WHERE Id = @id";
            command.Parameters.AddWithValue("@id", id.ToString());

            using var reader = command.ExecuteReader();
            if (!reader.Read()) return null;

            return MapPageEvent(reader);
        }

        public virtual IEnumerable<PageEvent> GetAll()
        {
            var connection = _context.GetConnection();
            using var command = connection.CreateCommand();

            command.CommandText = "SELECT * FROM PageEvents";

            using var reader = command.ExecuteReader();
            var results = new List<PageEvent>();
            while (reader.Read())
            {
                var pageEvent = MapPageEvent(reader);
                if (pageEvent != null) results.Add(pageEvent);
            }
            return results;
        }

        public virtual void Add(PageEvent item)
        {
            if (item == null) throw new ArgumentNullException(nameof(item));

            var connection = _context.GetConnection();
            using var command = connection.CreateCommand();

            command.CommandText = @"INSERT OR REPLACE INTO PageEvents (Id, PageId, Title, Description, StartsAt, EndsAt, Location)
                                    VALUES (@id, @pageId, @title, @description, @startsAt, @endsAt, @location)";
            command.Parameters.AddWithValue("@id", item.Id.ToString());
            command.Parameters.AddWithValue("@pageId", item.PageId.ToString());
            command.Parameters.AddWithValue("@title", item.Title ?? "");
            command.Parameters.AddWithValue("@description", item.Description ?? "");
            command.Parameters.AddWithValue("@startsAt", item.StartsAt.ToString("O"));
            command.Parameters.AddWithValue("@endsAt", item.EndsAt.ToString("O"));
            command.Parameters.AddWithValue("@location", item.Location ?? "");
            command.ExecuteNonQuery();

            AddPageEventComments(item);
            AddPageEventReactions(item);
        }

        public virtual void Remove(Guid id)
        {
            var connection = _context.GetConnection();
            using var command = connection.CreateCommand();

            command.CommandText = "DELETE FROM PageEvents WHERE Id = @id";
            command.Parameters.AddWithValue("@id", id.ToString());
            command.ExecuteNonQuery();
        }

        private PageEvent? MapPageEvent(SqliteDataReader reader)
        {
            var id = Guid.Parse(reader.GetString(reader.GetOrdinal("Id")));
            var pageId = Guid.Parse(reader.GetString(reader.GetOrdinal("PageId")));
            var title = reader.GetString(reader.GetOrdinal("Title"));
            var description = reader.GetString(reader.GetOrdinal("Description"));
            var startsAt = DateTime.Parse(reader.GetString(reader.GetOrdinal("StartsAt")));
            var endsAt = DateTime.Parse(reader.GetString(reader.GetOrdinal("EndsAt")));
            var location = reader.GetString(reader.GetOrdinal("Location"));

            var pageEvent = new PageEvent { Id = id, PageId = pageId, Title = title, Description = description, StartsAt = startsAt, EndsAt = endsAt, Location = location };

            LoadPageEventComments(pageEvent);
            LoadPageEventReactions(pageEvent);

            return pageEvent;
        }

        private void AddPageEventComments(PageEvent pageEvent)
        {
            var connection = _context.GetConnection();
            foreach (var commentId in pageEvent.CommentIds)
            {
                using var command = connection.CreateCommand();
                command.CommandText = @"INSERT OR IGNORE INTO PageEventComments (PageEventId, CommentId)
                                        VALUES (@pageEventId, @commentId)";
                command.Parameters.AddWithValue("@pageEventId", pageEvent.Id.ToString());
                command.Parameters.AddWithValue("@commentId", commentId.ToString());
                command.ExecuteNonQuery();
            }
        }

        private void LoadPageEventComments(PageEvent pageEvent)
        {
            var connection = _context.GetConnection();
            using var command = connection.CreateCommand();
            command.CommandText = "SELECT CommentId FROM PageEventComments WHERE PageEventId = @pageEventId";
            command.Parameters.AddWithValue("@pageEventId", pageEvent.Id.ToString());

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                var commentId = Guid.Parse(reader.GetString(reader.GetOrdinal("CommentId")));
                pageEvent.CommentIds.Add(commentId);
            }
        }

        private void AddPageEventReactions(PageEvent pageEvent)
        {
            var connection = _context.GetConnection();
            foreach (var reactionId in pageEvent.ReactionIds)
            {
                using var command = connection.CreateCommand();
                command.CommandText = @"INSERT OR IGNORE INTO PageEventReactions (PageEventId, ReactionId)
                                        VALUES (@pageEventId, @reactionId)";
                command.Parameters.AddWithValue("@pageEventId", pageEvent.Id.ToString());
                command.Parameters.AddWithValue("@reactionId", reactionId.ToString());
                command.ExecuteNonQuery();
            }
        }

        private void LoadPageEventReactions(PageEvent pageEvent)
        {
            var connection = _context.GetConnection();
            using var command = connection.CreateCommand();
            command.CommandText = "SELECT ReactionId FROM PageEventReactions WHERE PageEventId = @pageEventId";
            command.Parameters.AddWithValue("@pageEventId", pageEvent.Id.ToString());

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                var reactionId = Guid.Parse(reader.GetString(reader.GetOrdinal("ReactionId")));
                pageEvent.ReactionIds.Add(reactionId);
            }
        }
    }
}
