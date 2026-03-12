using System;
using System.Collections.Generic;

namespace SocialNetworkPlatform.Models
{
    /// <summary>
    /// Marker interface for entities that support reactions.
    /// Implementing types should have a ReactionIds collection.
    /// </summary>
    public interface IReactable : IIdentifiable
    {
        /// <summary>
        /// IDs of reactions attached to this entity.
        /// </summary>
        List<Guid> ReactionIds { get; }
    }
}
