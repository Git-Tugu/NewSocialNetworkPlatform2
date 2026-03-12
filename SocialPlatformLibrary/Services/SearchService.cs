using System;
using System.Collections.Generic;
using System.Linq;
using SocialNetworkPlatform.Models;
using SocialNetworkPlatform.Repositories;

namespace SocialNetworkPlatform.Services
{
    /// <summary>
    /// Simple in-memory search provider that performs case-insensitive substring matches
    /// across names, usernames and textual content. This is intentionally basic to
    /// keep the demo self-contained.
    /// </summary>
    public class SearchService : ISearchService
    {
        private readonly UserRepo _users;
        private readonly PostRepo _posts;
        private readonly PageRepo _pages;
        private readonly ReelRepo _reels;
        private readonly StoryRepo _stories;

        public SearchService(UserRepo users, PostRepo posts, PageRepo pages, ReelRepo reels, StoryRepo stories)
        {
            _users = users ?? throw new ArgumentNullException(nameof(users));
            _posts = posts ?? throw new ArgumentNullException(nameof(posts));
            _pages = pages ?? throw new ArgumentNullException(nameof(pages));
            _reels = reels ?? throw new ArgumentNullException(nameof(reels));
            _stories = stories ?? throw new ArgumentNullException(nameof(stories));
        }

        public IEnumerable<User> SearchUsers(string query)
        {
            if (string.IsNullOrWhiteSpace(query)) return Enumerable.Empty<User>();
            var q = query.Trim();
            return _users.GetAll().Where(u => u.Username.Contains(q, StringComparison.OrdinalIgnoreCase) || u.DisplayName.Contains(q, StringComparison.OrdinalIgnoreCase));
        }

        public IEnumerable<Post> SearchPosts(string query)
        {
            if (string.IsNullOrWhiteSpace(query)) return Enumerable.Empty<Post>();
            var q = query.Trim();
            return _posts.GetAll().Where(p => (p.Content ?? string.Empty).Contains(q, StringComparison.OrdinalIgnoreCase));
        }

        public IEnumerable<Page> SearchPages(string query)
        {
            if (string.IsNullOrWhiteSpace(query)) return Enumerable.Empty<Page>();
            var q = query.Trim();
            return _pages.GetAll().Where(p => (p.Name ?? string.Empty).Contains(q, StringComparison.OrdinalIgnoreCase) || (p.Description ?? string.Empty).Contains(q, StringComparison.OrdinalIgnoreCase));
        }

        public IEnumerable<Reel> SearchReels(string query)
        {
            if (string.IsNullOrWhiteSpace(query)) return Enumerable.Empty<Reel>();
            var q = query.Trim();
            return _reels.GetAll().Where(r => (r.MediaUrl ?? string.Empty).Contains(q, StringComparison.OrdinalIgnoreCase));
        }

        public IEnumerable<Story> SearchStories(string query)
        {
            if (string.IsNullOrWhiteSpace(query)) return Enumerable.Empty<Story>();
            var q = query.Trim();
            return _stories.GetAll().Where(s => (s.MediaUrl ?? string.Empty).Contains(q, StringComparison.OrdinalIgnoreCase));
        }

        public IEnumerable<(string Type, Guid Id, string Snippet)> SearchAll(string query)
        {
            if (string.IsNullOrWhiteSpace(query)) return Enumerable.Empty<(string, Guid, string)>();
            var q = query.Trim();
            var results = new List<(string, Guid, string)>();

            results.AddRange(SearchUsers(q).Select(u => ("User", u.Id, u.DisplayName ?? u.Username)));
            results.AddRange(SearchPages(q).Select(p => ("Page", p.Id, p.Name)));
            results.AddRange(SearchPosts(q).Select(p => ("Post", p.Id, p.Content ?? string.Empty)));
            results.AddRange(SearchReels(q).Select(r => ("Reel", r.Id, r.MediaUrl ?? string.Empty)));
            results.AddRange(SearchStories(q).Select(s => ("Story", s.Id, s.MediaUrl ?? string.Empty)));

            return results;
        }
    }
}
