using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using SocialNetworkPlatform.Models;

namespace SocialNetworkPlatform.Repositories
{
    /// <summary>
    /// Thread-safe in-memory repository implementing IRepository. Generated with AI. I kept it 'Cause it seems interesting to me.
    /// </summary>
    public class InMemoryRepository<T> : IRepository<T> where T : IIdentifiable
    {
        private readonly ConcurrentDictionary<Guid, T> _store = new();

        public void Add(T item)
        {
            if (item == null) throw new ArgumentNullException(nameof(item));
            _store[item.Id] = item;
        }

        public T? Get(Guid id)
        {
            return _store.TryGetValue(id, out var v) ? v : default;
        }

        public IEnumerable<T> GetAll() => _store.Values.ToArray();

        public void Remove(Guid id)
        {
            _store.TryRemove(id, out _);
        }
    }
}