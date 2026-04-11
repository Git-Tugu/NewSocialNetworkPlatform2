using SocialNetworkPlatform.Data;
using SocialNetworkPlatform.Models;

namespace SocialNetworkPlatform.Repositories
{
    /// <summary>
    /// Repository for page events with SQLite persistence.
    /// </summary>
    public class PageEventRepo : SqlitePageEventRepository
    {
        public PageEventRepo(SqliteContext context) : base(context) { }
    }
}