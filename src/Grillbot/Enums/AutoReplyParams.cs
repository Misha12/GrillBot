using System;

namespace Grillbot.Enums
{
    [Flags]
    public enum AutoReplyParams
    {
        None = 0,
        CaseSensitive = 1,
        Disabled = 2,
        AsCodeBlock = 4
    }
}
