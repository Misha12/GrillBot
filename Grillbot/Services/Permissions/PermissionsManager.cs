using Discord.Commands;
using Discord.WebSocket;
using Grillbot.Database.Enums;
using Grillbot.Database.Repository;
using Grillbot.Models.Config.AppSettings;
using Microsoft.Extensions.Options;
using System.Linq;

namespace Grillbot.Services.Permissions
{
    public class PermissionsManager
    {
        private Configuration Config { get; }
        private ConfigRepository Repository { get; }

        public PermissionsManager(IOptions<Configuration> options, ConfigRepository repository)
        {
            Config = options.Value;
            Repository = repository;
        }

        public PermissionsResult CheckPermissions(ICommandContext context, CommandInfo command)
        {
            if (context.Guild == null)
            {
                return Config.IsUserBotAdmin(context.User.Id) ? PermissionsResult.Success : PermissionsResult.PMNotAllowed;
            }

            if (Config.IsUserBotAdmin(context.User.Id))
                return PermissionsResult.Success;

            var config = Repository.FindConfig(context.Guild.Id, command.Module.Group, command.Name);

            if (config == null)
                return PermissionsResult.MethodNotFound;

            if (config.OnlyAdmins || config.Permissions.Count == 0)
                return PermissionsResult.OnlyAdmins;

            if (context.Message.Author is SocketGuildUser user)
            {
                var haveBan = config.Permissions.Any(o => o.PermType == PermType.User && o.DiscordIDSnowflake == user.Id && o.AllowType == AllowType.Deny);

                if (haveBan)
                    return PermissionsResult.UserIsBanned;

                foreach (var permission in config.Permissions)
                {
                    switch (permission.PermType)
                    {
                        case PermType.Role:
                            var haveRole = user.Roles.Any(role => permission.DiscordIDSnowflake == role.Id);
                            if (haveRole && permission.AllowType == AllowType.Allow)
                                return PermissionsResult.Success;
                            break;
                        case PermType.User:
                            if (permission.DiscordIDSnowflake == user.Id && permission.AllowType == AllowType.Allow)
                                return PermissionsResult.Success;
                            break;
                    }
                }
            }

            return PermissionsResult.MissingPermissions;
        }
    }
}
