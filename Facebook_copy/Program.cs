using System;
using System.Linq;
using SocialNetworkPlatform.Platform;
using SocialNetworkPlatform.DTOs;
using SocialNetworkPlatform.Enums;
using SocialNetworkPlatform.Models;

var platform = new Platform();
bool running = true;

PrintHeader("SocialNetworkPlatform - Interactive Example");

while (running)
{
    PrintMenu();
    Console.Write("Select an option: ");
    string choice = Console.ReadLine() ?? "0";

    try
    {
        switch (choice)
        {
            case "1":
                RunFullExample();
                break;
            case "2":
                RunUserAndFriendExample();
                break;
            case "3":
                RunCommentAndReactionExample();
                break;
            case "4":
                DisplayPlatformStatistics();
                break;
            case "0":
                running = false;
                PrintFooter("Goodbye!");
                break;
            default:
                Console.WriteLine("Invalid option. Please try again.\n");
                break;
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"\nError: {ex.Message}\n");
    }
}

// ============ MENU & DISPLAY ============

void PrintMenu()
{
    Console.WriteLine("\n+--------------------------------------------+");
    Console.WriteLine("|            EXAMPLE MENU                    |");
    Console.WriteLine("+--------------------------------------------+");
    Console.WriteLine("|  1. Run Full Example                       |");
    Console.WriteLine("|  2. User & Friendship Example              |");
    Console.WriteLine("|  3. Comments & Reactions Example           |");
    Console.WriteLine("|  4. Display Statistics                     |");
    Console.WriteLine("|  0. Exit                                   |");
    Console.WriteLine("+--------------------------------------------+");
}

void PrintHeader(string title)
{
    Console.WriteLine("\n+----------------------------------------------------+");
    Console.WriteLine($"| {title.PadRight(52)} |");
    Console.WriteLine("+----------------------------------------------------+\n");
}

void PrintSection(string title)
{
    Console.WriteLine($"\n+ {title}");
    Console.WriteLine(new string('-', 60));
}

void PrintFooter(string message)
{
    Console.WriteLine($"\n+ {message}");
}

// ============ EXAMPLE RUNNERS ============

void RunFullExample()
{
    PrintHeader("FULL PLATFORM EXAMPLE");

    Console.WriteLine("Creating 3 users...");
    var alice = platform.UserService.Create("alice123", "Alice Johnson", 30);
    var bob = platform.UserService.Create("bob456", "Bob Smith", 28);
    var charlie = platform.UserService.Create("charlie789", "Charlie Brown", 25);

    Console.WriteLine($"  + {alice.DisplayName} (Age: {alice.Age})");
    Console.WriteLine($"  + {bob.DisplayName} (Age: {bob.Age})");
    Console.WriteLine($"  + {charlie.DisplayName} (Age: {charlie.Age})");

    PrintSection("FRIENDSHIP SYSTEM");
    alice.AddFriend(bob.Id);
    bob.AddFriend(alice.Id);
    bob.AddFriend(charlie.Id);
    charlie.AddFriend(bob.Id);

    Console.WriteLine($"  {alice.DisplayName}'s friends: {string.Join(", ", GetFriendNames(alice))}");
    Console.WriteLine($"  {bob.DisplayName}'s friends: {string.Join(", ", GetFriendNames(bob))}");
    Console.WriteLine($"  {charlie.DisplayName}'s friends: {string.Join(", ", GetFriendNames(charlie))}");

    PrintSection("CONTENT CREATION");
    var post = platform.PostService.Create(new PostDto(alice.Id, "Just finished my morning run! Feeling energized!"));
    var reel = platform.ReelService.Create(new MediaDto(alice.Id, "https://media.example/reel1.mp4", TimeSpan.FromSeconds(30)));
    var story = platform.StoryService.Create(new MediaDto(bob.Id, "https://media.example/story1.jpg", null, DateTime.UtcNow.AddHours(24)));

    platform.ReelService.AddView(reel.Id, bob.Id);
    platform.ReelService.AddView(reel.Id, charlie.Id);
    platform.StoryService.AddView(story.Id, alice.Id);
    platform.StoryService.AddView(story.Id, charlie.Id);

    Console.WriteLine($"  + Post by {alice.DisplayName}: \"{post.Content}\"");
    Console.WriteLine($"  + Reel created (30sec video, {reel.ViewedBy.Count} views)");
    Console.WriteLine($"  + Story created (24h expiry, {story.ViewedBy.Count} views)");

    PrintSection("COMMENTS & REACTIONS");
    var c1 = platform.CommentService.Create(new CommentDto(bob.Id, post.Id, "Nice work!"));
    var c2 = platform.CommentService.Create(new CommentDto(charlie.Id, post.Id, "Inspiring! I should do the same."));

    platform.ReactionService.Create(new ReactionDto(charlie.Id, post.Id, ReactionType.Like));
    platform.ReactionService.Create(new ReactionDto(bob.Id, post.Id, ReactionType.Love));

    Console.WriteLine($"\n  Post: \"{post.Content}\" by {alice.DisplayName}");
    Console.WriteLine($"\n  Comments:");
    Console.WriteLine($"    + {bob.DisplayName}: \"{c1.Text}\"");
    Console.WriteLine($"    + {charlie.DisplayName}: \"{c2.Text}\"");
    Console.WriteLine($"\n  Reactions:");
    Console.WriteLine($"    + {charlie.DisplayName} reacted {ReactionType.Like}");
    Console.WriteLine($"    + {bob.DisplayName} reacted {ReactionType.Love}");

    PrintFooter("Full example complete!");
}

