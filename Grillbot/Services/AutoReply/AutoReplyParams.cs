using System;

namespace Grillbot.Services.AutoReply
{
    [Flags]
    public enum AutoReplyParams
    {
        None = 0,
        CaseSensitive = 1,
        Disabled = 2
    }
}
