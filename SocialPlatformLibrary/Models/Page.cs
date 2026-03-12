using System;
using System.Collections.Generic;

namespace SocialNetworkPlatform.Models
{
    /// <summary>
    /// Represents a public Page (brand, company, community) on the platform.
    /// </summary>
    public class Page : IIdentifiable
    {
        public Guid Id { get; } = Guid.NewGuid();

        public string Name { get; set; }

        public string Description { get; set; }

        public Guid OwnerId { get; set; }

        public List<Guid> FollowerIds { get; } = new();

        public List<Guid> PostIds { get; } = new();

        public List<Guid> EventIds { get; } = new();

        /// <summary>
        /// Create an event for this page. The created <see cref="PageEvent"/> will have an internal
        /// constructor and its id will be added to this page's EventIds collection.
        /// </summary>
        public PageEvent AddEvent(string title, string description, DateTime startsAt, DateTime endsAt, string location)
        {
            var e = new PageEvent(this.Id, title, description, startsAt, endsAt, location);
            EventIds.Add(e.Id);
            return e;
        }

        /// <summary>
        /// Follow the page.
        /// </summary>
        public void AddFollower(Guid userId)
        {
            if (userId == Guid.Empty) throw new ArgumentException("Invalid user id", nameof(userId));
            if (!FollowerIds.Contains(userId)) FollowerIds.Add(userId);
        }

        /// <summary>
        /// Remove follower from the page.
        /// </summary>
        public void RemoveFollower(Guid userId)
        {
            FollowerIds.Remove(userId);
        }
    }
}