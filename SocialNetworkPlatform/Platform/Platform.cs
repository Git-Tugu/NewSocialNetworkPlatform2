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

        public IUserService UserService { get; }
        public IPostService PostService { get; }
        public IReelService ReelService { get; }
        public IStoryService StoryService { get; }

        public Platform()
        {
            UserService = new UserService(Users);
            PostService = new PostService(Posts, Users);
            ReelService = new ReelService(Reels);
            StoryService = new StoryService(Stories);
        }
    }
}