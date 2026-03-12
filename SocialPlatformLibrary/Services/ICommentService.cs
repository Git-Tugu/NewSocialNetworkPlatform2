using System;
using System.Collections.Generic;
using SocialNetworkPlatform.DTOs;
using SocialNetworkPlatform.Models;

namespace SocialNetworkPlatform.Services
{
    /// <summary>
    /// Generic service for managing comments on any commentable content.
    /// </summary>
    public interface ICommentService
    {
        /// <summary>
        /// Create a comment on a target entity.
        /// </summary>
        Comment Create(CommentDto dto);

        /// <summary>
        /// Get a specific comment by id.
        /// </summary>
        Comment? Get(Guid id);

        /// <summary>
        /// Get all comments.
        /// </summary>
        IEnumerable<Comment> GetAll();

        /// <summary>
        /// Get all comments on a specific target (post, reel, story, event).
        /// </summary>
        IEnumerable<Comment> GetByTarget(Guid targetId);

        /// <summary>
        /// Delete a comment.
        /// </summary>
        void Delete(Guid id);

        /// <summary>
        /// Remove a comment from its target entity.
        /// </summary>
        void RemoveFromTarget(Guid commentId, Guid targetId);

        /// <summary>
        /// Delete all comments on a target entity (cascade deletion).
        /// Used when the target itself is being deleted.
        /// </summary>
        void DeleteByTarget(Guid targetId);
    }
}
