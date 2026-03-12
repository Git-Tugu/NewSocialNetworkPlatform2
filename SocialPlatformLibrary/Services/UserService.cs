using System;
using System.Collections.Generic;
using SocialNetworkPlatform.Models;
using SocialNetworkPlatform.Repositories;

namespace SocialNetworkPlatform.Services
{
    /// <summary>
    /// Basic user service implementing IUserService.
    /// </summary>
    public class UserService : IUserService
    {
        private readonly UserRepo _repo;

        public UserService(UserRepo repo)
        {
            _repo = repo ?? throw new ArgumentNullException(nameof(repo));
        }

        public User Create(string username, string displayName, byte age)
        {
            var u = new User(username, displayName, age);
            _repo.Add(u);
            return u;
        }

        public User? Get(Guid id) => _repo.Get(id);

        public IEnumerable<User> GetAll() => _repo.GetAll();

        public void Delete(Guid id)
        {
            _repo.Remove(id);
        }

        public User? Update(Guid id, string? username = null, string? displayName = null, byte? age = null)
        {
            var u = _repo.Get(id);
            if (u == null) return null;
            if (username != null) u.Username = username;
            if (displayName != null) u.DisplayName = displayName;
            if (age.HasValue) return new User(u.Username, u.DisplayName, age.Value); // simplistic: return new user copy
            return u;
        }

        public void Follow(Guid followerId, Guid followeeId)
        {
            // For demo purposes follow is equivalent to adding friend only one-way
            var follower = _repo.Get(followerId);
            if (follower == null) return;
            follower.AddFriend(followeeId);
        }

        public void Unfollow(Guid followerId, Guid followeeId)
        {
            var follower = _repo.Get(followerId);
            if (follower == null) return;
            follower.RemoveFriend(followeeId);
        }

        public void Befriend(Guid aId, Guid bId)
        {
            var a = _repo.Get(aId);
            var b = _repo.Get(bId);
            if (a == null || b == null) return;
            a.AddFriend(bId);
            b.AddFriend(aId);
        }

        public void Unfriend(Guid aId, Guid bId)
        {
            var a = _repo.Get(aId);
            var b = _repo.Get(bId);
            if (a == null || b == null) return;
            a.RemoveFriend(bId);
            b.RemoveFriend(aId);
        }

        public IEnumerable<User> GetFriends(Guid userId)
        {
            var u = _repo.Get(userId);
            if (u == null) return Array.Empty<User>();
            var friends = new List<User>();
            foreach (var fid in u.FriendIds)
            {
                var f = _repo.Get(fid);
                if (f != null) friends.Add(f);
            }
            return friends;
        }
    }
}