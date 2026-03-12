using SocialNetworkPlatform.DTOs;
using SocialNetworkPlatform.Models;
using System;
using System.Collections.Generic;

namespace SocialNetworkPlatform.Services
{
    /// <summary>
    /// I implemented reel as a separate entity from story, even though they share some similarities, 
    /// because they have different behaviors and use cases. Reels are designed for short-form video 
    /// content that can be shared on the main feed and has a longer lifespan than stories, which are
    /// typically more ephemeral and disappear after 24 hours. By keeping them separate, we can better 
    /// manage their specific features and interactions without conflating their functionalities.
    /// </summary>
    public interface IReelService
    {
        Reel Create(MediaDto dto);
        Reel? Get(Guid id);
        IEnumerable<Reel> GetAll();
        void AddView(Guid reelId, Guid userId);
        void Delete(Guid id);
    }
}