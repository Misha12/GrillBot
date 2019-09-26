using System;

namespace Grillbot.Services
{
    [Flags]
    public enum AutoReplyParams
    {
        None = 0,
        CaseSensitive = 1,
        Disabled = 2
    }
}
