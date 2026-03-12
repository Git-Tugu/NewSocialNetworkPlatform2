using System;
using System.Linq;
using SocialNetworkPlatform.Platform;
using SocialNetworkPlatform.DTOs;
using SocialNetworkPlatform.Enums;
using SocialNetworkPlatform.Models;

var platform = new Platform();
User currentUser = null;
bool appRunning = true;

Console.Clear();
PrintHeader("SocialNetworkPlatform");
Console.WriteLine("Welcome to the Social Network Platform!\n");

while (appRunning)
{
    if (currentUser == null)
    {
        DisplayAuthMenu();
    }
    else
    {
        DisplayMainMenu();
    }
}

Console.WriteLine("\nThank you for using SocialNetworkPlatform. Goodbye!\n");

// ============ AUTHENTICATION ============

void DisplayAuthMenu()
{
    PrintMenu("AUTHENTICATION MENU");
    Console.WriteLine("|  1. Create New Account                     |");
    Console.WriteLine("|  2. Login to Existing Account              |");
    Console.WriteLine("|  3. Exit Application                       |");
    Console.WriteLine("+--------------------------------------------+");

    Console.Write("\nSelect an option: ");
    string choice = Console.ReadLine() ?? "0";

    switch (choice)
    {
        case "1":
            CreateNewAccount();
            break;
        case "2":
            LoginToAccount();
            break;
        case "3":
            appRunning = false;
            break;
        default:
            Console.WriteLine("Invalid option. Please try again.\n");
            break;
    }
}

void CreateNewAccount()
{
    Console.Clear();
    PrintHeader("CREATE NEW ACCOUNT");

    Console.Write("Enter username: ");
    string username = Console.ReadLine() ?? "";

    Console.Write("Enter display name: ");
    string displayName = Console.ReadLine() ?? "";

    Console.Write("Enter age: ");
    if (!byte.TryParse(Console.ReadLine(), out byte age) || age < 1)
    {
        Console.WriteLine("Invalid age. Please try again.\n");
        return;
    }

    if (platform.UserService.GetAll().Any(u => u.Username == username))
    {
        Console.WriteLine("Username already exists. Please try again.\n");
        return;
    }

    currentUser = platform.UserService.Create(username, displayName, age);
    Console.WriteLine($"\nAccount created successfully!");
    Console.WriteLine($"Welcome, {currentUser.DisplayName}!\n");
    System.Threading.Thread.Sleep(1500);
}

void LoginToAccount()
{
    Console.Clear();
    PrintHeader("LOGIN TO ACCOUNT");

    Console.Write("Enter username: ");
    string username = Console.ReadLine() ?? "";

    var user = platform.UserService.GetAll().FirstOrDefault(u => u.Username == username);
    if (user != null)
    {
        currentUser = user;
        Console.WriteLine($"\nLogin successful! Welcome, {currentUser.DisplayName}!\n");
        System.Threading.Thread.Sleep(1500);
    }
    else
    {
        Console.WriteLine("Username not found. Please try again.\n");
    }
}

// ============ MAIN APPLICATION ============

void DisplayMainMenu()
{
    Console.Clear();
    PrintHeader($"MAIN MENU - {currentUser.DisplayName}");
    Console.WriteLine("|  1. View Feed                              |");
    Console.WriteLine("|  2. Create Post                            |");
    Console.WriteLine("|  3. Manage Friends                         |");
    Console.WriteLine("|  4. View Profile                           |");
    Console.WriteLine("|  5. Search Users                           |");
    Console.WriteLine("|  6. View All Posts                         |");
    Console.WriteLine("|  7. Logout                                 |");
    Console.WriteLine("+--------------------------------------------+");

    Console.Write("\nSelect an option: ");
    string choice = Console.ReadLine() ?? "0";

    try
    {
        switch (choice)
        {
            case "1":
                ViewFeed();
                break;
            case "2":
                CreatePost();
                break;
            case "3":
                ManageFriends();
                break;
            case "4":
                ViewProfile();
                break;
            case "5":
                SearchUsers();
                break;
            case "6":
                ViewAllPosts();
                break;
            case "7":
                Logout();
                break;
            default:
                Console.WriteLine("Invalid option. Please try again.\n");
                System.Threading.Thread.Sleep(1000);
                break;
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"\nError: {ex.Message}\n");
        System.Threading.Thread.Sleep(1500);
    }
}

