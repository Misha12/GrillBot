using Discord;

namespace Grillbot.Services.Unverify.Models
{
    public class ChannelOverwrite
    {
        public IChannel Channel { get; set; }
        public OverwritePermissions Permissions { get; set; }
    }
}
