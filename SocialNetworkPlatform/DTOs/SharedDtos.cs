using System;

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
}
