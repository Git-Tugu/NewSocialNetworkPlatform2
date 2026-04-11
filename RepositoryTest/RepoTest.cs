using Microsoft.Data.Sqlite;
using SocialNetworkPlatform.Data;
using SocialNetworkPlatform.Repositories;
using SocialNetworkPlatform.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SocialNetworkPlatform.Enums;
using SocialNetworkPlatform.Platform;
using SocialNetworkPlatform.DTOs;
using SocialNetworkPlatform.Services;
using System.Xml.Linq;
using static Microsoft.ApplicationInsights.MetricDimensionNames.TelemetryContext;

namespace SocialNetworkPlatform.Tests
{
    // ---------------------------------------------------------------------------
    // Helpers / base
    // ---------------------------------------------------------------------------

    /// <summary>
    /// Creates a shared, fully-initialised in-memory SQLite context.
    /// Using "Data Source=:memory:;Mode=Memory;Cache=Shared" keeps the schema
    /// alive for the lifetime of the keepAlive connection.
    /// </summary>
    internal static class TestDb
    {
        private static readonly string _dsn =
            $"Data Source=testdb_{Guid.NewGuid():N};Mode=Memory;Cache=Shared";

        private static SqliteConnection? _keepAlive;

        public static SqliteContext Create()
        {
            // Each call returns a fresh context wired to the same in-memory DB.
            // The keep-alive connection stops SQLite from destroying the DB
            // between context instances created in the same test run.
            if (_keepAlive == null)
            {
                _keepAlive = new SqliteConnection(_dsn);
                _keepAlive.Open();
            }

            var ctx = new TestSqliteContext(_dsn);
            ctx.InitializeDatabase();
            return ctx;
        }

        /// <summary>
        /// Subclass that accepts a full connection string (not just a path).
        /// </summary>
        private sealed class TestSqliteContext : SqliteContext
        {
            public TestSqliteContext(string dsn) : base(dsn) { }
        }
    }

    // ---------------------------------------------------------------------------
    // Model factories – minimal valid objects for every entity type
    // ---------------------------------------------------------------------------

    internal static class Make
    {
        public static Models.User User(string username = "alice", string display = "Alice", byte age = 25)
            => new Models.User(username, display, age);

        public static Post Post(Guid authorId, Visibility vis = Visibility.Public)
            => new Post { Id = Guid.NewGuid(), AuthorId = authorId, Content = "hello", Visibility = vis };

        public static Comment Comment(Guid authorId, Guid targetId)
            => new Comment { Id = Guid.NewGuid(), AuthorId = authorId, Text = "nice", TargetId = targetId };

        public static Reaction Reaction(Guid authorId, Guid targetId, ReactionType type = ReactionType.Like)
            => new Reaction { Id = Guid.NewGuid(), AuthorId = authorId, TargetId = targetId, Type = type };

        public static Reel Reel(Guid authorId)
            => new Reel { Id = Guid.NewGuid(), AuthorId = authorId, MediaUrl = "https://cdn/reel.mp4", Duration = TimeSpan.FromSeconds(30) };

        public static Story Story(Guid authorId)
            => new Story { Id = Guid.NewGuid(), AuthorId = authorId, MediaUrl = "https://cdn/story.jpg", ExpiresAt = DateTime.UtcNow.AddHours(24) };

        public static Page Page(Guid ownerId)
            => new Page { Id = Guid.NewGuid(), OwnerId = ownerId, Name = "My Page", Description = "desc" };

        public static PageEvent PageEvent(Guid pageId)
            => new PageEvent
            {
                Id = Guid.NewGuid(),
                PageId = pageId,
                Title = "Concert",
                Description = "Live show",
                StartsAt = DateTime.UtcNow.AddDays(1),
                EndsAt = DateTime.UtcNow.AddDays(1).AddHours(3),
                Location = "Stadium"
            };
    }

    // ---------------------------------------------------------------------------
    // SqliteUserRepository tests
    // ---------------------------------------------------------------------------

    [TestClass]
    public class SqliteUserRepositoryTests
    {
        private SqliteContext _ctx = null!;
        private SqliteUserRepository _repo = null!;

