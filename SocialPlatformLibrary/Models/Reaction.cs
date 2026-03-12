using System;
using System.Collections.Generic;
using SocialNetworkPlatform.Enums;

namespace SocialNetworkPlatform.Models
{
    /// <summary>
    /// Reaction to any reactable content (post, comment, reel, story, event).
    /// </summary>
    public class Reaction : ContentItem, IReactable
    {
        public Guid TargetId { get; set; }
        public ReactionType Type { get; set; }

        /// <summary>
        /// Reactions can also be reacted to.
        /// </summary>
        public List<Guid> ReactionIds { get; } = new();
    }
}