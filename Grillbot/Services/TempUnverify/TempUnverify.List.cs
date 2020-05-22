using Grillbot.Extensions.Discord;
using Grillbot.Database.Entity;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;
using Grillbot.Models.TempUnverify;
using System.Linq;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Grillbot.Database.Repository;

namespace Grillbot.Services.TempUnverify
{
    public partial class TempUnverifyService
    {
        public async Task<List<CurrentUnverifiedUser>> ListPersonsAsync(SocketGuild guild)
        {
            using var scope = Provider.CreateScope();
            using var repository = scope.ServiceProvider.GetService<TempUnverifyRepository>();
            var persons = await repository.GetAllItems(guild).ToListAsync();
            return await CreateListsPersonsAsync(persons).ConfigureAwait(false);
        }

        private async Task<List<CurrentUnverifiedUser>> CreateListsPersonsAsync(List<TempUnverifyItem> items)
        {
            var users = new List<CurrentUnverifiedUser>();

            foreach (var person in items)
            {
                var guild = Client.GetGuild(person.GuildIDSnowflake);

                var unverifiedUser = await guild.GetUserFromGuildAsync(person.UserIDSnowflake);
                users.Add(new CurrentUnverifiedUser()
                {
                    ChannelOverrideList = BuildChannelOverrideList(person.DeserializedChannelOverrides, guild),
                    EndDateTime = person.GetEndDatetime(),
                    ID = person.ID,
                    Reason = person.Reason,
                    Roles = person.DeserializedRolesToReturn.Select(id => guild.GetRole(id)).Where(role => role != null).OrderByDescending(o => o.Position).Select(o => o.Name).ToList(),
                    Username = unverifiedUser.GetFullName(),
                    GuildName = guild.Name
                });
            }

            return users;
        }
    }
}
