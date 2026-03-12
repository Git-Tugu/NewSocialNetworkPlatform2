using System;
using SocialNetworkPlatform.Enums;

namespace SocialNetworkPlatform.DTOs
{
    /// <summary>
    /// Data transfer object for creating posts.
    /// </summary>
    public record PostDto(Guid AuthorId, string Content);

    /// <summary>
    /// DTO for media uploads like reels and stories.
    /// </summary>
    public record MediaDto(Guid AuthorId, string MediaUrl, TimeSpan? Duration = null, DateTime? ExpiresAt = null);

    /// <summary>
    /// DTO for creating comments on any commentable content.
    /// </summary>
    public record CommentDto(Guid AuthorId, Guid TargetId, string Text);

    /// <summary>
    /// DTO for creating reactions on any reactable content.
    /// </summary>
    public record ReactionDto(Guid AuthorId, Guid TargetId, ReactionType Type);
}