        [TestInitialize]
        public void Init()
        {
            _ctx = TestDb.Create();
            _repo = new SqliteUserRepository(_ctx);
        }

        [TestCleanup]
        public void Cleanup() => _ctx.Dispose();

        [TestMethod]
        public void Constructor_NullContext_Throws()
            => Assert.ThrowsExactly<ArgumentNullException>(() => new SqliteUserRepository(null!));

        [TestMethod]
        public void Add_NullUser_Throws()
            => Assert.ThrowsExactly<ArgumentNullException>(() => _repo.Add(null!));
        [TestMethod]
        public void Add_And_Get_ReturnsUser()
        {
            var user = Make.User();
            _repo.Add(user);

            var result = _repo.Get(user.Id);

            Assert.IsNotNull(result);
            Assert.AreEqual(user.Id, result.Id);
            Assert.AreEqual(user.Username, result.Username);
            Assert.AreEqual(user.DisplayName, result.DisplayName);
            Assert.AreEqual(user.Age, result.Age);
        }

        [TestMethod]
        public void Get_NonExistentId_ReturnsNull()
            => Assert.IsNull(_repo.Get(Guid.NewGuid()));

        [TestMethod]
        public void GetAll_ReturnsAllUsers()
        {
            var u1 = Make.User("bob", "Bob", 30);
            var u2 = Make.User("carol", "Carol", 28);
            _repo.Add(u1);
            _repo.Add(u2);

            var all = _repo.GetAll().ToList();

            Assert.IsTrue(all.Any(u => u.Id == u1.Id));
            Assert.IsTrue(all.Any(u => u.Id == u2.Id));
        }

        [TestMethod]
        public void Remove_DeletesUser()
        {
            var user = Make.User("dave", "Dave", 22);
            _repo.Add(user);
            _repo.Remove(user.Id);

            Assert.IsNull(_repo.Get(user.Id));
        }

        [TestMethod]
        public void Add_UserWithFriends_LoadsFriends()
        {
            var friend = Make.User("eve", "Eve", 24);
            _repo.Add(friend);

            var user = Make.User("frank", "Frank", 26);
            user.AddFriend(friend.Id);
            _repo.Add(user);

            var loaded = _repo.Get(user.Id);
            Assert.IsNotNull(loaded);
            Assert.IsTrue(loaded.FriendIds.Contains(friend.Id));
        }

        [TestMethod]
        public void Add_DuplicateFriendId_DoesNotThrow()
        {
            var friend = Make.User("grace", "Grace", 21);
            _repo.Add(friend);

            var user = Make.User("henry", "Henry", 27);
            user.AddFriend(friend.Id);
            user.AddFriend(friend.Id); // duplicate – INSERT OR IGNORE should handle
            _repo.Add(user);           // should not throw

            var loaded = _repo.Get(user.Id);
            Assert.IsNotNull(loaded);
        }
    }

    // ---------------------------------------------------------------------------
    // SqlitePostRepository tests
    // ---------------------------------------------------------------------------

    [TestClass]
    public class SqlitePostRepositoryTests
    {
        private SqliteContext _ctx = null!;
        private SqliteUserRepository _userRepo = null!;
        private SqlitePostRepository _repo = null!;
        private Guid _authorId;

        [TestInitialize]
        public void Init()
        {
            _ctx = TestDb.Create();
            _userRepo = new SqliteUserRepository(_ctx);
            _repo = new SqlitePostRepository(_ctx);

            var author = Make.User($"author_{Guid.NewGuid():N}"[..20], "Author", 30);
            _userRepo.Add(author);
            _authorId = author.Id;
        }

        [TestCleanup]
        public void Cleanup() => _ctx.Dispose();

        [TestMethod]
        public void Constructor_NullContext_Throws()
            => Assert.ThrowsExactly<ArgumentNullException>(() => new SqlitePostRepository(null!));

        [TestMethod]
        public void Add_NullPost_Throws()
            => Assert.ThrowsExactly<ArgumentNullException>(() => _repo.Add(null!));

