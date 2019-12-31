using Discord.WebSocket;
using Grillbot.Extensions;
using Grillbot.Extensions.Discord;
using Grillbot.Models.Embed;
using Grillbot.Repository;
using Grillbot.Repository.Entity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Grillbot.Services.TempUnverify
{
    public partial class TempUnverifyService
    {
        public async Task<BotEmbed> ListPersonsAsync(SocketUser caller)
        {
            using (var repository = new TempUnverifyRepository(Config))
            {
                var persons = await repository.GetAllItems().ToListAsync().ConfigureAwait(false);

                if (persons.Count == 0)
                    throw new ArgumentException("Nikdo zatím nemá odebraný přístup.");

                return await CreateListPersonsAsync(persons, caller).ConfigureAwait(false);
            }
        }

        private async Task<BotEmbed> CreateListPersonsAsync(List<TempUnverifyItem> items, SocketUser user)
        {
            var embed = new BotEmbed(user, title: "Seznam osob s odebraným přístupem", thumbnail: Client.CurrentUser.GetUserAvatarUrl());

            foreach (var person in items)
            {
                var guild = Client.GetGuild(person.GuildIDSnowflake);

                var desc = string.Join("\n", new[]
                {
                    $"ID: {person.ID}",
                    $"Do kdy: {person.GetEndDatetime().ToLocaleDatetime()}",
                    $"Role: {string.Join(", ", person.DeserializedRolesToReturn)}",
                    $"Extra kanály: {BuildChannelOverrideList(person.DeserializedChannelOverrides, guild)}",
                    $"Důvod: {person.Reason}"
                });

                var unverifiedUser = await guild.GetUserFromGuildAsync(person.UserID).ConfigureAwait(false);
                embed.AddField(o => o.WithName(unverifiedUser.GetFullName()).WithValue(desc));
            }

            return embed;
        }
    }
}
