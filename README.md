# SocialNetworkPlatform

Lightweight, demo social networking platform (Facebook/Instagram-like) built with C# / .NET 10. Purpose: demonstrate OOP design, reusable architecture (Services + Repositories), DRY, and easy extension.

## Project layout

- `SocialNetworkPlatform/` - single .NET project containing the demo platform
  - `Models/` - domain models (`User`, `Post`, `Comment`, `Reaction`, `Reel`, `Story`, `ContentItem`, ...)
  - `Enums/` - enums (`ReactionType`, `Visibility`)
  - `DTOs/` - simple DTOs (`PostDto`, `MediaDto`)
  - `Repositories/` - generic `IRepository<T>` and `InMemoryRepository<T>` plus typed repos (`UserRepo`, `PostRepo`, ...)
  - `Services/` - service layer APIs and implementations (`IUserService`/`UserService`, `IPostService`/`PostService`, `IReelService`/`ReelService`, `IStoryService`/`StoryService`)
  - `Platform/Platform.cs` - composition root (wires repos and services)
  - `Program.cs` - small console demo showing typical usage

## Design summary

- Separation of concerns: repositories handle data storage, services contain business logic.
- Reuse and DRY: `ContentItem` abstract base provides common fields for content types (post/comment/reaction/reel/story).
- Simple, testable composition root in `Platform.Platform` - swap implementations or add persistence by providing another `IRepository<T>` implementation.
- `IUserService` and `IPostService` expose common user/post operations (create, delete, update, follow/befriend, create/edit/share posts, visibility checks).

## Key classes / interfaces

- `IRepository<T>` / `InMemoryRepository<T>` - thread-safe in-memory storage
- `IUserService` / `UserService` - user lifecycle (create, delete, update, follow, befriend, get friends)
- `IPostService` / `PostService` - post lifecycle (create, edit, delete, change visibility, share, view authorization)
- `IReelService` / `ReelService` and `IStoryService` / `StoryService` - media content with view counting and expiry
- Domain models: `User`, `Post`, `Comment`, `Reaction`, `Reel`, `Story` (see `Models/`)

## How to run

Requirements:
- .NET 10 SDK
- (Optional) Visual Studio 2022/2025/2026

From the repository root:

```bash
cd SocialNetworkPlatform
dotnet build
dotnet run
```

Or open the `SocialNetworkPlatform.csproj` in Visual Studio and run the project.

## Example usage (from `Program.cs`)

- Create platform composition root:

```csharp
var platform = new Platform();
var alice = platform.UserService.Create("alice123", "Alice", 30);
var post = platform.PostService.Create(new PostDto(alice.Id, "Hello"));
```

- Create a reel and a story, add views:

```csharp
var reel = platform.ReelService.Create(new MediaDto(alice.Id, "https://.../r.mp4", TimeSpan.FromSeconds(30)));
platform.ReelService.AddView(reel.Id, otherUser.Id);
```

- Change post visibility or share:

```csharp
platform.PostService.ChangeVisibility(post.Id, Visibility.Friends);
var shared = platform.PostService.Share(post.Id, byUserId);
```

## Extending the platform

- Persistence: implement `IRepository<T>` with EF Core (DbContext) or other store and register replacements in `Platform.Platform`.
- Authorization: add checks to services (owner-only edit/delete) and return result objects or throw domain-specific exceptions.
- Add `AddComment` / `AddReaction` methods to services and wire to repositories.
- Add unit tests: create a test project that references this project and mock repositories or use `InMemoryRepository<T>`.

## Documentation and coding style

- Public APIs include XML documentation for IntelliSense (`Services/*.cs`, `Models/*.cs`).
- Naming follows C# conventions; `Age` is a `byte` and readonly in `User` model.

## Contribution

This is a demo repository. If you want to extend it:
- Open a branch, make changes, add tests, and create a PR.

## License

MIT-style (adapt as needed).

---

If you want, I can:
- Add a CONTRIBUTING.md or CODE_OF_CONDUCT
- Add unit tests for `UserService` and `PostService`
- Add an EF Core persistence implementation

Tell me which one to do next.
