using System;
using System.Collections.Generic;

namespace SocialNetworkPlatform.Models
{
    /// <summary>
    /// Represents an event created by a Page.
    /// </summary>
    /// <summary>
    /// Represents an event created by a <see cref="Page"/>.
    /// Instances are intended to be created by <see cref="Page"/> only.
    /// </summary>
    public class PageEvent : IIdentifiable, ICommentable, IReactable
    {
        /// <inheritdoc />
        public Guid Id { get; } = Guid.NewGuid();

        /// <summary>
        /// The owning page id.
        /// </summary>
        public Guid PageId { get; }

        /// <summary>
        /// Event title.
        /// </summary>
        public string Title { get; }

        /// <summary>
        /// Event description.
        /// </summary>
        public string Description { get; }

        /// <summary>
        /// Start time (UTC).
        /// </summary>
        public DateTime StartsAt { get; }

        /// <summary>
        /// End time (UTC).
        /// </summary>
        public DateTime EndsAt { get; }

        /// <summary>
        /// Event location.
        /// </summary>
        public string Location { get; }

        /// <summary>
        /// Users attending the event.
        /// </summary>
        public List<Guid> AttendeeIds { get; } = new();

        /// <summary>
        /// Comments attached to this event.
        /// </summary>
        public List<Guid> CommentIds { get; } = new();

        /// <summary>
        /// Reactions attached to this event.
        /// </summary>
        public List<Guid> ReactionIds { get; } = new();

        /// <summary>
        /// Internal constructor to ensure only code in this assembly (e.g. <see cref="Page"/>)
        /// can create events.
        /// </summary>
        /// <param name="startsAt">Event start time (UTC).</param>
        /// <param name="location">Event location.</param>
        /// <param name="endsAt">Event end time (UTC).</param>
        /// <param name="description">Event description.</param>
        /// <param name="pageId">Owning page id.</param>
        /// <param name="title">Event title.</param>
        internal PageEvent(Guid pageId, string title, string description, DateTime startsAt, DateTime endsAt, string location)
        {
            PageId = pageId;
            Title = title ?? string.Empty;
            Description = description ?? string.Empty;
            StartsAt = startsAt;
            EndsAt = endsAt;
            Location = location ?? string.Empty;
        }

        /// <summary>
        /// Adds an attendee to the event.
        /// </summary>
        public void AddAttendee(Guid userId)
        {
            if (userId == Guid.Empty) throw new ArgumentException("Invalid user id", nameof(userId));
            if (!AttendeeIds.Contains(userId)) AttendeeIds.Add(userId);
        }

        /// <summary>
        /// Removes an attendee from the event.
        /// </summary>
        public void RemoveAttendee(Guid userId) => AttendeeIds.Remove(userId);
    }
}