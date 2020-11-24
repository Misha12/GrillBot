using System;

namespace Grillbot.Database.Enums.Includes
{
    /// <summary>
    /// Flags for include tables in UsersRepository
    /// </summary>
    [Flags]
    public enum UsersIncludes : long
    {
        None = 0,
        Channels = 1,
        Reminders = 16,
        CreatedInvites = 32,
        Emotes = 64,
        Unverify = 128,
        UnverifyLogIncoming = 256,
        UnverifyLogOutgoing = 512,
        UsedInvite = 1024,

        Invites = CreatedInvites | UsedInvite
    }
}
