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

        public PageService(PageRepo repo, PageEventRepo eventRepo)
        {
            _repo = repo ?? throw new ArgumentNullException(nameof(repo));
            _eventRepo = eventRepo ?? throw new ArgumentNullException(nameof(eventRepo));
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
    }
}