void ViewFeed()
{
    Console.Clear();
    PrintHeader("YOUR FEED");

    var friendPosts = platform.PostService.GetAll()
        .Where(p => p.AuthorId == currentUser.Id ||
                    currentUser.FriendIds.Contains(p.AuthorId))
        .OrderByDescending(p => p.Id)
        .ToList();

    if (friendPosts.Count == 0)
    {
        Console.WriteLine("Your feed is empty. Add friends to see their posts.\n");
    }
    else
    {
        foreach (var post in friendPosts)
        {
            var author = platform.UserService.Get(post.AuthorId);
            Console.WriteLine("+-----------------------------------+");
            Console.WriteLine($"| {author.DisplayName,33} |");
            Console.WriteLine("+-----------------------------------+");
            Console.WriteLine($"| {(post.Content.Length > 35 ? post.Content.Substring(0, 32) + "..." : post.Content).PadRight(35)} |");
            Console.WriteLine("+-----------------------------------+");
            Console.WriteLine($"| Comments: {post.CommentIds.Count,-24} |");
            Console.WriteLine($"| Reactions: {post.ReactionIds.Count,-24} |");
            Console.WriteLine("+-----------------------------------+\n");
        }
    }

    Console.Write("Press Enter to continue...");
    Console.ReadLine();
}

void CreatePost()
{
    Console.Clear();
    PrintHeader("CREATE NEW POST");

    Console.Write("Enter your post content: ");
    string content = Console.ReadLine() ?? "";

    if (string.IsNullOrWhiteSpace(content))
    {
        Console.WriteLine("Post content cannot be empty.\n");
        System.Threading.Thread.Sleep(1500);
        return;
    }

    var post = platform.PostService.Create(new PostDto(currentUser.Id, content));
    Console.WriteLine($"\nPost created successfully!\n");
    System.Threading.Thread.Sleep(1500);
}

void ManageFriends()
{
    bool managing = true;
    while (managing)
    {
        Console.Clear();
        PrintHeader("MANAGE FRIENDS");
        Console.WriteLine("|  1. View Friends                           |");
        Console.WriteLine("|  2. Add Friend                             |");
        Console.WriteLine("|  3. Remove Friend                          |");
        Console.WriteLine("|  4. Back to Main Menu                      |");
        Console.WriteLine("+--------------------------------------------+");

        Console.Write("\nSelect an option: ");
        string choice = Console.ReadLine() ?? "0";

        switch (choice)
        {
            case "1":
                ViewFriends();
                break;
            case "2":
                AddFriend();
                break;
            case "3":
                RemoveFriend();
                break;
            case "4":
                managing = false;
                break;
            default:
                Console.WriteLine("Invalid option. Please try again.\n");
                System.Threading.Thread.Sleep(1000);
                break;
        }
    }
}

void ViewFriends()
{
    Console.Clear();
    PrintHeader("YOUR FRIENDS");

    var friends = currentUser.FriendIds.Select(id => platform.UserService.Get(id)).Where(f => f != null).ToList();

    if (friends.Count == 0)
    {
        Console.WriteLine("You have no friends yet.\n");
    }
    else
    {
        foreach (var friend in friends)
        {
            Console.WriteLine($"+ {friend.DisplayName} (Age: {friend.Age})");
        }
        Console.WriteLine("");
    }

    Console.Write("Press Enter to continue...");
    Console.ReadLine();
}

void AddFriend()
{
    Console.Clear();
    PrintHeader("ADD FRIEND");

    Console.Write("Enter username of friend to add: ");
    string username = Console.ReadLine() ?? "";

    var friend = platform.UserService.GetAll().FirstOrDefault(u => u.Username == username);
    if (friend == null)
    {
        Console.WriteLine("User not found.\n");
        System.Threading.Thread.Sleep(1500);
        return;
    }

    if (friend.Id == currentUser.Id)
    {
        Console.WriteLine("You cannot add yourself as a friend.\n");
        System.Threading.Thread.Sleep(1500);
        return;
    }

    if (currentUser.FriendIds.Contains(friend.Id))
    {
        Console.WriteLine("Already friends with this user.\n");
        System.Threading.Thread.Sleep(1500);
        return;
    }

    currentUser.AddFriend(friend.Id);
    Console.WriteLine($"Added {friend.DisplayName} as a friend!\n");
    System.Threading.Thread.Sleep(1500);
}

