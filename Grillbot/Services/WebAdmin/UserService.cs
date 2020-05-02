using Discord.WebSocket;
using Grillbot.Database.Repository;
using Grillbot.Extensions.Discord;
using Grillbot.Models.Users;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Grillbot.Services.WebAdmin
{
    public class UserService : IDisposable
    {
        private WebAuthRepository WebAuthRepository { get; }
        private DiscordSocketClient DiscordClient { get; }
        private ILogger<UserService> Logger { get; }

        public UserService(WebAuthRepository webAuthRepository, DiscordSocketClient discordClient, ILogger<UserService> logger)
        {
            WebAuthRepository = webAuthRepository;
            DiscordClient = discordClient;
            Logger = logger;
        }

        public async Task<List<WebAdminUser>> GetUsersList()
        {
            var permList = WebAuthRepository.GetAllPerms();
            var result = new List<WebAdminUser>();

            foreach (var perm in permList)
            {
                var guild = DiscordClient.GetGuild(perm.GuildIDSnowflake);

                if (guild == null)
                {
                    Logger.LogWarning("Removed user from database. Guild not found. (Guild:{0}, User:{1})", perm.GuildID, perm.ID);
                    WebAuthRepository.RemoveUser(perm.GuildIDSnowflake, perm.IDSnowflake);
                    continue;
                }

                var user = await guild.GetUserFromGuildAsync(perm.IDSnowflake);

                if (user == null)
                {
                    Logger.LogWarning("Removed user from database. User not found in guild. (Guild:{0} ({1}), User:{2})", guild.Name, guild.Id, perm.ID);
                    WebAuthRepository.RemoveUser(guild.Id, perm.IDSnowflake);
                    continue;
                }

                result.Add(new WebAdminUser(guild, user));
            }

            return result;
        }

        public void Dispose()
        {
            WebAuthRepository.Dispose();
        }
    }
}
