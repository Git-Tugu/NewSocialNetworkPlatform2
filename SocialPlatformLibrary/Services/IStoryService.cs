using SocialNetworkPlatform.DTOs;
using SocialNetworkPlatform.Models;
using System;
using System.Collections.Generic;

namespace SocialNetworkPlatform.Services
{
    public interface IStoryService
    {
        Story Create(MediaDto dto);
        Story? Get(Guid id);
        IEnumerable<Story> GetAll();
        void AddView(Guid storyId, Guid userId);
        void RemoveExpiredStories();
    }
}