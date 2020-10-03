using Discord.Commands;
using Discord.WebSocket;
using Grillbot.Database.Enums;
using Grillbot.Database.Enums.Includes;
using Grillbot.Database.Repository;
using Grillbot.Enums;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Grillbot.Services.Permissions
{
    public class PermissionsManager : IDisposable
    {
        private ConfigRepository Repository { get; }
        private BotState BotState { get; }
        private UsersRepository UsersRepository { get; }

        public PermissionsManager(ConfigRepository repository, BotState botState, UsersRepository usersRepository)
        {
            Repository = repository;
            BotState = botState;
            UsersRepository = usersRepository;
        }

        public async Task<PermissionsResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command)
        {
            if (context.Guild == null)
                return PermissionsResult.PMNotAllowed;

            if (BotState.AppInfo.Owner.Id == context.User.Id)
                return PermissionsResult.Success;

            var dbUser = await UsersRepository.GetUserAsync(context.Guild.Id, context.User.Id, UsersIncludes.None);

            if (dbUser == null)
                return PermissionsResult.MissingPermissions;

            if ((dbUser.Flags & (long)UserFlags.BotAdmin) != 0)
                return PermissionsResult.Success;

            var config = Repository.FindConfig(context.Guild.Id, command.Module.Group, command.Name);

            if (config == null)
                return PermissionsResult.MethodNotFound;

            if (config.OnlyAdmins)
                return PermissionsResult.OnlyAdmins;

            if (config.Permissions.Count == 0)
                return PermissionsResult.NoPermissions;

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
                        case PermType.User when permission.DiscordIDSnowflake == user.Id && permission.AllowType == AllowType.Allow:
                        case PermType.Everyone:
                            return PermissionsResult.Success;
                    }
                }
            }

            return PermissionsResult.MissingPermissions;
        }

        public void Dispose()
        {
            Repository.Dispose();
            UsersRepository.Dispose();
        }
    }
}
