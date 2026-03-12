using System;
using System.Collections.Generic;
using System.Linq;
using SocialNetworkPlatform.DTOs;
using SocialNetworkPlatform.Models;
using SocialNetworkPlatform.Repositories;

namespace SocialNetworkPlatform.Services
{
    /// <summary>
    /// Service that manages <see cref="Story"/> lifecycle and view operations.
    /// </summary>
    public class StoryService : CrudService<Story, MediaDto>, IStoryService
    {
        private readonly StoryRepo _repo;

        public StoryService(StoryRepo repo)
            : base(repo, dto => new Story { AuthorId = dto.AuthorId, MediaUrl = dto.MediaUrl ?? string.Empty, ExpiresAt = dto.ExpiresAt ?? DateTime.UtcNow.AddHours(24) })
        {
            _repo = repo ?? throw new ArgumentNullException(nameof(repo));
        }

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