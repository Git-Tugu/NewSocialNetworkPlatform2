using System;
using System.Collections.Generic;
using SocialNetworkPlatform.Models;
using SocialNetworkPlatform.Repositories;

namespace SocialNetworkPlatform.Services
{
    /// <summary>
    /// Generic CRUD service for simple entities. Subclasses can extend with type-specific behavior.
    /// </summary>
    public class CrudService<T, TDto> where T : IIdentifiable
    {
        protected readonly IRepository<T> _repo;
        private readonly Func<TDto, T> _factory;

        public CrudService(IRepository<T> repo, Func<TDto, T> factory)
        {
            _repo = repo ?? throw new ArgumentNullException(nameof(repo));
            _factory = factory ?? throw new ArgumentNullException(nameof(factory));
        }

        public virtual T Create(TDto dto)
        {
            var item = _factory(dto);
            _repo.Add(item);
            return item;
        }

        public virtual T? Get(Guid id) => _repo.Get(id);

        public virtual IEnumerable<T> GetAll() => _repo.GetAll();

        public virtual void Delete(Guid id) => _repo.Remove(id);
    }
}
