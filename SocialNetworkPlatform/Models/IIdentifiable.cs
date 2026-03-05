using System;

namespace SocialNetworkPlatform.Models
{
    /// <summary>
    /// Provides an Id for entities.
    /// </summary>
    public interface IIdentifiable
    {
        Guid Id { get; }
    }
}