using System;
using System.Collections.Generic;
using SocialNetworkPlatform.Models;

namespace SocialNetworkPlatform.Services
{
    /// <summary>
    /// Provides search capabilities across platform entities.
    /// </summary>
    public interface ISearchService
    {
        IEnumerable<User> SearchUsers(string query);
        IEnumerable<Post> SearchPosts(string query);
        IEnumerable<Page> SearchPages(string query);
        IEnumerable<Reel> SearchReels(string query);
        IEnumerable<Story> SearchStories(string query);

        /// <summary>
        /// Generic search that returns tuples describing matched items.
        /// </summary>
        IEnumerable<(string Type, Guid Id, string Snippet)> SearchAll(string query);
    }
}
