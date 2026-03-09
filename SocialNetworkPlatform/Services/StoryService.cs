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
    public class StoryService : IStoryService
    {
        private readonly StoryRepo _repo;

        /// <summary>
        /// Initializes a new instance of the <see cref="StoryService"/> class.
        /// </summary>
        /// <param name="repo">Repository used to persist stories.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="repo"/> is null.</exception>
        public StoryService(StoryRepo repo)
        {
            _repo = repo ?? throw new ArgumentNullException(nameof(repo));
        }

        /// <summary>
        /// Creates and persists a new <see cref="Story"/> from the provided <see cref="MediaDto"/>.
        /// </summary>
        /// <param name="dto">Data transfer object containing story information (author, media, optional expiry).</param>
        /// <returns>The created <see cref="Story"/> instance (Id populated).</returns>
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

        /// <summary>
        /// Retrieves a story by identifier.
        /// </summary>
        /// <param name="id">The story identifier.</param>
        /// <returns>The matching <see cref="Story"/>, or <c>null</c> when not found.</returns>
        public Story? Get(Guid id) => _repo.Get(id);

        /// <summary>
        /// Returns all non-expired stories currently stored in the repository.
        /// </summary>
        /// <returns>An enumerable of active <see cref="Story"/> instances.</returns>
        public IEnumerable<Story> GetAll() => _repo.GetAll().Where(s => s.ExpiresAt > DateTime.UtcNow);

        /// <summary>
        /// Registers that a user has viewed the specified story.
        /// If the user already viewed the story, the call is a no-op.
        /// </summary>
        /// <param name="storyId">The story identifier.</param>
        /// <param name="userId">The user who viewed the story.</param>
        public void AddView(Guid storyId, Guid userId)
        {
            var s = _repo.Get(storyId);
            if (s == null) return;
            if (!s.ViewedBy.Contains(userId)) s.ViewedBy.Add(userId);
        }

        /// <summary>
        /// Removes expired stories from the repository. Stories whose <see cref="Story.ExpiresAt"/>
        /// is in the past will be deleted.
        /// </summary>
        /// <remarks>
        /// This method should be called periodically (for example by a scheduler) to keep the
        /// repository free of expired items.
        /// </remarks>
        public void RemoveExpiredStories()
        {
            var expired = _repo.GetAll().Where(s => s.ExpiresAt <= DateTime.UtcNow).ToArray();
            foreach (var e in expired) _repo.Remove(e.Id);
        }
    }
}