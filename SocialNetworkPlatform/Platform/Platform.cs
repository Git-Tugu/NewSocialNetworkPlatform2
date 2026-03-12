using System;
using SocialNetworkPlatform.Repositories;
using SocialNetworkPlatform.Services;

namespace SocialNetworkPlatform.Platform
{
    /// <summary>
    /// Composition root for the platform where services and repositories are assembled.
    /// </summary>
    public class Platform
    {
        public UserRepo Users { get; } = new();
        public PostRepo Posts { get; } = new();
        public ReelRepo Reels { get; } = new();
        public StoryRepo Stories { get; } = new();
        public PageRepo Pages { get; } = new();
        public PageEventRepo PageEvents { get; } = new();
        public CommentRepo Comments { get; } = new();
        public ReactionRepo Reactions { get; } = new();

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
    }
}