        [TestMethod]
        public void Add_And_Get_ReturnsPost()
        {
            var post = Make.Post(_authorId);
            _repo.Add(post);

            var result = _repo.Get(post.Id);

            Assert.IsNotNull(result);
            Assert.AreEqual(post.Id, result.Id);
            Assert.AreEqual(post.Content, result.Content);
            Assert.AreEqual(post.Visibility, result.Visibility);
            Assert.IsNull(result.SharedFrom);
        }

        [TestMethod]
        public void Get_NonExistentId_ReturnsNull()
            => Assert.IsNull(_repo.Get(Guid.NewGuid()));

        [TestMethod]
        public void GetAll_ReturnsPosts()
        {
            var p1 = Make.Post(_authorId);
            var p2 = Make.Post(_authorId);
            _repo.Add(p1);
            _repo.Add(p2);

            var all = _repo.GetAll().ToList();
            Assert.IsTrue(all.Any(p => p.Id == p1.Id));
            Assert.IsTrue(all.Any(p => p.Id == p2.Id));
        }

        [TestMethod]
        public void Remove_DeletesPost()
        {
            var post = Make.Post(_authorId);
            _repo.Add(post);
            _repo.Remove(post.Id);

            Assert.IsNull(_repo.Get(post.Id));
        }

        [TestMethod]
        public void Add_PostWithSharedFrom_PersistedAndLoaded()
        {
            var original = Make.Post(_authorId);
            _repo.Add(original);

            var shared = Make.Post(_authorId);
            shared.SharedFrom = original.Id;
            _repo.Add(shared);

            var loaded = _repo.Get(shared.Id);
            Assert.IsNotNull(loaded);
            Assert.AreEqual(original.Id, loaded.SharedFrom);
        }

        [TestMethod]
        public void Add_PostWithCommentIds_LoadsCommentIds()
        {
            // We only persist the IDs; no FK enforcement in the junction table insert.
            var post = Make.Post(_authorId);
            var commentId = Guid.NewGuid();
            post.CommentIds.Add(commentId);
            _repo.Add(post);

            var loaded = _repo.Get(post.Id);
            Assert.IsNotNull(loaded);
            Assert.IsTrue(loaded.CommentIds.Contains(commentId));
        }

        [TestMethod]
        public void Add_PostWithReactionIds_LoadsReactionIds()
        {
            var post = Make.Post(_authorId);
            var reactionId = Guid.NewGuid();
            post.ReactionIds.Add(reactionId);
            _repo.Add(post);

            var loaded = _repo.Get(post.Id);
            Assert.IsNotNull(loaded);
            Assert.IsTrue(loaded.ReactionIds.Contains(reactionId));
        }
    }

    // ---------------------------------------------------------------------------
    // SqliteCommentRepository tests
    // ---------------------------------------------------------------------------

    [TestClass]
    public class SqliteCommentRepositoryTests
    {
        private SqliteContext _ctx = null!;
        private SqliteUserRepository _userRepo = null!;
        private SqliteCommentRepository _repo = null!;
        private Guid _authorId;
        private Guid _targetId;

        [TestInitialize]
        public void Init()
        {
            _ctx = TestDb.Create();
            _userRepo = new SqliteUserRepository(_ctx);
            _repo = new SqliteCommentRepository(_ctx);

            var author = Make.User($"cmtauth_{Guid.NewGuid():N}"[..20], "CmtAuthor", 25);
            _userRepo.Add(author);
            _authorId = author.Id;
            _targetId = Guid.NewGuid();
        }

        [TestCleanup]
        public void Cleanup() => _ctx.Dispose();

        [TestMethod]
        public void Constructor_NullContext_Throws()
            => Assert.ThrowsExactly<ArgumentNullException>(() => new SqliteCommentRepository(null!));

        [TestMethod]
        public void Add_NullComment_Throws()
            => Assert.ThrowsExactly<ArgumentNullException>(() => _repo.Add(null!));