void RemoveFriend()
{
    Console.Clear();
    PrintHeader("REMOVE FRIEND");

    Console.Write("Enter username of friend to remove: ");
    string username = Console.ReadLine() ?? "";

    var friend = platform.UserService.GetAll().FirstOrDefault(u => u.Username == username);
    if (friend == null)
    {
        Console.WriteLine("User not found.\n");
        System.Threading.Thread.Sleep(1500);
        return;
    }

    if (!currentUser.FriendIds.Contains(friend.Id))
    {
        Console.WriteLine("Not friends with this user.\n");
        System.Threading.Thread.Sleep(1500);
        return;
    }

    currentUser.RemoveFriend(friend.Id);
    Console.WriteLine($"Removed {friend.DisplayName} from friends.\n");
    System.Threading.Thread.Sleep(1500);
}

void ViewProfile()
{
    Console.Clear();
    PrintHeader(currentUser.DisplayName.ToUpper());

    Console.WriteLine("+-----------------------------------+");
    Console.WriteLine($"| Age: {currentUser.Age,-28} |");
    Console.WriteLine($"| Friends: {currentUser.FriendIds.Count,-25} |");
    Console.WriteLine($"| Posts: {platform.PostService.GetAll().Count(p => p.AuthorId == currentUser.Id),-27} |");
    Console.WriteLine("+-----------------------------------+");

    Console.WriteLine("\nYour Posts:");
    var userPosts = platform.PostService.GetAll().Where(p => p.AuthorId == currentUser.Id).ToList();

    if (userPosts.Count == 0)
    {
        Console.WriteLine("You have no posts yet.\n");
    }
    else
    {
        foreach (var post in userPosts)
        {
            Console.WriteLine($"+ {post.Content}");
            Console.WriteLine($"  {post.CommentIds.Count} comments, {post.ReactionIds.Count} reactions\n");
        }
    }

    Console.Write("Press Enter to continue...");
    Console.ReadLine();
}

void SearchUsers()
{
    Console.Clear();
    PrintHeader("SEARCH USERS");

    Console.Write("Enter username to search: ");
    string searchTerm = Console.ReadLine() ?? "";

    var results = platform.UserService.GetAll()
        .Where(u => u.Username.Contains(searchTerm) || u.DisplayName.Contains(searchTerm))
        .ToList();

    if (results.Count == 0)
    {
        Console.WriteLine("No users found.\n");
    }
    else
    {
        Console.WriteLine("\nSearch Results:");
        foreach (var user in results)
        {
            bool isFriend = currentUser.FriendIds.Contains(user.Id);
            string status = user.Id == currentUser.Id ? "[YOU]" : (isFriend ? "[FRIEND]" : "[NOT FRIEND]");
            Console.WriteLine($"+ {user.DisplayName} (@{user.Username}) {status}");
        }
        Console.WriteLine("");
    }

    Console.Write("Press Enter to continue...");
    Console.ReadLine();
}

void ViewAllPosts()
{
    Console.Clear();
    PrintHeader("ALL POSTS");

    var allPosts = platform.PostService.GetAll().OrderByDescending(p => p.Id).ToList();

    if (allPosts.Count == 0)
    {
        Console.WriteLine("No posts yet.\n");
    }
    else
    {
        foreach (var post in allPosts)
        {
            var author = platform.UserService.Get(post.AuthorId);
            Console.WriteLine("+-----------------------------------+");
            Console.WriteLine($"| {author.DisplayName,33} |");
            Console.WriteLine("+-----------------------------------+");
            Console.WriteLine($"| {(post.Content.Length > 35 ? post.Content.Substring(0, 32) + "..." : post.Content).PadRight(35)} |");
            Console.WriteLine("+-----------------------------------+");
            Console.WriteLine($"| Comments: {post.CommentIds.Count,-24} |");
            Console.WriteLine($"| Reactions: {post.ReactionIds.Count,-24} |");
            Console.WriteLine("+-----------------------------------+\n");
        }
    }

    Console.Write("Press Enter to continue...");
    Console.ReadLine();
}

void Logout()
{
    currentUser = null;
    Console.WriteLine("Logged out successfully.\n");
    System.Threading.Thread.Sleep(1500);
}


// ============ HELPER METHODS ============

void PrintHeader(string title)
{
    Console.WriteLine("+--------------------------------------------+");
    Console.WriteLine($"| {title.PadRight(42)} |");
    Console.WriteLine("+--------------------------------------------+");
}

void PrintMenu(string title)
{
    Console.WriteLine("+--------------------------------------------+");
    Console.WriteLine($"| {title.PadRight(42)} |");
    Console.WriteLine("+--------------------------------------------+");
}