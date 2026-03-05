using System;
using System.Collections.Generic;
using System.Linq;

namespace SocialNetworkPlatform.Models
{
    /// <summary>
    /// Represents a platform user.
    /// </summary>
    public class User : IUser
    {
        private readonly HashSet<Guid> _friendIds;

        /// <inheritdoc />
        public Guid Id { get; private set; } = Guid.NewGuid();

        /// <inheritdoc />
        public string Username { get; set; }

        /// <inheritdoc />
        public string DisplayName { get; set; }

        private readonly byte _age;
        /// <inheritdoc />
        public byte Age => _age;

        /// <inheritdoc />
        public IReadOnlyCollection<Guid> FriendIds => Array.AsReadOnly(_friendIds.ToArray());

        /// <summary>
        /// Primary constructor.
        /// </summary>
        public User(string username, string displayName, byte age)
            : this(username, displayName, age, Guid.NewGuid(), new HashSet<Guid>())
        {
        }

        private User(string username, string displayName, byte age, Guid id, HashSet<Guid> friendIds)
        {
            Username = username ?? throw new ArgumentNullException(nameof(username));
            DisplayName = displayName ?? throw new ArgumentNullException(nameof(displayName));
            _age = age;
            Id = id;
            _friendIds = new HashSet<Guid>(friendIds ?? throw new ArgumentNullException(nameof(friendIds)));
        }

        /// <inheritdoc />
        public void AddFriend(Guid userId)
        {
            if (userId == Guid.Empty) throw new ArgumentException("Invalid user id", nameof(userId));
            _friendIds.Add(userId);
        }

        /// <inheritdoc />
        public void RemoveFriend(Guid userId)
        {
            _friendIds.Remove(userId);
        }

        // Functional/immutable-style APIs

        /// <summary>
        /// Returns a copy of this user with the provided friend added.
        /// Does not mutate the original instance.
        /// </summary>
        public User WithFriendAdded(Guid userId)
        {
            if (userId == Guid.Empty) throw new ArgumentException("Invalid user id", nameof(userId));
            var copyFriends = new HashSet<Guid>(_friendIds) { userId };
            return new User(Username, DisplayName, _age, Id, copyFriends);
        }

        /// <summary>
        /// Returns a copy of this user with the provided friend removed.
        /// Does not mutate the original instance.
        /// </summary>
        public User WithFriendRemoved(Guid userId)
        {
            var copyFriends = new HashSet<Guid>(_friendIds);
            copyFriends.Remove(userId);
            return new User(Username, DisplayName, _age, Id, copyFriends);
        }

        /// <summary>
        /// Returns a copy with a changed display name.
        /// </summary>
        public User WithDisplayName(string newDisplayName) => new User(Username, newDisplayName ?? DisplayName, _age, Id, _friendIds);

        /// <summary>
        /// Returns a copy with a changed username.
        /// </summary>
        public User WithUsername(string newUsername) => new User(newUsername ?? Username, DisplayName, _age, Id, _friendIds);

        /// <summary>
        /// Returns a copy with a changed age.
        /// </summary>
        public User WithAge(byte newAge) => new User(Username, DisplayName, newAge, Id, _friendIds);
    }
}
