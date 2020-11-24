using Discord.Commands;
using Discord.WebSocket;
using Grillbot.Database;
using Grillbot.Database.Enums;
using Grillbot.Database.Enums.Includes;
using Grillbot.Enums;
using System.Linq;
using System.Threading.Tasks;

namespace Grillbot.Services.Permissions
{
    public class PermissionsManager
    {
        private BotState BotState { get; }
        private IGrillBotRepository GrillBotRepository { get; }

        public PermissionsManager(BotState botState, IGrillBotRepository grillBotRepository)
        {
            BotState = botState;
            GrillBotRepository = grillBotRepository;
        }

        public async Task<PermissionsResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command)
        {
            if (context.Guild == null)
                return PermissionsResult.PMNotAllowed;

            if (BotState.AppInfo.Owner.Id == context.User.Id)
                return PermissionsResult.Success;

            var dbUser = await GrillBotRepository.UsersRepository.GetUserAsync(context.Guild.Id, context.User.Id, UsersIncludes.None);

            if (dbUser == null)
                return PermissionsResult.MissingPermissions;

            if ((dbUser.Flags & (long)UserFlags.BotAdmin) != 0)
                return PermissionsResult.Success;

            var config = await GrillBotRepository.ConfigRepository.FindConfigAsync(context.Guild.Id, command.Module.Group, command.Name);

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

                var haveExplicitAllow = config.Permissions.Any(o => o.PermType == PermType.User && o.DiscordIDSnowflake == user.Id && o.AllowType == AllowType.Allow);
                if (haveExplicitAllow)
                    return PermissionsResult.Success;

                foreach (var role in user.Roles)
                {
                    if (config.Permissions.Any(o => o.PermType == PermType.Role && o.DiscordIDSnowflake == role.Id && o.AllowType == AllowType.Deny))
                        return PermissionsResult.RoleIsBanned;
                }

                foreach (var permission in config.Permissions)
                {
                    switch (permission.PermType)
                    {
                        case PermType.Role:
                            var haveRole = user.Roles.Any(role => permission.DiscordIDSnowflake == role.Id);
                            if (haveRole && permission.AllowType == AllowType.Allow)
                                return PermissionsResult.Success;
                            break;
                        case PermType.Everyone:
                            return PermissionsResult.Success;
                    }
                }
            }

            return PermissionsResult.MissingPermissions;
        }
    }
}
