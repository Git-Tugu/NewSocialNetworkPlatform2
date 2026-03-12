using System;
using System.Collections.Generic;
using SocialNetworkPlatform.Models;
using SocialNetworkPlatform.Repositories;

namespace SocialNetworkPlatform.Services
{
    /// <summary>
    /// Manages pages, their followers, and events. Deleting a page cascades deletes all its events and their comments/reactions.
    /// </summary>
    public class PageService : IPageService
    {
        private readonly PageRepo _repo;
        private readonly PageEventRepo _eventRepo;
        private readonly ICommentService _comments;
        private readonly IReactionService _reactions;

        /// <summary>
        /// Generic constructor for PageService with dependency injection. All dependencies are required and cannot be null.
        /// </summary>
        /// <param name="repo">Repo</param>
        /// <param name="eventRepo">Event repo</param>
        /// <param name="comments">Comments</param>
        /// <param name="reactions">Reactions</param>
        /// <exception cref="ArgumentNullException"></exception>
        public PageService(PageRepo repo, PageEventRepo eventRepo, ICommentService comments, IReactionService reactions)
        {
            _repo = repo ?? throw new ArgumentNullException(nameof(repo));
            _eventRepo = eventRepo ?? throw new ArgumentNullException(nameof(eventRepo));
            _comments = comments ?? throw new ArgumentNullException(nameof(comments));
            _reactions = reactions ?? throw new ArgumentNullException(nameof(reactions));
        }

        /// <summary>
        /// Creates a new page with the given name, description, and owner ID. The new page is added to the repository and returned.
        /// </summary>
        /// <param name="name">Page name</param>
        /// <param name="description">Page description</param>
        /// <param name="ownerId">Page owner ID</param>
        /// <returns></returns>
        public Page Create(string name, string description, Guid ownerId)
        {
            var p = new Page
            {
                Name = name,
                Description = description,
                OwnerId = ownerId
            };
            _repo.Add(p);
            return p;
        }

        /// <summary>
        /// Retrieves a page by its ID. Returns null if the page does not exist.
        /// </summary>
        /// <param name="id">Page ID</param>
        /// <returns></returns>
        public Page? Get(Guid id) => _repo.Get(id);

        /// <summary>
        /// Retrieves all pages in the repository. Returns an empty collection if there are no pages.
        /// </summary>
        /// <returns>All pages</returns>
        public IEnumerable<Page> GetAll() => _repo.GetAll();

        /// <summary>
        /// Registers a user as a follower of a page. If the page does not exist, the method does nothing. If the user is already a follower, it does nothing.
        /// </summary>
        /// <param name="pageId">Page ID</param>
        /// <param name="userId"> User ID</param>
        public void Follow(Guid pageId, Guid userId)
        {
            var p = _repo.Get(pageId);
            if (p == null) return;
            p.AddFollower(userId);
        }

        /// <summary>
        /// Detaches a user from following a page. If the page does not exist, the method does nothing. If the user is not a follower, it does nothing.
        /// </summary>
        /// <param name="pageId">Page ID</param>
        /// <param name="userId">User ID</param>
        public void Unfollow(Guid pageId, Guid userId)
        {
            var p = _repo.Get(pageId);
            if (p == null) return;
            p.RemoveFollower(userId);
        }

        /// <summary>
        /// Creates a new event associated with a page. The event is added to the event repository and 
        /// its ID is added to the page's list of events. If the page does not exist, an exception is thrown.
        /// </summary>
        /// <param name="pageId">Page ID</param>
        /// <param name="title">Event title</param>
        /// <param name="description">Event Description</param>
        /// <param name="startsAt">Event start date</param>
        /// <param name="endsAt">Event end date</param>
        /// <param name="location">Location which event will be held</param>
        /// <returns>Event</returns>
        /// <exception cref="InvalidOperationException"></exception>
        public PageEvent CreateEvent(Guid pageId, string title, string description, DateTime startsAt, DateTime endsAt, string location)
        {
            var p = _repo.Get(pageId) ?? throw new InvalidOperationException("Page not found");
            var e = p.AddEvent(title, description, startsAt, endsAt, location);
            _eventRepo.Add(e);
            return e;
        }

        /// <summary>
        /// Deletes an event by its ID. This method also cascades deletes all comments and reactions
        /// associated with the event. If the event does not exist, the method does nothing.
        /// </summary>
        /// <param name="eventId"></param>
        public void DeleteEvent(Guid eventId)
        {
            // Delete all comments on this event
            _comments.DeleteByTarget(eventId);
            
            // Delete all reactions on this event
            _reactions.DeleteByTarget(eventId);
            
            _eventRepo.Remove(eventId);
            // Also remove from any page that references it
            foreach (var p in _repo.GetAll())
            {
                if (p.EventIds.Contains(eventId)) p.EventIds.Remove(eventId);
            }
        }

        /// <summary>
        /// Delete a page and cascade delete all its events with their comments and reactions.
        /// </summary>
        public void Delete(Guid pageId)
        {
            var page = _repo.Get(pageId);
            if (page == null) return;

            // Delete all events and their comments/reactions
            var eventIds = page.EventIds.ToList();
            foreach (var eventId in eventIds)
            {
                DeleteEvent(eventId);
            }

            // Delete the page itself
            _repo.Remove(pageId);
        }
    }
}