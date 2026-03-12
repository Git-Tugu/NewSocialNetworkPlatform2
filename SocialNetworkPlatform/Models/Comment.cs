using System;
using System.Collections.Generic;

namespace SocialNetworkPlatform.Models
{
    /// <summary>
    /// Comment on any commentable content (post, reel, story, event).
    /// Comments can also be reacted to.
    /// </summary>
    public class Comment : ContentItem, IReactable
    {
        /// <summary>
        /// ID of the target entity being commented on (Post, Reel, Story, or PageEvent).
        /// </summary>
        public Guid TargetId { get; set; }

        /// <summary>
        /// Text content of the comment.
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// Reactions attached to this comment.
        /// </summary>
        public List<Guid> ReactionIds { get; } = new();
    }
}