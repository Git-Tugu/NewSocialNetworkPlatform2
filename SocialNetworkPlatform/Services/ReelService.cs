using System;
using System.Collections.Generic;
using SocialNetworkPlatform.DTOs;
using SocialNetworkPlatform.Models;
using SocialNetworkPlatform.Repositories;

namespace SocialNetworkPlatform.Services
{
    public class ReelService : CrudService<Reel, MediaDto>, IReelService
    {
        private readonly ReelRepo _typedRepo;

        public ReelService(ReelRepo repo)
            : base(repo, dto => new Reel { AuthorId = dto.AuthorId, MediaUrl = dto.MediaUrl ?? string.Empty, Duration = dto.Duration ?? TimeSpan.Zero })
        {
            _typedRepo = repo ?? throw new ArgumentNullException(nameof(repo));
        }

        public void AddView(Guid reelId, Guid userId)
        {
            var r = _typedRepo.Get(reelId);
            if (r == null) return;
            if (!r.ViewedBy.Contains(userId)) r.ViewedBy.Add(userId);
        }
    }
}