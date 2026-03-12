using System;
using System.Collections.Generic;
using SocialNetworkPlatform.DTOs;
using SocialNetworkPlatform.Models;

namespace SocialNetworkPlatform.Services
{
    /// <summary>
    /// Generic service for managing reactions on any reactable content.
    /// </summary>
    public interface IReactionService
    {
        /// <summary>
        /// Create a reaction on a target entity.
        /// </summary>
        Reaction Create(ReactionDto dto);

        /// <summary>
        /// Get a specific reaction by id.
        /// </summary>
        Reaction? Get(Guid id);

        /// <summary>
        /// Get all reactions.
        /// </summary>
        IEnumerable<Reaction> GetAll();

        /// <summary>
        /// Get all reactions on a specific target (post, reel, story, event, comment).
        /// </summary>
        IEnumerable<Reaction> GetByTarget(Guid targetId);

        /// <summary>
        /// Delete a reaction.
        /// </summary>
        void Delete(Guid id);

        /// <summary>
        /// Remove a reaction from its target entity.
        /// </summary>
        void RemoveFromTarget(Guid reactionId, Guid targetId);

        /// <summary>
        /// Delete all reactions on a target entity (cascade deletion).
        /// Used when the target itself is being deleted.
        /// </summary>
        void DeleteByTarget(Guid targetId);
    }
}
