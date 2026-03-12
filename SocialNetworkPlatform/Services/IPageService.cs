using SocialNetworkPlatform.Models;
using System;
using System.Collections.Generic;

namespace SocialNetworkPlatform.Services
{
    public interface IPageService
    {
        Page Create(string name, string description, Guid ownerId);
        Page? Get(Guid id);
        IEnumerable<Page> GetAll();
        void Follow(Guid pageId, Guid userId);
        void Unfollow(Guid pageId, Guid userId);
        PageEvent CreateEvent(Guid pageId, string title, string description, DateTime startsAt, DateTime endsAt, string location);
        void DeleteEvent(Guid eventId);
        void Delete(Guid pageId);
    }
}