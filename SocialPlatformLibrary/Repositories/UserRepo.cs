using SocialNetworkPlatform.Data;
using SocialNetworkPlatform.Models;

namespace SocialNetworkPlatform.Repositories
{
    /// <summary>
    /// Repository specialized for users with SQLite persistence.
    /// </summary>
    public class UserRepo : SqliteUserRepository
    {
        public UserRepo(SqliteContext context) : base(context) { }
    }
}