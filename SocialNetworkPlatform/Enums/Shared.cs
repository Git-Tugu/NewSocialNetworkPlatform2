using System;

namespace SocialNetworkPlatform.Enums
{
    /// <summary>
    /// Reaction types supported by the platform. 1.Like, 2. Love, 3. Haha, 4. Wow, 5. Sad, 6. Angry.
    /// </summary>
    public enum ReactionType : byte
    {
        Like = 1,
        Love = 2,
        Haha = 3,
        Wow = 4,
        Sad = 5,
        Angry = 6
    }

    /// <summary>
    /// Visibility for posts and content. 1. Public - visible to everyone, 2. Friends - visible only to friends, 3. Private - visible only to the owner.
    /// </summary>
    public enum Visibility : byte
    {
        Public = 1,
        Friends = 2,
        Private = 3
    }
}
