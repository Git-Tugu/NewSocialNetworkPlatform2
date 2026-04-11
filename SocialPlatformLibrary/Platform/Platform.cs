using System;
using System.IO;
using SocialNetworkPlatform.Data;
using SocialNetworkPlatform.Repositories;
using SocialNetworkPlatform.Services;

namespace SocialNetworkPlatform.Platform
{
    /// <summary>
    /// Composition root for the platform where services and repositories are assembled.
    /// Uses SQLite for data persistence.
    /// </summary>
    public class Platform : IDisposable
    {
        private readonly SqliteContext _context;

        public UserRepo Users { get; }
        public PostRepo Posts { get; }
        public ReelRepo Reels { get; }
        public StoryRepo Stories { get; }
        public PageRepo Pages { get; }
        public PageEventRepo PageEvents { get; }
        public CommentRepo Comments { get; }
        public ReactionRepo Reactions { get; }

        public IUserService UserService { get; }
        public IPostService PostService { get; }
        public IReelService ReelService { get; }
        public IStoryService StoryService { get; }
        public IPageService PageService { get; }
        public ISearchService SearchService { get; }
        public ICommentService CommentService { get; }
        public IReactionService ReactionService { get; }

        public Platform()
        {
            // Initialize SQLite context
            var databaseFolder = Path.Combine(AppContext.BaseDirectory, "Database");
            Directory.CreateDirectory(databaseFolder);
            var databasePath = Path.Combine(databaseFolder, "socialnetwork.db");

            _context = new SqliteContext(databasePath);
            _context.InitializeDatabase();

            // Initialize SQLite repositories
            Users = new UserRepo(_context);
            Posts = new PostRepo(_context);
            Reels = new ReelRepo(_context);
            Stories = new StoryRepo(_context);
            Pages = new PageRepo(_context);
            PageEvents = new PageEventRepo(_context);
            Comments = new CommentRepo(_context);
            Reactions = new ReactionRepo(_context);

            // Initialize comment and reaction services first (other services depend on them)
            ReactionService = new ReactionService(Reactions, Posts, Reels, Stories, PageEvents, Comments);
            CommentService = new CommentService(Comments, Posts, Reels, Stories, PageEvents, ReactionService);

            // Initialize content services with comment/reaction dependencies
            UserService = new UserService(Users);
            PostService = new PostService(Posts, Users, CommentService, ReactionService);
            ReelService = new ReelService(Reels, CommentService, ReactionService);
            StoryService = new StoryService(Stories, CommentService, ReactionService);
            PageService = new PageService(Pages, PageEvents, CommentService, ReactionService);
            SearchService = new SearchService(Users, Posts, Pages, Reels, Stories);
        }

        public void Dispose()
        {
            _context?.Dispose();
        }
    }
}
