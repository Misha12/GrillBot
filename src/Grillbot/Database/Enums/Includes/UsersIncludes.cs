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
        MathAudit = 4,
        Reminders = 16,
        Invites = 32,
        Emotes = 64,
        Unverify = 128,
        UnverifyLogIncoming = 256,
        UnverifyLogOutgoing = 512,
    }
}
