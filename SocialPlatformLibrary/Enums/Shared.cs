using System;

namespace SocialNetworkPlatform.Enums
{
    /// <summary>
    /// Reaction types supported by the platform.
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
    /// Visibility for posts and content.
    /// </summary>
    public enum Visibility : byte
    {
        Public = 1,
        Friends = 2,
        Private = 3
    }
}
