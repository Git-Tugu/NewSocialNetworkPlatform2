using SocialNetworkPlatform.DTOs;
using SocialNetworkPlatform.Models;
using System.Collections.Generic;
using System;

namespace SocialNetworkPlatform.Services
{
    /// <summary>
    /// Operations for managing posts.
    /// </summary>
    public interface IPostService
    {
        Post Create(PostDto dto);
        Post? Get(Guid id);
        IEnumerable<Post> GetAll();
        void Delete(Guid id);
        void Edit(Guid id, string newContent);
        void ChangeVisibility(Guid id, SocialNetworkPlatform.Enums.Visibility visibility);
        Post Share(Guid id, Guid byUserId);
        bool CanView(Guid postId, Guid viewerUserId);
    }
}