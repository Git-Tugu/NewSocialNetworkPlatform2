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


        /// <summary>
        /// Get a reaction by ID. Returns null if not found.Rreactions are 
        /// not directly associated with a single content type, so we search the reaction repository directly.
        /// </summary>
        /// <param name="id">Reaction ID</param>
        /// <returns>Reaction</returns>
        public Reaction? Get(Guid id) => _repo.Get(id);


        /// <summary>
        /// Return all reactions. In a real application, this would likely be paginated and filtered by target or author.
        /// </summary>
        /// <returnsReactions></returns>
        public IEnumerable<Reaction> GetAll() => _repo.GetAll();


        /// <summary>
        /// Get all reactions for a specific target entity (Post, Reel, Story, PageEvent, or Comment). Returns empty if none found.
        /// </summary>
        /// <param name="targetId">Reaction ID</param>
        /// <returns>Reaction</returns>
        public IEnumerable<Reaction> GetByTarget(Guid targetId) =>
            _repo.GetAll().Where(r => r.TargetId == targetId).ToList();


        /// <summary>
        /// Delete a reaction by ID. Also removes the reaction ID from the target entity's
        /// list of reactions. If the reaction doesn't exist, does nothing.
        /// </summary>
        /// <param name="id">Reaction ID</param>
        public void Delete(Guid id)
        {
            var reaction = _repo.Get(id);
            if (reaction == null) return;

            // Remove from target
            RemoveFromTarget(id, reaction.TargetId);
            _repo.Remove(id);
        }


        /// <summary>
        /// Remove a reaction ID from the target entity's list of reactions. 
        /// This is used when deleting a reaction to maintain data integrity.
        /// </summary>
        /// <param name="reactionId"Reaction ID></param>
        /// <param name="targetId">Target Media ID</param>
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
