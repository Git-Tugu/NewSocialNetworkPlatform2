using System;
using System.Collections.Generic;
using SocialNetworkPlatform.Models;
using SocialNetworkPlatform.Repositories;

namespace SocialNetworkPlatform.Services
{
    public class PageService : IPageService
    {
        private readonly PageRepo _repo;
        private readonly PageEventRepo _eventRepo;
        private readonly ICommentService _comments;
        private readonly IReactionService _reactions;

        public PageService(PageRepo repo, PageEventRepo eventRepo, ICommentService comments, IReactionService reactions)
        {
            _repo = repo ?? throw new ArgumentNullException(nameof(repo));
            _eventRepo = eventRepo ?? throw new ArgumentNullException(nameof(eventRepo));
            _comments = comments ?? throw new ArgumentNullException(nameof(comments));
            _reactions = reactions ?? throw new ArgumentNullException(nameof(reactions));
        }

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

        public Page? Get(Guid id) => _repo.Get(id);

        public IEnumerable<Page> GetAll() => _repo.GetAll();

        public void Follow(Guid pageId, Guid userId)
        {
            var p = _repo.Get(pageId);
            if (p == null) return;
            p.AddFollower(userId);
        }

        public void Unfollow(Guid pageId, Guid userId)
        {
            var p = _repo.Get(pageId);
            if (p == null) return;
            p.RemoveFollower(userId);
        }

        public PageEvent CreateEvent(Guid pageId, string title, string description, DateTime startsAt, DateTime endsAt, string location)
        {
            var p = _repo.Get(pageId) ?? throw new InvalidOperationException("Page not found");
            var e = p.AddEvent(title, description, startsAt, endsAt, location);
            _eventRepo.Add(e);
            return e;
        }

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