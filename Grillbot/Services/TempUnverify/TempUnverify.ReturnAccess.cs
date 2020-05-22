using Discord.WebSocket;
using Grillbot.Extensions.Discord;
using Grillbot.Database.Entity;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Grillbot.Extensions;
using Grillbot.Exceptions;
using Grillbot.Database.Repository;
using Microsoft.Extensions.DependencyInjection;

namespace Grillbot.Services.TempUnverify
{
    public partial class TempUnverifyService
    {
        private void ReturnAccess(object item)
        {
            if (item is TempUnverifyItem unverify)
            {
                using var scope = Provider.CreateScope();
                using var repository = scope.ServiceProvider.GetService<TempUnverifyRepository>();

                if (!repository.UnverifyExists(unverify.ID))
                {
                    if (Data.Any(o => o.ID == unverify.ID))
                        Data.RemoveAll(o => o.ID == unverify.ID);

                    unverify.Dispose();
                    return;
                }

                var guild = Client.GetGuild(unverify.GuildIDSnowflake);
                if (guild == null) return;

                var user = guild.GetUserFromGuildAsync(unverify.UserIDSnowflake).Result;
                if (user == null)
                {
                    Logger.LogWarning($"Invalid unverify. User not found. {JsonConvert.SerializeObject(unverify)}");
                    return;
                }

                var rolesToReturn = unverify.DeserializedRolesToReturn;
                var roles = rolesToReturn.Select(id => guild.GetRole(id)).Where(role => role != null && !user.Roles.Any(x => x.Id == role.Id)).ToList();

                var isAutoRemove = (unverify.GetEndDatetime() - DateTime.Now).Ticks <= 0;
                if (isAutoRemove)
                {
                    using var logService = scope.ServiceProvider.GetService<TempUnverifyLogService>();
                    logService.LogAutoRemove(unverify, user, guild);
                }

                var overrides = unverify.DeserializedChannelOverrides
                    .Select(o => new { channel = guild.GetChannel(o.ChannelIdSnowflake), channelOverride = o })
                    .Where(o => o.channel != null)
                    .ToList();

                user.AddRolesAsync(roles).RunSync();
                foreach (var channelOverride in overrides)
                {
                    channelOverride.channel
                        .AddPermissionOverwriteAsync(user, channelOverride.channelOverride.GetPermissions())
                        .RunSync();
                }

                FindAndToggleMutedRoleAsync(user, guild, false).RunSync();
                RemoveOverwritesForPreprocessedChannels(user, guild, overrides.Select(o => o.channelOverride).ToList()).RunSync();

                repository.RemoveItem(unverify.ID);
                unverify.Dispose();
                Data.RemoveAll(o => o.ID == unverify.ID);
            }
        }

        public async Task<string> ReturnAccessAsync(int id, SocketUser fromUser)
        {
            using var scope = Provider.CreateScope();
            using var repository = scope.ServiceProvider.GetService<TempUnverifyRepository>();
            var item = await repository.FindItemByIDAsync(id);

            if (item == null)
                throw new NotFoundException($"Odebrání přístupu s ID {id} nebylo v databázi nalezeno.");

            var guild = Client.GetGuild(Convert.ToUInt64(item.GuildID));
            var user = await guild.GetUserFromGuildAsync(item.UserID);

            if (user == null)
                throw new NotFoundException($"Uživatel s ID **{item.UserID}** nebyl na serveru **{guild.Name}** nalezen.");

            using var logService = scope.ServiceProvider.GetService<TempUnverifyLogService>();
            logService.LogRemove(item, user, fromUser, guild);

            ReturnAccess(item);
            return $"Předčasné vrácení přístupu pro uživatele **{user.GetFullName()}** bylo dokončeno.";
        }
    }
}