        [TestMethod]
        public void Add_And_Get_ReturnsComment()
        {
            var comment = Make.Comment(_authorId, _targetId);
            _repo.Add(comment);

            var result = _repo.Get(comment.Id);

            Assert.IsNotNull(result);
            Assert.AreEqual(comment.Id, result.Id);
            Assert.AreEqual(comment.Text, result.Text);
            Assert.AreEqual(comment.TargetId, result.TargetId);
        }

        [TestMethod]
        public void Get_NonExistentId_ReturnsNull()
            => Assert.IsNull(_repo.Get(Guid.NewGuid()));

        [TestMethod]
        public void GetAll_ReturnsComments()
        {
            var c1 = Make.Comment(_authorId, _targetId);
            var c2 = Make.Comment(_authorId, _targetId);
            _repo.Add(c1);
            _repo.Add(c2);

            var all = _repo.GetAll().ToList();
            Assert.IsTrue(all.Any(c => c.Id == c1.Id));
            Assert.IsTrue(all.Any(c => c.Id == c2.Id));
        }

        [TestMethod]
        public void Remove_DeletesComment()
        {
            var comment = Make.Comment(_authorId, _targetId);
            _repo.Add(comment);
            _repo.Remove(comment.Id);

            Assert.IsNull(_repo.Get(comment.Id));
        }

        [TestMethod]
        public void Add_CommentWithReactionIds_LoadsReactionIds()
        {
            var comment = Make.Comment(_authorId, _targetId);
            var reactionId = Guid.NewGuid();
            comment.ReactionIds.Add(reactionId);
            _repo.Add(comment);

            var loaded = _repo.Get(comment.Id);
            Assert.IsNotNull(loaded);
            Assert.IsTrue(loaded.ReactionIds.Contains(reactionId));
        }
    }

    // ---------------------------------------------------------------------------
    // SqliteReactionRepository tests
    // ---------------------------------------------------------------------------

    [TestClass]
    public class SqliteReactionRepositoryTests
    {
        private SqliteContext _ctx = null!;
        private SqliteUserRepository _userRepo = null!;
        private SqliteReactionRepository _repo = null!;
        private Guid _authorId;

        [TestInitialize]
        public void Init()
        {
            _ctx = TestDb.Create();
            _userRepo = new SqliteUserRepository(_ctx);
            _repo = new SqliteReactionRepository(_ctx);

            var author = Make.User($"rxnauth_{Guid.NewGuid():N}"[..20], "RxnAuthor", 22);
            _userRepo.Add(author);
            _authorId = author.Id;
        }

        [TestCleanup]
        public void Cleanup() => _ctx.Dispose();

        [TestMethod]
        public void Constructor_NullContext_Throws()
            => Assert.ThrowsExactly<ArgumentNullException>(() => new SqliteReactionRepository(null!));

        [TestMethod]
        public void Add_NullReaction_Throws()
            => Assert.ThrowsExactly<ArgumentNullException>(() => _repo.Add(null!));

        [TestMethod]
        public void Add_And_Get_ReturnsReaction()
        {
            var reaction = Make.Reaction(_authorId, Guid.NewGuid());
            _repo.Add(reaction);

            var result = _repo.Get(reaction.Id);

            Assert.IsNotNull(result);
            Assert.AreEqual(reaction.Id, result.Id);
            Assert.AreEqual(reaction.Type, result.Type);
            Assert.AreEqual(reaction.TargetId, result.TargetId);
        }

        [TestMethod]
        public void Get_NonExistentId_ReturnsNull()
            => Assert.IsNull(_repo.Get(Guid.NewGuid()));

        [TestMethod]
        public void GetAll_ReturnsReactions()
        {
            var r1 = Make.Reaction(_authorId, Guid.NewGuid(), ReactionType.Like);
            var r2 = Make.Reaction(_authorId, Guid.NewGuid(), ReactionType.Love);
            _repo.Add(r1);
            _repo.Add(r2);

            var all = _repo.GetAll().ToList();
            Assert.IsTrue(all.Any(r => r.Id == r1.Id));
            Assert.IsTrue(all.Any(r => r.Id == r2.Id));
        }

        [TestMethod]
        public void Remove_DeletesReaction()
        {
            var reaction = Make.Reaction(_authorId, Guid.NewGuid());
            _repo.Add(reaction);
            _repo.Remove(reaction.Id);

            Assert.IsNull(_repo.Get(reaction.Id));
        }

