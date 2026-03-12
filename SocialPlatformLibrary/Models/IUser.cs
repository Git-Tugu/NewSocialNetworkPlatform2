using System;
using System.Collections.Generic;

namespace SocialNetworkPlatform.Models
{
    /// <summary>
    /// Public contract for user-like behavior.
    /// </summary>
    public interface IUser : IIdentifiable
    {
        string Username { get; set; }
        string DisplayName { get; set; }
        byte Age { get; }
        IReadOnlyCollection<Guid> FriendIds { get; }

        void AddFriend(Guid userId);
        void RemoveFriend(Guid userId);
    }
}