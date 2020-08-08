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
        Birthday = 2,
        MathAudit = 4,
        Statistics = 8,
        Reminders = 16,
        Invites = 32,
        Emotes = 64,
        Unverify = 128,

        All = Channels | Birthday | MathAudit | Statistics | Reminders | Invites | Emotes | Unverify
    }
}
