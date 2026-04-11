using SocialNetworkPlatform.Data;
using SocialNetworkPlatform.Models;

namespace SocialNetworkPlatform.Repositories
{
    /// <summary>
    /// Repository for comments with SQLite persistence.
    /// </summary>
    public class CommentRepo : SqliteCommentRepository
    {
        public CommentRepo(SqliteContext context) : base(context) { }
    }
}