        [TestMethod]
        public void Add_ReactionWithNestedReactionIds_LoadsNestedIds()
        {
            var reaction = Make.Reaction(_authorId, Guid.NewGuid());
            var nestedId = Guid.NewGuid();
            reaction.ReactionIds.Add(nestedId);
            _repo.Add(reaction);

            var loaded = _repo.Get(reaction.Id);
            Assert.IsNotNull(loaded);
            Assert.IsTrue(loaded.ReactionIds.Contains(nestedId));
        }
    }

    // ---------------------------------------------------------------------------
    // SqliteReelRepository tests
    // ---------------------------------------------------------------------------

    [TestClass]
    public class SqliteReelRepositoryTests
    {
        private SqliteContext _ctx = null!;
        private SqliteUserRepository _userRepo = null!;
        private SqliteReelRepository _repo = null!;
        private Guid _authorId;

        [TestInitialize]
        public void Init()
        {
            _ctx = TestDb.Create();
            _userRepo = new SqliteUserRepository(_ctx);
            _repo = new SqliteReelRepository(_ctx);

            var author = Make.User($"reelauth_{Guid.NewGuid():N}"[..20], "ReelAuthor", 29);
            _userRepo.Add(author);
            _authorId = author.Id;
        }

        [TestCleanup]
        public void Cleanup() => _ctx.Dispose();

        [TestMethod]
        public void Constructor_NullContext_Throws()
            => Assert.ThrowsExactly<ArgumentNullException>(() => new SqliteReelRepository(null!));

        [TestMethod]
        public void Add_NullReel_Throws()
            => Assert.ThrowsExactly<ArgumentNullException>(() => _repo.Add(null!));

        [TestMethod]
        public void Add_And_Get_ReturnsReel()
        {
            var reel = Make.Reel(_authorId);
            _repo.Add(reel);

            var result = _repo.Get(reel.Id);

            Assert.IsNotNull(result);
            Assert.AreEqual(reel.Id, result.Id);
            Assert.AreEqual(reel.MediaUrl, result.MediaUrl);
            Assert.AreEqual(reel.Duration, result.Duration);
        }

        [TestMethod]
        public void Get_NonExistentId_ReturnsNull()
            => Assert.IsNull(_repo.Get(Guid.NewGuid()));

        [TestMethod]
        public void GetAll_ReturnsReels()
        {
            var r1 = Make.Reel(_authorId);
            var r2 = Make.Reel(_authorId);
            _repo.Add(r1);
            _repo.Add(r2);

            var all = _repo.GetAll().ToList();
            Assert.IsTrue(all.Any(r => r.Id == r1.Id));
            Assert.IsTrue(all.Any(r => r.Id == r2.Id));
        }

        [TestMethod]
        public void Remove_DeletesReel()
        {
            var reel = Make.Reel(_authorId);
            _repo.Add(reel);
            _repo.Remove(reel.Id);

            Assert.IsNull(_repo.Get(reel.Id));
        }

        [TestMethod]
        public void Add_ReelWithViews_LoadsViewedBy()
        {
            var viewer = Make.User($"viewer_{Guid.NewGuid():N}"[..20], "Viewer", 20);
            _userRepo.Add(viewer);

            var reel = Make.Reel(_authorId);
            reel.ViewedBy.Add(viewer.Id);
            _repo.Add(reel);

            var loaded = _repo.Get(reel.Id);
            Assert.IsNotNull(loaded);
            Assert.IsTrue(loaded.ViewedBy.Contains(viewer.Id));
        }

        [TestMethod]
        public void Add_ReelWithCommentIds_LoadsCommentIds()
        {
            var reel = Make.Reel(_authorId);
            var commentId = Guid.NewGuid();
            reel.CommentIds.Add(commentId);
            _repo.Add(reel);

            var loaded = _repo.Get(reel.Id);
            Assert.IsNotNull(loaded);
            Assert.IsTrue(loaded.CommentIds.Contains(commentId));
        }

