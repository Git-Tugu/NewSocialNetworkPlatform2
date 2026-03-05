using System;
using System.Collections.Generic;

namespace SocialNetworkPlatform.Models
{
    /// <summary>
    /// Short video content (Reel) uploaded by a user.
    /// </summary>
    public class Reel : ContentItem
    {
        /// <summary>
        /// URL or identifier for the media.
        /// </summary>
        public string MediaUrl { get; set; }

        /// <summary>
        /// Duration of the reel.
        /// </summary>
        public TimeSpan Duration { get; set; }

        /// <summary>
        /// Users who viewed the reel.
        /// </summary>
        public List<Guid> ViewedBy { get; } = new();

        /// <summary>
        /// Reactions attached to this reel.
        /// </summary>
        public List<Guid> ReactionIds { get; } = new();
    }
}