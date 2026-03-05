using System;
using System.Collections.Generic;

namespace SocialNetworkPlatform.Models
{
    /// <summary>
    /// Ephemeral story content that expires after a time window.
    /// </summary>
    public class Story : ContentItem
    {
        /// <summary>
        /// URL or identifier for the media content of the story.
        /// </summary>
        public string MediaUrl { get; set; }

        /// <summary>
        /// When the story expires (UTC).
        /// </summary>
        public DateTime ExpiresAt { get; set; }

        /// <summary>
        /// Users who have viewed the story.
        /// </summary>
        public List<Guid> ViewedBy { get; } = new();
    }
}
