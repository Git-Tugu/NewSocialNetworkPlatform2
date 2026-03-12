using System;

namespace SocialNetworkPlatform.Models
{
    /// <summary>
    /// Abstract base for content-like entities (posts, comments, reactions).
    /// Provides common fields so derived types don't repeat them.
    /// </summary>
    public abstract class ContentItem : IIdentifiable
    {
        /// <inheritdoc />
        public Guid Id { get; } = Guid.NewGuid();

        /// <summary>
        /// Author user id.
        /// </summary>
        public Guid AuthorId { get; set; }

        /// <summary>
        /// UTC creation time.
        /// </summary>
        public DateTime CreatedAt { get; } = DateTime.UtcNow;
    }
}