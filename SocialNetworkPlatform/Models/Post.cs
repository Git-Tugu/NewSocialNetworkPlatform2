using System;
using System.Collections.Generic;
using SocialNetworkPlatform.Enums;

namespace SocialNetworkPlatform.Models
{
    /// <summary>
    /// Represents a user-generated post.
    /// </summary>
    public class Post : ContentItem, ICommentable, IReactable
    {
        /// <summary>
        /// Textual content of the post.
        /// </summary>
        public string Content { get; set; }

        /// <summary>
        /// Visibility of the post (public, friends, private).
        /// </summary>
        public Visibility Visibility { get; set; } = Visibility.Public;

        /// <summary>
        /// If shared, original post id this was shared from.
        /// </summary>
        public Guid? SharedFrom { get; set; }

        /// <summary>
        /// Comments attached to the post.
        /// </summary>
        public List<Guid> CommentIds { get; } = new();

        /// <summary>
        /// Reactions attached to the post.
        /// </summary>
        public List<Guid> ReactionIds { get; } = new();

        /// <summary>
        /// Edit the content of the post. Caller should ensure authorization.
        /// </summary>
        public void Edit(string newContent)
        {
            Content = newContent ?? Content;
        }

        /// <summary>
        /// Change visibility of the post.
        /// </summary>
        public void ChangeVisibility(Visibility visibility)
        {
            Visibility = visibility;
        }

        /// <summary>
        /// Mark this post as shared from another post id.
        /// </summary>
        public void ShareFrom(Guid originalPostId)
        {
            if (originalPostId == Guid.Empty) throw new ArgumentException("Invalid original post id", nameof(originalPostId));
            SharedFrom = originalPostId;
        }
    }
}