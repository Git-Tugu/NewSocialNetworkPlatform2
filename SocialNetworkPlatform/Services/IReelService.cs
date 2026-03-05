using SocialNetworkPlatform.DTOs;
using SocialNetworkPlatform.Models;
using System;
using System.Collections.Generic;

namespace SocialNetworkPlatform.Services
{
    public interface IReelService
    {
        Reel Create(MediaDto dto);
        Reel? Get(Guid id);
        IEnumerable<Reel> GetAll();
        void AddView(Guid reelId, Guid userId);
    }
}