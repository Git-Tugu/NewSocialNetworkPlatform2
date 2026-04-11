using SocialNetworkPlatform.Data;
using SocialNetworkPlatform.Models;

namespace SocialNetworkPlatform.Repositories
{
    /// <summary>
    /// Repository for reactions with SQLite persistence.
    /// </summary>
    public class ReactionRepo : SqliteReactionRepository
    {
        public ReactionRepo(SqliteContext context) : base(context) { }
    }
}
