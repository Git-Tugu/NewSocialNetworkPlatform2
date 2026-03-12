using System;
using System.Collections.Generic;
using System.Linq;
using SocialNetworkPlatform.DTOs;
using SocialNetworkPlatform.Models;
using SocialNetworkPlatform.Repositories;

namespace SocialNetworkPlatform.Services
{
    /// <summary>
    /// Service for managing reactions on any reactable content.
    /// Supports reactions on posts, reels, stories, events, and comments.
    /// </summary>
    public class ReactionService : IReactionService
    {
        private readonly ReactionRepo _repo;
        private readonly IRepository<Post> _postRepo;
        private readonly IRepository<Reel> _reelRepo;
        private readonly IRepository<Story> _storyRepo;
        private readonly IRepository<PageEvent> _eventRepo;
        private readonly IRepository<Comment> _commentRepo;

        public ReactionService(
            ReactionRepo repo,
            IRepository<Post> postRepo,
            IRepository<Reel> reelRepo,
            IRepository<Story> storyRepo,
            IRepository<PageEvent> eventRepo,
            IRepository<Comment> commentRepo)
        {
            _repo = repo ?? throw new ArgumentNullException(nameof(repo));
            _postRepo = postRepo ?? throw new ArgumentNullException(nameof(postRepo));
            _reelRepo = reelRepo ?? throw new ArgumentNullException(nameof(reelRepo));
            _storyRepo = storyRepo ?? throw new ArgumentNullException(nameof(storyRepo));
            _eventRepo = eventRepo ?? throw new ArgumentNullException(nameof(eventRepo));
            _commentRepo = commentRepo ?? throw new ArgumentNullException(nameof(commentRepo));
        }

        /// <summary>
        /// Create a reaction on a target entity (Post, Reel, Story, PageEvent, or Comment).
        /// </summary>
        public Reaction Create(ReactionDto dto)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            // Validate target exists and is reactable
            var target = GetReactableTarget(dto.TargetId)
                ?? throw new InvalidOperationException($"Target {dto.TargetId} not found or is not reactable");

            var reaction = new Reaction
            {
                AuthorId = dto.AuthorId,
                TargetId = dto.TargetId,
                Type = dto.Type
            };

            _repo.Add(reaction);
            target.ReactionIds.Add(reaction.Id);
            return reaction;
        }

        public Reaction? Get(Guid id) => _repo.Get(id);

        public IEnumerable<Reaction> GetAll() => _repo.GetAll();

        public IEnumerable<Reaction> GetByTarget(Guid targetId) =>
            _repo.GetAll().Where(r => r.TargetId == targetId).ToList();

        public void Delete(Guid id)
        {
            var reaction = _repo.Get(id);
            if (reaction == null) return;

            // Remove from target
            RemoveFromTarget(id, reaction.TargetId);
            _repo.Remove(id);
        }

        public void RemoveFromTarget(Guid reactionId, Guid targetId)
        {
            var target = GetReactableTarget(targetId);
            if (target != null)
            {
                target.ReactionIds.Remove(reactionId);
            }
        }

        /// <summary>
        /// Delete all reactions on a target entity (cascade deletion).
        /// </summary>
        public void DeleteByTarget(Guid targetId)
        {
            var reactions = GetByTarget(targetId).ToList();
            foreach (var reaction in reactions)
            {
                Delete(reaction.Id);
            }
        }

        /// <summary>
        /// Get the target entity if it's reactable. Searches all content types.
        /// </summary>
        private IReactable? GetReactableTarget(Guid targetId)
        {
            // Try each content type
            if (_postRepo.Get(targetId) is IReactable post) return post;
            if (_reelRepo.Get(targetId) is IReactable reel) return reel;
            if (_storyRepo.Get(targetId) is IReactable story) return story;
            if (_eventRepo.Get(targetId) is IReactable pageEvent) return pageEvent;
            if (_commentRepo.Get(targetId) is IReactable comment) return comment;

            return null;
        }
    }
}
