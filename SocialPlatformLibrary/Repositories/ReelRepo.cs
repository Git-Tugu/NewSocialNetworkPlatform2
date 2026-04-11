using SocialNetworkPlatform.Data;
using SocialNetworkPlatform.Models;

namespace SocialNetworkPlatform.Repositories
{
    /// <summary>
    /// Repository for reels with SQLite persistence.
    /// </summary>
    public class ReelRepo : SqliteReelRepository
    {
        public ReelRepo(SqliteContext context) : base(context) { }
    }
}