using SocialNetworkPlatform.Data;
using SocialNetworkPlatform.Models;

namespace SocialNetworkPlatform.Repositories
{
    /// <summary>
    /// Repository for pages with SQLite persistence.
    /// </summary>
    public class PageRepo : SqlitePageRepository
    {
        public PageRepo(SqliteContext context) : base(context) { }
    }
}