        [TestMethod]
        public void Add_ReelWithReactionIds_LoadsReactionIds()
        {
            var reel = Make.Reel(_authorId);
            var reactionId = Guid.NewGuid();
            reel.ReactionIds.Add(reactionId);
            _repo.Add(reel);

            var loaded = _repo.Get(reel.Id);
            Assert.IsNotNull(loaded);
            Assert.IsTrue(loaded.ReactionIds.Contains(reactionId));
        }
    }

    // ---------------------------------------------------------------------------
    // SqliteStoryRepository tests
    // ---------------------------------------------------------------------------

    [TestClass]
    public class SqliteStoryRepositoryTests
    {
        private SqliteContext _ctx = null!;
        private SqliteUserRepository _userRepo = null!;
        private SqliteStoryRepository _repo = null!;
        private Guid _authorId;

        [TestInitialize]
        public void Init()
        {
            _ctx = TestDb.Create();
            _userRepo = new SqliteUserRepository(_ctx);
            _repo = new SqliteStoryRepository(_ctx);

            var author = Make.User($"stryauth_{Guid.NewGuid():N}"[..20], "StoryAuthor", 27);
            _userRepo.Add(author);
            _authorId = author.Id;
        }

        [TestCleanup]
        public void Cleanup() => _ctx.Dispose();

        [TestMethod]
        public void Constructor_NullContext_Throws()
            => Assert.ThrowsExactly<ArgumentNullException>(() => new SqliteStoryRepository(null!));

        [TestMethod]
        public void Add_NullStory_Throws()
            => Assert.ThrowsExactly<ArgumentNullException>(() => _repo.Add(null!));

        [TestMethod]
        public void Add_And_Get_ReturnsStory()
        {
            var story = Make.Story(_authorId);
            _repo.Add(story);

            var result = _repo.Get(story.Id);

            Assert.IsNotNull(result);
            Assert.AreEqual(story.Id, result.Id);
            Assert.AreEqual(story.MediaUrl, result.MediaUrl);
        }

        [TestMethod]
        public void Get_NonExistentId_ReturnsNull()
            => Assert.IsNull(_repo.Get(Guid.NewGuid()));

        [TestMethod]
        public void GetAll_ReturnsStories()
        {
            var s1 = Make.Story(_authorId);
            var s2 = Make.Story(_authorId);
            _repo.Add(s1);
            _repo.Add(s2);

            var all = _repo.GetAll().ToList();
            Assert.IsTrue(all.Any(s => s.Id == s1.Id));
            Assert.IsTrue(all.Any(s => s.Id == s2.Id));
        }

        [TestMethod]
        public void Remove_DeletesStory()
        {
            var story = Make.Story(_authorId);
            _repo.Add(story);
            _repo.Remove(story.Id);

            Assert.IsNull(_repo.Get(story.Id));
        }

        [TestMethod]
        public void Add_StoryWithViews_LoadsViewedBy()
        {
            var viewer = Make.User($"strvwr_{Guid.NewGuid():N}"[..20], "StoryViewer", 19);
            _userRepo.Add(viewer);

            var story = Make.Story(_authorId);
            story.ViewedBy.Add(viewer.Id);
            _repo.Add(story);

            var loaded = _repo.Get(story.Id);
            Assert.IsNotNull(loaded);
            Assert.IsTrue(loaded.ViewedBy.Contains(viewer.Id));
        }

        [TestMethod]
        public void Add_StoryWithCommentIds_LoadsCommentIds()
        {
            var story = Make.Story(_authorId);
            var commentId = Guid.NewGuid();
            story.CommentIds.Add(commentId);
            _repo.Add(story);

            var loaded = _repo.Get(story.Id);
            Assert.IsNotNull(loaded);
            Assert.IsTrue(loaded.CommentIds.Contains(commentId));
        }

        [TestMethod]
        public void Add_StoryWithReactionIds_LoadsReactionIds()
        {
            var story = Make.Story(_authorId);
            var reactionId = Guid.NewGuid();
            story.ReactionIds.Add(reactionId);
            _repo.Add(story);

            var loaded = _repo.Get(story.Id);
            Assert.IsNotNull(loaded);
            Assert.IsTrue(loaded.ReactionIds.Contains(reactionId));
        }
    }

