using System;
using SocialNetworkPlatform.Enums;

namespace SocialNetworkPlatform.Models
{
    /// <summary>
    /// Reaction to a post or comment.
    /// </summary>
    public class Reaction : ContentItem
    {
        public Guid TargetId { get; set; }
        public ReactionType Type { get; set; }
    }
}