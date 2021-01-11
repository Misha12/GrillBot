using Discord;
using Discord.WebSocket;
using System.Collections.Generic;
using System.Linq;

namespace Grillbot.Extensions.Discord
{
    public static class SocketGuildChannelExtensions
    {
        public static bool HaveAccess(this SocketGuildChannel channel, SocketGuildUser user)
        {
            if (channel.GetUser(user.Id) != null)
                return true;

            if (channel.PermissionOverwrites.Count == 0)
                return true; // Default permissions. Access all

            var overwrite = channel.GetPermissionOverwrite(user);
            if (overwrite != null)
            {
                // Specific user overwrite
                if (overwrite.Value.ViewChannel == PermValue.Allow)
                {
                    return true;
                }
                else if (overwrite.Value.ViewChannel == PermValue.Deny)
                {
                    return false;
                }
            }

            var everyonePerm = channel.GetPermissionOverwrite(user.Guild.EveryoneRole);
            var isEveryonePerm = everyonePerm != null && (everyonePerm.Value.ViewChannel == PermValue.Allow || everyonePerm.Value.ViewChannel == PermValue.Inherit);

            foreach (var role in user.Roles.Where(o => !o.IsEveryone))
            {
                var roleOverwrite = channel.GetPermissionOverwrite(role);
                if (roleOverwrite == null) continue;

                if (roleOverwrite.Value.ViewChannel == PermValue.Deny && isEveryonePerm)
                    return false;

                if (roleOverwrite.Value.ViewChannel == PermValue.Allow)
                    return true;
            }

            return isEveryonePerm;
        }

        public static bool IsEquals(this SocketGuildChannel before, SocketGuildChannel after)
        {
            if (before.Name != after.Name || before.Position != after.Position)
                return false;

            if (before is SocketTextChannel beforeTextChannel && after is SocketTextChannel afterTextChannel)
            {
                return beforeTextChannel.IsNsfw == afterTextChannel.IsNsfw && beforeTextChannel.CategoryId == afterTextChannel.CategoryId
                    && beforeTextChannel.SlowModeInterval == afterTextChannel.SlowModeInterval && beforeTextChannel.Topic == afterTextChannel.Topic;
            }
            else if (before is SocketVoiceChannel beforeVoiceChannel && after is SocketVoiceChannel afterVoiceChannel)
            {
                return beforeVoiceChannel.Bitrate == afterVoiceChannel.Bitrate && beforeVoiceChannel.UserLimit == afterVoiceChannel.UserLimit
                    && beforeVoiceChannel.CategoryId == afterVoiceChannel.CategoryId;
            }
            else if (before is SocketCategoryChannel && after is SocketCategoryChannel)
            {
                return true;
            }

            return false;
        }
    }
}
