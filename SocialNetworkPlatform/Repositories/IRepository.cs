using System;
using System.Collections.Generic;
using SocialNetworkPlatform.Models;

namespace SocialNetworkPlatform.Repositories
{
    /// <summary>
    /// Generic repository contract for in-memory storage.
    /// </summary>
    public interface IRepository<T> where T : IIdentifiable
    {
        T? Get(Guid id);
        IEnumerable<T> GetAll();
        void Add(T item);
        void Remove(Guid id);
    }
}