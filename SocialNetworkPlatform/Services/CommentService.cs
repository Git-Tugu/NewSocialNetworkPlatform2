using System;
using System.Collections.Generic;
using System.Linq;
using SocialNetworkPlatform.DTOs;
using SocialNetworkPlatform.Models;
using SocialNetworkPlatform.Repositories;

namespace SocialNetworkPlatform.Services
{
    /// <summary>
    /// Service for managing comments on any commentable content.
    /// Supports comments on posts, reels, stories, and events.
    /// </summary>
    public class CommentService : ICommentService
    {
        private readonly CommentRepo _repo;
        private readonly IRepository<Post> _postRepo;
        private readonly IRepository<Reel> _reelRepo;
        private readonly IRepository<Story> _storyRepo;
        private readonly IRepository<PageEvent> _eventRepo;
        private readonly IReactionService _reactions;

        public CommentService(
            CommentRepo repo,
            IRepository<Post> postRepo,
            IRepository<Reel> reelRepo,
            IRepository<Story> storyRepo,
            IRepository<PageEvent> eventRepo,
            IReactionService reactions = null)
        {
            _repo = repo ?? throw new ArgumentNullException(nameof(repo));
            _postRepo = postRepo ?? throw new ArgumentNullException(nameof(postRepo));
            _reelRepo = reelRepo ?? throw new ArgumentNullException(nameof(reelRepo));
            _storyRepo = storyRepo ?? throw new ArgumentNullException(nameof(storyRepo));
            _eventRepo = eventRepo ?? throw new ArgumentNullException(nameof(eventRepo));
            _reactions = reactions;
        }

        /// <summary>
        /// Create a comment on a target entity (Post, Reel, Story, or PageEvent).
        /// </summary>
        public Comment Create(CommentDto dto)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));
            if (string.IsNullOrWhiteSpace(dto.Text))
                throw new ArgumentException("Comment text cannot be empty", nameof(dto.Text));

            // Validate target exists and is commentable
            var target = GetCommentableTarget(dto.TargetId)
                ?? throw new InvalidOperationException($"Target {dto.TargetId} not found or is not commentable");

            var comment = new Comment
            {
                AuthorId = dto.AuthorId,
                TargetId = dto.TargetId,
                Text = dto.Text
            };

            _repo.Add(comment);
            target.CommentIds.Add(comment.Id);
            return comment;
        }

        public Comment? Get(Guid id) => _repo.Get(id);

        public IEnumerable<Comment> GetAll() => _repo.GetAll();

        public IEnumerable<Comment> GetByTarget(Guid targetId) =>
            _repo.GetAll().Where(c => c.TargetId == targetId).ToList();

        public void Delete(Guid id)
        {
            var comment = _repo.Get(id);
            if (comment == null) return;

            // Delete all reactions on this comment (if reactions service is available)
            if (_reactions != null)
            {
                _reactions.DeleteByTarget(id);
            }

            // Remove from target
            RemoveFromTarget(id, comment.TargetId);
            _repo.Remove(id);
        }

        public void RemoveFromTarget(Guid commentId, Guid targetId)
        {
            var target = GetCommentableTarget(targetId);
            if (target != null)
            {
                target.CommentIds.Remove(commentId);
            }
        }

        /// <summary>
        /// Delete all comments on a target entity (cascade deletion).
        /// </summary>
        public void DeleteByTarget(Guid targetId)
        {
            var comments = GetByTarget(targetId).ToList();
            foreach (var comment in comments)
            {
                Delete(comment.Id);
            }
        }

        /// <summary>
        /// Get the target entity if it's commentable. Searches all content types.
        /// </summary>
        private ICommentable? GetCommentableTarget(Guid targetId)
        {
            // Try each content type
            if (_postRepo.Get(targetId) is ICommentable post) return post;
            if (_reelRepo.Get(targetId) is ICommentable reel) return reel;
            if (_storyRepo.Get(targetId) is ICommentable story) return story;
            if (_eventRepo.Get(targetId) is ICommentable pageEvent) return pageEvent;

            return null;
        }
    }
}