    // ---------------------------------------------------------------------------
    // SqlitePageRepository tests
    // ---------------------------------------------------------------------------

    [TestClass]
    public class SqlitePageRepositoryTests
    {
        private SqliteContext _ctx = null!;
        private SqliteUserRepository _userRepo = null!;
        private SqlitePageRepository _repo = null!;
        private Guid _ownerId;

        [TestInitialize]
        public void Init()
        {
            _ctx = TestDb.Create();
            _userRepo = new SqliteUserRepository(_ctx);
            _repo = new SqlitePageRepository(_ctx);

            var owner = Make.User($"pgowner_{Guid.NewGuid():N}"[..20], "PageOwner", 35);
            _userRepo.Add(owner);
            _ownerId = owner.Id;
        }

        [TestCleanup]
        public void Cleanup() => _ctx.Dispose();

        [TestMethod]
        public void Constructor_NullContext_Throws()
            => Assert.ThrowsExactly<ArgumentNullException>(() => new SqlitePageRepository(null!));

        [TestMethod]
        public void Add_NullPage_Throws()
            => Assert.ThrowsExactly<ArgumentNullException>(() => _repo.Add(null!));

        [TestMethod]
        public void Add_And_Get_ReturnsPage()
        {
            var page = Make.Page(_ownerId);
            _repo.Add(page);

            var result = _repo.Get(page.Id);

            Assert.IsNotNull(result);
            Assert.AreEqual(page.Id, result.Id);
            Assert.AreEqual(page.Name, result.Name);
            Assert.AreEqual(page.Description, result.Description);
        }

        [TestMethod]
        public void Get_NonExistentId_ReturnsNull()
            => Assert.IsNull(_repo.Get(Guid.NewGuid()));

        [TestMethod]
        public void GetAll_ReturnsPages()
        {
            var p1 = Make.Page(_ownerId);
            var p2 = Make.Page(_ownerId);
            _repo.Add(p1);
            _repo.Add(p2);

            var all = _repo.GetAll().ToList();
            Assert.IsTrue(all.Any(p => p.Id == p1.Id));
            Assert.IsTrue(all.Any(p => p.Id == p2.Id));
        }

        [TestMethod]
        public void Remove_DeletesPage()
        {
            var page = Make.Page(_ownerId);
            _repo.Add(page);
            _repo.Remove(page.Id);

            Assert.IsNull(_repo.Get(page.Id));
        }

        [TestMethod]
        public void Add_PageWithFollowers_LoadsFollowerIds()
        {
            var follower = Make.User($"pgflwr_{Guid.NewGuid():N}"[..20], "Follower", 20);
            _userRepo.Add(follower);

            var page = Make.Page(_ownerId);
            page.FollowerIds.Add(follower.Id);
            _repo.Add(page);

            var loaded = _repo.Get(page.Id);
            Assert.IsNotNull(loaded);
            Assert.IsTrue(loaded.FollowerIds.Contains(follower.Id));
        }

        [TestMethod]
        public void Add_PageWithPostIds_LoadsPostIds()
        {
            var page = Make.Page(_ownerId);
            var postId = Guid.NewGuid();
            page.PostIds.Add(postId);
            _repo.Add(page);

            var loaded = _repo.Get(page.Id);
            Assert.IsNotNull(loaded);
            Assert.IsTrue(loaded.PostIds.Contains(postId));
        }
    }

    // ---------------------------------------------------------------------------
    // SqlitePageEventRepository tests
    // ---------------------------------------------------------------------------

    [TestClass]
    public class SqlitePageEventRepositoryTests
    {
        private SqliteContext _ctx = null!;
        private SqliteUserRepository _userRepo = null!;
        private SqlitePageRepository _pageRepo = null!;
        private SqlitePageEventRepository _repo = null!;
        private Guid _pageId;

