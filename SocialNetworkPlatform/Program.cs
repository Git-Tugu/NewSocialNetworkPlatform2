using System;
using SocialNetworkPlatform.Platform;
using SocialNetworkPlatform.DTOs;
using SocialNetworkPlatform.Enums;

Console.WriteLine("=== SocialNetworkPlatform Demo ===\n");

var platform = new Platform();

// Create users
Console.WriteLine("--- Creating Users ---");
var alice = platform.UserService.Create("alice123", "Alice", 30);
var bob = platform.UserService.Create("bob456", "Bob", 28);
var charlie = platform.UserService.Create("charlie789", "Charlie", 25);

alice.AddFriend(bob.Id);
bob.AddFriend(alice.Id);
bob.AddFriend(charlie.Id);

Console.WriteLine($"Created: {alice.DisplayName}, {bob.DisplayName}, {charlie.DisplayName}\n");

// Create content
Console.WriteLine("--- Creating Content ---");
var post = platform.PostService.Create(new PostDto(alice.Id, "Hello world from Alice!"));
var post2 = platform.PostService.Create(new PostDto(bob.Id, "Bob's first post!"));
var reel = platform.ReelService.Create(new MediaDto(alice.Id, "https://media.example/reel1.mp4", TimeSpan.FromSeconds(30)));
var story = platform.StoryService.Create(new MediaDto(bob.Id, "https://media.example/story1.jpg", null, DateTime.UtcNow.AddHours(24)));

Console.WriteLine($"Created: Post (by {alice.DisplayName}), Post (by {bob.DisplayName}), Reel, Story\n");

// Add views
platform.ReelService.AddView(reel.Id, bob.Id);
platform.ReelService.AddView(reel.Id, charlie.Id);
platform.StoryService.AddView(story.Id, alice.Id);

// Create comments
Console.WriteLine("--- Creating Comments ---");
var comment1 = platform.CommentService.Create(new CommentDto(bob.Id, post.Id, "Great post, Alice!"));
var comment2 = platform.CommentService.Create(new CommentDto(charlie.Id, post.Id, "I agree!"));
var reelComment = platform.CommentService.Create(new CommentDto(bob.Id, reel.Id, "Awesome reel!"));
var storyComment = platform.CommentService.Create(new CommentDto(alice.Id, story.Id, "Nice story!"));

Console.WriteLine($"Added {post.CommentIds.Count} comments to post");
Console.WriteLine($"Added {reel.CommentIds.Count} comments to reel");
Console.WriteLine($"Added {story.CommentIds.Count} comments to story\n");

// Create reactions
Console.WriteLine("--- Creating Reactions ---");
var reaction1 = platform.ReactionService.Create(new ReactionDto(charlie.Id, post.Id, ReactionType.Like));
var reaction2 = platform.ReactionService.Create(new ReactionDto(alice.Id, post.Id, ReactionType.Love));
var commentReaction = platform.ReactionService.Create(new ReactionDto(alice.Id, comment1.Id, ReactionType.Like));
var reelReaction = platform.ReactionService.Create(new ReactionDto(charlie.Id, reel.Id, ReactionType.Love));

Console.WriteLine($"Added {post.ReactionIds.Count} reactions to post");
Console.WriteLine($"Added {comment1.ReactionIds.Count} reactions to comment");
Console.WriteLine($"Added {reel.ReactionIds.Count} reactions to reel\n");

// Display all content
Console.WriteLine("--- All Posts ---");
foreach (var p in platform.PostService.GetAll())
{
    Console.WriteLine($"  '{p.Content}' by {platform.UserService.Get(p.AuthorId)?.DisplayName}");
    Console.WriteLine($"    Comments: {p.CommentIds.Count}, Reactions: {p.ReactionIds.Count}");
}

Console.WriteLine("\n--- All Comments ---");
foreach (var c in platform.CommentService.GetAll())
{
    Console.WriteLine($"  '{c.Text}' on {c.TargetId}");
    Console.WriteLine($"    Reactions: {c.ReactionIds.Count}");
}

Console.WriteLine("\n--- All Reactions ---");
foreach (var r in platform.ReactionService.GetAll())
{
    Console.WriteLine($"  {r.Type} reaction by {platform.UserService.Get(r.AuthorId)?.DisplayName} on {r.TargetId}");
}

Console.WriteLine("\n--- Reel Details ---");
foreach (var r in platform.ReelService.GetAll())
{
    Console.WriteLine($"  Views: {r.ViewedBy.Count}, Comments: {r.CommentIds.Count}, Reactions: {r.ReactionIds.Count}");
}

Console.WriteLine("\n--- Story Details ---");
foreach (var s in platform.StoryService.GetAll())
{
    Console.WriteLine($"  Views: {s.ViewedBy.Count}, Comments: {s.CommentIds.Count}");
}

// Demonstrate cascade deletion
Console.WriteLine("\n--- Testing Cascade Deletion ---");
Console.WriteLine($"Before delete: Post has {post.CommentIds.Count} comments and {post.ReactionIds.Count} reactions");
Console.WriteLine("Deleting post...");
platform.PostService.Delete(post.Id);
Console.WriteLine($"After delete: Total comments = {platform.CommentService.GetAll().Count()}");
Console.WriteLine($"              Total reactions = {platform.ReactionService.GetAll().Count()}");

Console.WriteLine("\nDemo Complete!");

