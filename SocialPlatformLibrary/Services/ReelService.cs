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
        private readonly ICommentService _comments;
        private readonly IReactionService _reactions;

        public ReelService(ReelRepo repo, ICommentService comments, IReactionService reactions)
            : base(repo, dto => new Reel { AuthorId = dto.AuthorId, MediaUrl = dto.MediaUrl ?? string.Empty, Duration = dto.Duration ?? TimeSpan.Zero })
        {
            _typedRepo = repo ?? throw new ArgumentNullException(nameof(repo));
            _comments = comments ?? throw new ArgumentNullException(nameof(comments));
            _reactions = reactions ?? throw new ArgumentNullException(nameof(reactions));
        }

        public void AddView(Guid reelId, Guid userId)
        {
            var r = _typedRepo.Get(reelId);
            if (r == null) return;
            if (!r.ViewedBy.Contains(userId)) r.ViewedBy.Add(userId);
        }

        /// <summary>
        /// Delete a reel and cascade delete all its comments and reactions.
        /// </summary>
        public override void Delete(Guid id)
        {
            // Delete all comments on this reel
            _comments.DeleteByTarget(id);
            
            // Delete all reactions on this reel
            _reactions.DeleteByTarget(id);
            
            // Delete the reel itself
            base.Delete(id);
        }
    }
}