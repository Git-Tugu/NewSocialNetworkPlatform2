using System;
using System.Collections.Generic;

namespace SocialNetworkPlatform.Models
{
    /// <summary>
    /// Marker interface for entities that support comments.
    /// Implementing types should have a CommentIds collection.
    /// </summary>
    public interface ICommentable : IIdentifiable
    {
        /// <summary>
        /// IDs of comments attached to this entity.
        /// </summary>
        List<Guid> CommentIds { get; }
    }
}
