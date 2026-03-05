using System;
using System.Collections.Generic;
using SocialNetworkPlatform.DTOs;
using SocialNetworkPlatform.Models;
using SocialNetworkPlatform.Repositories;

namespace SocialNetworkPlatform.Services
{
    public class ReelService : IReelService
    {
        private readonly ReelRepo _repo;

        public ReelService(ReelRepo repo)
        {
            _repo = repo ?? throw new ArgumentNullException(nameof(repo));
        }

        public Reel Create(MediaDto dto)
        {
            var r = new Reel
            {
                AuthorId = dto.AuthorId,
                MediaUrl = dto.MediaUrl ?? string.Empty,
                Duration = dto.Duration ?? TimeSpan.Zero
            };
            _repo.Add(r);
            return r;
        }

        public Reel? Get(Guid id) => _repo.Get(id);

        public IEnumerable<Reel> GetAll() => _repo.GetAll();

        public void AddView(Guid reelId, Guid userId)
        {
            var r = _repo.Get(reelId);
            if (r == null) return;
            if (!r.ViewedBy.Contains(userId)) r.ViewedBy.Add(userId);
        }
    }
}