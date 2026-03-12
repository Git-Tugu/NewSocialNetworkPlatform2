using SocialNetworkPlatform.DTOs;
using SocialNetworkPlatform.Models;
using System;
using System.Collections.Generic;

namespace SocialNetworkPlatform.Services
{
    /// <summary>
    /// Story exists for a limited time (e.g., 24 hours) and can include photos, videos, or text. 
    /// They are designed to be ephemeral and are often used for sharing moments from the day that 
    /// users want to share with their followers without permanently adding them to their profile.
    /// </summary>
    public interface IStoryService
    {
        Story Create(MediaDto dto);
        Story? Get(Guid id);
        IEnumerable<Story> GetAll();
        void AddView(Guid storyId, Guid userId);
        void RemoveExpiredStories();
        void Delete(Guid id);
    }
}