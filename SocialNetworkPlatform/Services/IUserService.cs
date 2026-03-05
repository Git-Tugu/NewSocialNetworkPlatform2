using SocialNetworkPlatform.Models;
using System;
using System.Collections.Generic;

namespace SocialNetworkPlatform.Services
{
    /// <summary>
    /// Operations for managing users.
    /// </summary>
    public interface IUserService
    {
        User Create(string username, string displayName, byte age);
        User? Get(Guid id);
        IEnumerable<User> GetAll();
        void Delete(Guid id);
        User? Update(Guid id, string? username = null, string? displayName = null, byte? age = null);

        // Follow is one-way; Befriend adds both users as friends.
        void Follow(Guid followerId, Guid followeeId);
        void Unfollow(Guid followerId, Guid followeeId);

        void Befriend(Guid aId, Guid bId);
        void Unfriend(Guid aId, Guid bId);

        IEnumerable<User> GetFriends(Guid userId);
    }
}