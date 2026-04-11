using SocialNetworkPlatform.Data;
using SocialNetworkPlatform.Models;

namespace SocialNetworkPlatform.Repositories
{
    /// <summary>
    /// Repository for stories with SQLite persistence.
    /// </summary>
    public class StoryRepo : SqliteStoryRepository
    {
        public StoryRepo(SqliteContext context) : base(context) { }
    }
}