        [TestInitialize]
        public void Init()
        {
            _ctx = TestDb.Create();
            _userRepo = new SqliteUserRepository(_ctx);
            _pageRepo = new SqlitePageRepository(_ctx);
            _repo = new SqlitePageEventRepository(_ctx);

            var owner = Make.User($"evtowner_{Guid.NewGuid():N}"[..20], "EvtOwner", 40);
            _userRepo.Add(owner);

            var page = Make.Page(owner.Id);
            _pageRepo.Add(page);
            _pageId = page.Id;
        }

        [TestCleanup]
        public void Cleanup() => _ctx.Dispose();

        [TestMethod]
        public void Constructor_NullContext_Throws()
            => Assert.ThrowsExactly<ArgumentNullException>(() => new SqlitePageEventRepository(null!));

        [TestMethod]
        public void Add_NullPageEvent_Throws()
            => Assert.ThrowsExactly<ArgumentNullException>(() => _repo.Add(null!));

        [TestMethod]
        public void Add_And_Get_ReturnsPageEvent()
        {
            var ev = Make.PageEvent(_pageId);
            _repo.Add(ev);

            var result = _repo.Get(ev.Id);

            Assert.IsNotNull(result);
            Assert.AreEqual(ev.Id, result.Id);
            Assert.AreEqual(ev.Title, result.Title);
            Assert.AreEqual(ev.Description, result.Description);
            Assert.AreEqual(ev.Location, result.Location);
            Assert.AreEqual(ev.PageId, result.PageId);
        }

        [TestMethod]
        public void Get_NonExistentId_ReturnsNull()
            => Assert.IsNull(_repo.Get(Guid.NewGuid()));

        [TestMethod]
        public void GetAll_ReturnsPageEvents()
        {
            var e1 = Make.PageEvent(_pageId);
            var e2 = Make.PageEvent(_pageId);
            _repo.Add(e1);
            _repo.Add(e2);

            var all = _repo.GetAll().ToList();
            Assert.IsTrue(all.Any(e => e.Id == e1.Id));
            Assert.IsTrue(all.Any(e => e.Id == e2.Id));
        }

        [TestMethod]
        public void Remove_DeletesPageEvent()
        {
            var ev = Make.PageEvent(_pageId);
            _repo.Add(ev);
            _repo.Remove(ev.Id);

            Assert.IsNull(_repo.Get(ev.Id));
        }

        [TestMethod]
        public void Add_PageEventWithCommentIds_LoadsCommentIds()
        {
            var ev = Make.PageEvent(_pageId);
            var commentId = Guid.NewGuid();
            ev.CommentIds.Add(commentId);
            _repo.Add(ev);

            var loaded = _repo.Get(ev.Id);
            Assert.IsNotNull(loaded);
            Assert.IsTrue(loaded.CommentIds.Contains(commentId));
        }

        [TestMethod]
        public void Add_PageEventWithReactionIds_LoadsReactionIds()
        {
            var ev = Make.PageEvent(_pageId);
            var reactionId = Guid.NewGuid();
            ev.ReactionIds.Add(reactionId);
            _repo.Add(ev);

            var loaded = _repo.Get(ev.Id);
            Assert.IsNotNull(loaded);
            Assert.IsTrue(loaded.ReactionIds.Contains(reactionId));
        }
    }

    // ---------------------------------------------------------------------------
    // SqliteContext tests
    // ---------------------------------------------------------------------------

    [TestClass]
    public class SqliteContextTests
    {
        [TestMethod]
        public void GetConnection_ReturnsSameConnection_OnMultipleCalls()
        {
            using var ctx = TestDb.Create();
            var c1 = ctx.GetConnection();
            var c2 = ctx.GetConnection();
            Assert.AreSame(c1, c2);
        }

        [TestMethod]
        public void InitializeDatabase_IdempotentOnSecondCall()
        {
            // Second InitializeDatabase call must not throw (CREATE TABLE IF NOT EXISTS).
            using var ctx = TestDb.Create();
            ctx.InitializeDatabase(); // second call
        }

        [TestMethod]
        public void Dispose_DoesNotThrow()
        {
            var ctx = TestDb.Create();
            ctx.Dispose(); // should not throw
        }
    }
}