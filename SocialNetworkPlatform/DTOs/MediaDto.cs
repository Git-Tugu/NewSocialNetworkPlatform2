using System;

namespace SocialNetworkPlatform.DTOs
{
    /// <summary>
    /// DTO for media uploads like reels and stories.
    /// </summary>
    public record MediaDto(Guid AuthorId, string MediaUrl, TimeSpan? Duration = null, DateTime? ExpiresAt = null);
}