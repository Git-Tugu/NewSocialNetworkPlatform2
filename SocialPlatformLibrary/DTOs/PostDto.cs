using System;

namespace SocialNetworkPlatform.DTOs
{
    /// <summary>
    /// Data transfer object for creating posts.
    /// </summary>
    public record PostDto(Guid AuthorId, string Content);
}