void RunUserAndFriendExample()
{
    PrintHeader("USER & FRIENDSHIP EXAMPLE");

    Console.WriteLine("Creating users...");
    var user1 = platform.UserService.Create("user1", "Alice", 25);
    var user2 = platform.UserService.Create("user2", "Bob", 30);
    var user3 = platform.UserService.Create("user3", "Charlie", 28);

    Console.WriteLine($"  + {user1.DisplayName}");
    Console.WriteLine($"  + {user2.DisplayName}");
    Console.WriteLine($"  + {user3.DisplayName}");

    PrintSection("BUILDING FRIENDSHIPS");
    user1.AddFriend(user2.Id);
    user2.AddFriend(user1.Id);
    user2.AddFriend(user3.Id);
    user3.AddFriend(user2.Id);

    Console.WriteLine($"  {user1.DisplayName}'s friends: {string.Join(", ", GetFriendNames(user1))}");
    Console.WriteLine($"  {user2.DisplayName}'s friends: {string.Join(", ", GetFriendNames(user2))}");
    Console.WriteLine($"  {user3.DisplayName}'s friends: {string.Join(", ", GetFriendNames(user3))}");

    PrintFooter("User & friendship example complete!");
}

void RunCommentAndReactionExample()
{
    PrintHeader("COMMENTS & REACTIONS EXAMPLE");

    var users = new[]
    {
        platform.UserService.Create("Alice", "Alice", 25),
        platform.UserService.Create("Bob", "Bob", 30),
        platform.UserService.Create("Charlie", "Charlie", 28)
    };

    Console.WriteLine("Creating content...");
    var post = platform.PostService.Create(new PostDto(users[0].Id, "Check out this amazing post!"));
    var reel = platform.ReelService.Create(new MediaDto(users[1].Id, "https://example.com/video.mp4", TimeSpan.FromSeconds(15)));

    Console.WriteLine($"  + Post created by {users[0].DisplayName}");
    Console.WriteLine($"  + Reel created by {users[1].DisplayName}");

    PrintSection("ADDING COMMENTS");
    var c1 = platform.CommentService.Create(new CommentDto(users[1].Id, post.Id, "Love it!"));
    var c2 = platform.CommentService.Create(new CommentDto(users[2].Id, post.Id, "Amazing work!"));

    Console.WriteLine($"  Post: \"{post.Content}\" by {users[0].DisplayName}");
    Console.WriteLine($"\n  + {users[1].DisplayName}: \"{c1.Text}\"");
    Console.WriteLine($"  + {users[2].DisplayName}: \"{c2.Text}\"");

    PrintSection("ADDING REACTIONS");
    platform.ReactionService.Create(new ReactionDto(users[0].Id, post.Id, ReactionType.Like));
    platform.ReactionService.Create(new ReactionDto(users[1].Id, post.Id, ReactionType.Love));
    platform.ReactionService.Create(new ReactionDto(users[2].Id, post.Id, ReactionType.Wow));

    Console.WriteLine($"  Post: \"{post.Content}\" by {users[0].DisplayName}");
    Console.WriteLine($"\n  + {users[0].DisplayName} reacted {ReactionType.Like}");
    Console.WriteLine($"  + {users[1].DisplayName} reacted {ReactionType.Love}");
    Console.WriteLine($"  + {users[2].DisplayName} reacted {ReactionType.Wow}");
    Console.WriteLine($"\n  Post now has {post.CommentIds.Count} comments and {post.ReactionIds.Count} reactions");

    PrintFooter("Comments & reactions example complete!");
}

void DisplayPlatformStatistics()
{
    PrintHeader("PLATFORM STATISTICS");

    var users = platform.UserService.GetAll().Count();
    var posts = platform.PostService.GetAll().Count();
    var reels = platform.ReelService.GetAll().Count();
    var stories = platform.StoryService.GetAll().Count();
    var comments = platform.CommentService.GetAll().Count();
    var reactions = platform.ReactionService.GetAll().Count();

    Console.WriteLine("+-------------------+-------+");
    Console.WriteLine("| Type              | Count |");
    Console.WriteLine("+-------------------+-------+");
    Console.WriteLine($"| Users             | {users,5} |");
    Console.WriteLine($"| Posts             | {posts,5} |");
    Console.WriteLine($"| Reels             | {reels,5} |");
    Console.WriteLine($"| Stories           | {stories,5} |");
    Console.WriteLine($"| Comments          | {comments,5} |");
    Console.WriteLine($"| Reactions         | {reactions,5} |");
    Console.WriteLine("+-------------------+-------+");
    var total = users + posts + reels + stories + comments + reactions;
    Console.WriteLine($"| Total             | {total,5} |");
    Console.WriteLine("+-------------------+-------+");

    PrintFooter("Statistics displayed!");
}


// ============ HELPER METHODS ============

System.Collections.Generic.IEnumerable<string> GetFriendNames(User user)
{
    return user.FriendIds.Select(id => platform.UserService.Get(id)?.DisplayName ?? "Unknown").Where(n => n != null);
}
