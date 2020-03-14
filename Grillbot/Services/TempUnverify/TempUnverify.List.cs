using Discord;
using Discord.WebSocket;
using Grillbot.Extensions;
using Grillbot.Extensions.Discord;
using Grillbot.Models.Embed;
using Grillbot.Database;
using Grillbot.Database.Entity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Grillbot.Services.TempUnverify
{
    public partial class TempUnverifyService
    {
        public async Task<List<BotEmbed>> ListPersonsAsync(SocketUser caller)
        {
            var persons = await Repository.GetAllItems().ToListAsync().ConfigureAwait(false);

            if (persons.Count == 0)
                throw new ArgumentException("Nikdo zatím nemá odebraný přístup.");

            return await CreateListsPersonsAsync(persons, caller).ConfigureAwait(false);
        }

        private async Task<List<BotEmbed>> CreateListsPersonsAsync(List<TempUnverifyItem> items, SocketUser user)
        {
            var fields = new List<EmbedFieldBuilder>();

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
                fields.Add(new EmbedFieldBuilder().WithName(unverifiedUser.GetFullName()).WithValue(desc));
            }

            var embeds = new List<BotEmbed>();
            var pages = System.Math.Ceiling(fields.Count / 10.0);
            for (var i = 0; i < pages; i++)
            {
                var partialFields = fields.Skip(Convert.ToInt32(System.Math.Ceiling(i * 10.0))).Take(10).ToList();

                var embed = new BotEmbed(user, title: "Seznam osob s odebraným přístupem", thumbnail: Client.CurrentUser.GetUserAvatarUrl());
                embed.WithFields(partialFields);

                embeds.Add(embed);
            }

            return embeds;
        }
    }
}
