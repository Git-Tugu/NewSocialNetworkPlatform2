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


        /// <summary>
        /// Creates a new user with the given username, display name and age, adds it to the repository and returns it.
        /// </summary>
        /// <param name="username">User name</param>
        /// <param name="displayName">Nickname</param>
        /// <param name="age">Age</param>
        /// <returns></returns>
        public User Create(string username, string displayName, byte age)
        {
            var u = new User(username, displayName, age);
            _repo.Add(u);
            return u;
        }

        /// <summary>
        /// Gets user by id. Returns null if not found.
        /// </summary>
        /// <param name="id">User ID</param>
        /// <returns></returns>
        public User? Get(Guid id) => _repo.Get(id);


        /// <summary>
        /// Gets all users in the repository. Returns empty collection if no users exist.
        /// </summary>
        /// <returns>Users</returns>
        public IEnumerable<User> GetAll() => _repo.GetAll();

        /// <summary>
        /// Deletes a user by id. If the user does not exist, does nothing. 
        /// This does not cascade delete any related data (e.g. posts, comments, friendships).
        /// </summary>
        /// <param name="id">User ID</param>
        public void Delete(Guid id)
        {
            _repo.Remove(id);
        }

        /// <summary>
        /// Updates a user's information. Only non-null parameters are updated. Returns the updated user, or null if user not found.
        /// </summary>
        /// <param name="id">User ID</param>
        /// <param name="username">New User Name</param>
        /// <param name="displayName">New nickname</param>
        /// <param name="age">new age</param>
        /// <returns></returns>
        public User? Update(Guid id, string? username = null, string? displayName = null, byte? age = null)
        {
            var u = _repo.Get(id);
            if (u == null) return null;
            if (username != null) u.Username = username;
            if (displayName != null) u.DisplayName = displayName;
            if (age.HasValue) return new User(u.Username, u.DisplayName, age.Value); // simplistic: return new user copy
            return u;
        }


        /// <summary>
        /// Follows another user. For demo purposes this is implemented as adding the followee as a 
        /// friend to the follower, but in a real application this would likely be a separate "follow"
        /// relationship that is not necessarily symmetric like friendship.
        /// </summary>
        /// <param name="followerId">Follower ID</param>
        /// <param name="followeeId">Followee ID</param>
        public void Follow(Guid followerId, Guid followeeId)
        {
            // For demo purposes follow is equivalent to adding friend only one-way
            var follower = _repo.Get(followerId);
            if (follower == null) return;
            follower.AddFriend(followeeId);
        }


        /// <summary>
        /// Unfollows another user. For demo purposes this is implemented as removing the followee from the follower's
        /// </summary>
        /// <param name="followerId">Follower ID</param>
        /// <param name="followeeId">Followee ID</param>
        public void Unfollow(Guid followerId, Guid followeeId)
        {
            var follower = _repo.Get(followerId);
            if (follower == null) return;
            follower.RemoveFriend(followeeId);
        }


        /// <summary>
        /// Friends two users. This is a symmetric relationship, so both users will have each other in their friend lists.
        /// </summary>
        /// <param name="aId">User ID 1</param>
        /// <param name="bId">User ID 2</param>
        public void Befriend(Guid aId, Guid bId)
        {
            var a = _repo.Get(aId);
            var b = _repo.Get(bId);
            if (a == null || b == null) return;
            a.AddFriend(bId);
            b.AddFriend(aId);
        }


        /// <summary>
        /// Unfriends two users. This is a symmetric relationship, so both users will have each other removed from their friend lists.
        /// </summary>
        /// <param name="aId">User ID 1</param>
        /// <param name="bId">User ID 2</param>
        public void Unfriend(Guid aId, Guid bId)
        {
            var a = _repo.Get(aId);
            var b = _repo.Get(bId);
            if (a == null || b == null) return;
            a.RemoveFriend(bId);
            b.RemoveFriend(aId);
        }


        /// <summary>
        /// Returns a list of User objects representing the friends of the
        /// user with the given ID. If the user does not exist, returns an empty collection.
        /// </summary>
        /// <param name="userId">USer ID</param>
        /// <returns></returns>
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