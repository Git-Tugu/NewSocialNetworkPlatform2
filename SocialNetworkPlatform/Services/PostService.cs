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
    public class PostService : CrudService<Post, PostDto>, IPostService
    {
        private readonly PostRepo _typedRepo;
        private readonly UserRepo _users;
        private readonly ICommentService _comments;
        private readonly IReactionService _reactions;

        public PostService(PostRepo repo, UserRepo users, ICommentService comments, IReactionService reactions)
            : base(repo, dto => new Post { AuthorId = dto.AuthorId, Content = dto.Content ?? string.Empty })
        {
            _typedRepo = repo ?? throw new ArgumentNullException(nameof(repo));
            _users = users ?? throw new ArgumentNullException(nameof(users));
            _comments = comments ?? throw new ArgumentNullException(nameof(comments));
            _reactions = reactions ?? throw new ArgumentNullException(nameof(reactions));
        }

        public void Edit(Guid id, string newContent)
        {
            var p = _typedRepo.Get(id);
            if (p == null) return;
            p.Edit(newContent);
        }

        public void ChangeVisibility(Guid id, SocialNetworkPlatform.Enums.Visibility visibility)
        {
            var p = _typedRepo.Get(id);
            if (p == null) return;
            p.ChangeVisibility(visibility);
        }

        public Post Share(Guid id, Guid byUserId)
        {
            var original = _typedRepo.Get(id) ?? throw new InvalidOperationException("Original post not found");
            var shared = new Post
            {
                AuthorId = byUserId,
                Content = original.Content,
                SharedFrom = original.Id,
                Visibility = original.Visibility
            };
            _typedRepo.Add(shared);
            return shared;
        }

        public bool CanView(Guid postId, Guid viewerUserId)
        {
            var p = _typedRepo.Get(postId);
            if (p == null) return false;
            if (p.Visibility == SocialNetworkPlatform.Enums.Visibility.Public) return true;
            if (p.Visibility == SocialNetworkPlatform.Enums.Visibility.Private) return p.AuthorId == viewerUserId;
            // Friends visibility
            var author = _users?.Get(p.AuthorId);
            if (author == null) return false;
            return author.FriendIds.Contains(viewerUserId) || p.AuthorId == viewerUserId;
        }

        /// <summary>
        /// Delete a post and cascade delete all its comments and reactions.
        /// </summary>
        public override void Delete(Guid id)
        {
            // Delete all comments on this post
            _comments.DeleteByTarget(id);
            
            // Delete all reactions on this post
            _reactions.DeleteByTarget(id);
            
            // Delete the post itself
            base.Delete(id);
        }
    }
}