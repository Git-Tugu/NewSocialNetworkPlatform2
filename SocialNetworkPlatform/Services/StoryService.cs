using System;
using System.Collections.Generic;
using System.Linq;
using SocialNetworkPlatform.DTOs;
using SocialNetworkPlatform.Models;
using SocialNetworkPlatform.Repositories;

namespace SocialNetworkPlatform.Services
{
    public class StoryService : IStoryService
    {
        private readonly StoryRepo _repo;

        public StoryService(StoryRepo repo)
        {
            _repo = repo ?? throw new ArgumentNullException(nameof(repo));
        }

        public Story Create(MediaDto dto)
        {
            var s = new Story
            {
                AuthorId = dto.AuthorId,
                MediaUrl = dto.MediaUrl ?? string.Empty,
                ExpiresAt = dto.ExpiresAt ?? DateTime.UtcNow.AddHours(24)
            };
            _repo.Add(s);
            return s;
        }

        public Story? Get(Guid id) => _repo.Get(id);

        public IEnumerable<Story> GetAll() => _repo.GetAll().Where(s => s.ExpiresAt > DateTime.UtcNow);

        public void AddView(Guid storyId, Guid userId)
        {
            var s = _repo.Get(storyId);
            if (s == null) return;
            if (!s.ViewedBy.Contains(userId)) s.ViewedBy.Add(userId);
        }

        public void RemoveExpiredStories()
        {
            var expired = _repo.GetAll().Where(s => s.ExpiresAt <= DateTime.UtcNow).ToArray();
            foreach (var e in expired) _repo.Remove(e.Id);
        }
    }
}