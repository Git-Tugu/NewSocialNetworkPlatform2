using System;
using SocialNetworkPlatform.Platform;
using SocialNetworkPlatform.DTOs;

Console.WriteLine("SocialNetworkPlatform demo\n");

var platform = new Platform();

var alice = platform.UserService.Create("alice123", "Alice", 30);
var bob = platform.UserService.Create("bob456", "Bob", 28);

alice.AddFriend(bob.Id);

var post = platform.PostService.Create(new PostDto(alice.Id, "Hello world from Alice!"));

Console.WriteLine($"User: {alice.Username} ({alice.DisplayName}), Age: {alice.Age}");
Console.WriteLine($"Post by {alice.Username}: {post.Content}");

Console.WriteLine("\nAll users:");
foreach (var u in platform.UserService.GetAll())
{
    Console.WriteLine($" - {u.Username} ({u.DisplayName})");
}

Console.WriteLine("\nAll posts:");
foreach (var p in platform.PostService.GetAll())
{
    Console.WriteLine($" - {p.Content} (by {p.AuthorId})");
}

// Create Reel and Story
var reel = platform.ReelService.Create(new MediaDto(alice.Id, "https://media.example/reel1.mp4", TimeSpan.FromSeconds(30)));
var story = platform.StoryService.Create(new MediaDto(bob.Id, "https://media.example/story1.jpg", null, DateTime.UtcNow.AddHours(12)));

// Views
platform.ReelService.AddView(reel.Id, bob.Id);
platform.StoryService.AddView(story.Id, alice.Id);

Console.WriteLine("\nReels:");
foreach (var r in platform.ReelService.GetAll())
{
    Console.WriteLine($" - Reel {r.Id} by {r.AuthorId}, views: {r.ViewedBy.Count}");
}

Console.WriteLine("\nStories:");
foreach (var s in platform.StoryService.GetAll())
{
    Console.WriteLine($" - Story {s.Id} by {s.AuthorId}, expires: {s.ExpiresAt}, views: {s.ViewedBy.Count}");
}

// Search demo
Console.WriteLine("\nSearch for 'alice':");
foreach (var res in platform.SearchService.SearchAll("alice"))
{
    Console.WriteLine($" - {res.Type}: {res.Id} => {res.Snippet}");
}
