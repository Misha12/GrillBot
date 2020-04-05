using Grillbot.Extensions.Discord;
using Grillbot.Database.Entity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Grillbot.Models.TempUnverify;

namespace Grillbot.Services.TempUnverify
{
    public partial class TempUnverifyService
    {
        public async Task<List<CurrentUnverifiedUser>> ListPersonsAsync()
        {
            var persons = await Repository.GetAllItems().ToListAsync();

            if (persons.Count == 0)
                throw new ArgumentException("Nikdo zatím nemá odebraný přístup.");

            return await CreateListsPersonsAsync(persons).ConfigureAwait(false);
        }

        private async Task<List<CurrentUnverifiedUser>> CreateListsPersonsAsync(List<TempUnverifyItem> items)
        {
            var users = new List<CurrentUnverifiedUser>();

            foreach (var person in items)
            {
                var guild = Client.GetGuild(person.GuildIDSnowflake);

                var unverifiedUser = await guild.GetUserFromGuildAsync(person.UserID);
                users.Add(new CurrentUnverifiedUser()
                {
                    ChannelOverrideList = BuildChannelOverrideList(person.DeserializedChannelOverrides, guild),
                    EndDateTime = person.GetEndDatetime(),
                    ID = person.ID,
                    Reason = person.Reason,
                    Roles = person.DeserializedRolesToReturn,
                    Username = unverifiedUser.GetFullName()
                });
            }

            return users;
        }
    }
}
