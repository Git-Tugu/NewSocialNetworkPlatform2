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
        private readonly ICommentService _comments;
        private readonly IReactionService _reactions;

        public StoryService(StoryRepo repo, ICommentService comments, IReactionService reactions)
            : base(repo, dto => new Story { AuthorId = dto.AuthorId, MediaUrl = dto.MediaUrl ?? string.Empty, ExpiresAt = dto.ExpiresAt ?? DateTime.UtcNow.AddHours(24) })
        {
            _repo = repo ?? throw new ArgumentNullException(nameof(repo));
            _comments = comments ?? throw new ArgumentNullException(nameof(comments));
            _reactions = reactions ?? throw new ArgumentNullException(nameof(reactions));
        }


        /// <summary>
        /// Returns all non-expired stories. Expired stories are automatically 
        /// removed by a background job that calls <see cref="RemoveExpiredStories"/> periodically.
        /// </summary>
        /// <returns>Stories (existing)</returns>
        public IEnumerable<Story> GetAll() => _repo.GetAll().Where(s => s.ExpiresAt > DateTime.UtcNow);

        public void AddView(Guid storyId, Guid userId)
        {
            var s = _repo.Get(storyId);
            if (s == null) return;
            if (!s.ViewedBy.Contains(userId)) s.ViewedBy.Add(userId);
        }


        /// <summary>
        /// Scans for expired stories and removes them from the repository. 
        /// </summary>
        public void RemoveExpiredStories()
        {
            var expired = _repo.GetAll().Where(s => s.ExpiresAt <= DateTime.UtcNow).ToArray();
            foreach (var e in expired) _repo.Remove(e.Id);
        }

        /// <summary>
        /// Delete a story and cascade delete all its comments and reactions.
        /// </summary>
        public override void Delete(Guid id)
        {
            // Delete all comments on this story
            _comments.DeleteByTarget(id);
            
            // Delete all reactions on this story
            _reactions.DeleteByTarget(id);
            
            // Delete the story itself
            base.Delete(id);
        }
    }
}