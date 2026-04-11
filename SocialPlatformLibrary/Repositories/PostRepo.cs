using SocialNetworkPlatform.Data;
using SocialNetworkPlatform.Models;

namespace SocialNetworkPlatform.Repositories
{
    /// <summary>
    /// Repository specialized for posts with SQLite persistence.
    /// </summary>
    public class PostRepo : SqlitePostRepository
    {
        public PostRepo(SqliteContext context) : base(context) { }
    }
}