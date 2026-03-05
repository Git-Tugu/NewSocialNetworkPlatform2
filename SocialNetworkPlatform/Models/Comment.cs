using System;

namespace SocialNetworkPlatform.Models
{
    /// <summary>
    /// Comment on a post.
    /// </summary>
    public class Comment : ContentItem
    {
        public Guid PostId { get; set; }
        public string Text { get; set; }
    }
}