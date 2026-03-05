using System;
using System.Collections.Generic;
using SocialNetworkPlatform.DTOs;
using SocialNetworkPlatform.Models;
using SocialNetworkPlatform.Repositories;

namespace SocialNetworkPlatform.Services
{
    /// <summary>
    /// Basic post service implementing IPostService.
    /// </summary>
    public class PostService : IPostService
    {
        private readonly PostRepo _repo;
        private readonly UserRepo _users;

        public PostService(PostRepo repo, UserRepo users)
        {
            _repo = repo ?? throw new ArgumentNullException(nameof(repo));
            _users = users ?? throw new ArgumentNullException(nameof(users));
        }

        public Post Create(PostDto dto)
        {
            var post = new Post
            {
                AuthorId = dto.AuthorId,
                Content = dto.Content ?? string.Empty
            };
            _repo.Add(post);
            return post;
        }

        public Post? Get(Guid id) => _repo.Get(id);

        public IEnumerable<Post> GetAll() => _repo.GetAll();

        public void Delete(Guid id)
        {
            _repo.Remove(id);
        }

        public void Edit(Guid id, string newContent)
        {
            var p = _repo.Get(id);
            if (p == null) return;
            p.Edit(newContent);
        }

        public void ChangeVisibility(Guid id, SocialNetworkPlatform.Enums.Visibility visibility)
        {
            var p = _repo.Get(id);
            if (p == null) return;
            p.ChangeVisibility(visibility);
        }

        public Post Share(Guid id, Guid byUserId)
        {
            var original = _repo.Get(id) ?? throw new InvalidOperationException("Original post not found");
            var shared = new Post
            {
                AuthorId = byUserId,
                Content = original.Content,
                SharedFrom = original.Id,
                Visibility = original.Visibility
            };
            _repo.Add(shared);
            return shared;
        }

        public bool CanView(Guid postId, Guid viewerUserId)
        {
            var p = _repo.Get(postId);
            if (p == null) return false;
            if (p.Visibility == SocialNetworkPlatform.Enums.Visibility.Public) return true;
            if (p.Visibility == SocialNetworkPlatform.Enums.Visibility.Private) return p.AuthorId == viewerUserId;
            // Friends visibility
            var author = _users?.Get(p.AuthorId);
            if (author == null) return false;
            return author.FriendIds.Contains(viewerUserId) || p.AuthorId == viewerUserId;
        }
    }
}