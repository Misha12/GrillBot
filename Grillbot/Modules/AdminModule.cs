using Discord;
using Discord.Commands;
using Discord.Rest;
using Discord.WebSocket;
using Grillbot.Extensions;
using Grillbot.Extensions.Discord;
using Grillbot.Models.Embed;
using Grillbot.Services.Preconditions;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Grillbot.Modules
{
    [RequirePermissions]
    [Name("Administrační funkce")]
    public class AdminModule : BotModuleBase
    {
        [Command("pinpurge")]
        [Summary("Hromadné odpinování zpráv.")]
        [Remarks("Poslední parametr skipCount je volitelný. Výchozí hodnota je 0.")]
        public async Task PinPurge(string channel, int takeCount, int skipCount = 0)
        {
            await DoAsync(async () =>
            {
                var mentionedChannel = Context.Message.MentionedChannels
                    .OfType<SocketTextChannel>()
                    .FirstOrDefault(o => $"<#{o.Id}>" == channel);

                if (mentionedChannel != null)
                {
                    var pins = await mentionedChannel.GetPinnedMessagesAsync().ConfigureAwait(false);

                    if (pins.Count == 0)
                        throw new ArgumentException($"V kanálu **{mentionedChannel.Mention}** ještě nebylo nic připnuto.");

                    var pinsToRemove = pins
                        .OrderByDescending(o => o.CreatedAt)
                        .Skip(skipCount).Take(takeCount)
                        .OfType<RestUserMessage>();

                    foreach (var pin in pinsToRemove)
                    {
                        await pin.RemoveAllReactionsAsync().ConfigureAwait(false);
                        await pin.UnpinAsync().ConfigureAwait(false);
                    }

                    await ReplyAsync($"Úpěšně dokončeno. Počet odepnutých zpráv: **{pinsToRemove.Count()}**").ConfigureAwait(false);
                }
                else
                {
                    throw new ArgumentException($"Odkazovaný textový kanál **{channel}** nebyl nalezen.");
                }
            }).ConfigureAwait(false);
        }

        [Command("guildStatus")]
        [Summary("Stav serveru")]
        public async Task GuildStatusAsync()
        {
            var guild = Context.Guild;

            var embed = new BotEmbed(Context.Message.Author, title: guild.Name)
                .WithThumbnail(guild.IconUrl)
                .WithFields(
                    new EmbedFieldBuilder().WithName("CategoryChannelsCount").WithValue($"**{guild.CategoryChannels?.Count ?? 0}**").WithIsInline(true),
                    new EmbedFieldBuilder().WithName("ChannelsCount").WithValue($"**{guild.Channels.Count}**").WithIsInline(true),
                    new EmbedFieldBuilder().WithName("CreatedAt").WithValue($"**{guild.CreatedAt.DateTime.ToLocaleDatetime()}**").WithIsInline(true),
                    new EmbedFieldBuilder().WithName("HasAllMembers").WithValue($"**{guild.HasAllMembers}**").WithIsInline(true),
                    new EmbedFieldBuilder().WithName("IsEmbeddable").WithValue($"**{guild.IsEmbeddable}**").WithIsInline(true),
                    new EmbedFieldBuilder().WithName("IsSynced").WithValue($"**{guild.IsSynced}**").WithIsInline(true),
                    new EmbedFieldBuilder().WithName("MemberCount").WithValue($"**{guild.MemberCount}**").WithIsInline(true),
                    new EmbedFieldBuilder().WithName("CachedUsersCount").WithValue($"**{guild.Users.Count}**").WithIsInline(true),
                    new EmbedFieldBuilder().WithName("RolesCount").WithValue($"**{guild.Roles.Count}**").WithIsInline(true),
                    new EmbedFieldBuilder().WithName("OwnerID").WithValue($"**{guild.OwnerId}**").WithIsInline(true),
                    new EmbedFieldBuilder().WithName("SplashID").WithValue($"**{guild.SplashId?.ToString() ?? "null"}**").WithIsInline(true),
                    new EmbedFieldBuilder().WithName("IconID").WithValue($"**{guild.IconId}**").WithIsInline(true),
                    new EmbedFieldBuilder().WithName("VerificationLevel").WithValue($"**{guild.VerificationLevel}**").WithIsInline(true),
                    new EmbedFieldBuilder().WithName("VoiceRegionID").WithValue($"**{guild.VoiceRegionId ?? "null"}**").WithIsInline(true),
                    new EmbedFieldBuilder().WithName("MfaLevel").WithValue($"**{guild.MfaLevel}**").WithIsInline(true),
                    new EmbedFieldBuilder().WithName("ExplicitContentFilter").WithValue($"**{guild.ExplicitContentFilter}**").WithIsInline(true),
                    new EmbedFieldBuilder().WithName("SystemChannel").WithValue($"**{guild.SystemChannel?.Name ?? "None"}**").WithIsInline(true),
                    new EmbedFieldBuilder().WithName("DefaultMessageNotifications").WithValue($"**{guild.DefaultMessageNotifications}**").WithIsInline(true)
                );

            await ReplyAsync(embed: embed.Build()).ConfigureAwait(false);
        }

        [Command("syncGuild")]
        [Summary("Synchronizace serveru s botem.")]
        public async Task SyncGuild()
        {
            try
            {
                await Context.Guild.SyncGuildAsync().ConfigureAwait(false);
                await ReplyAsync("Synchronizace dokončena").ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                await ReplyAsync($"Synchronizace se nezdařila ({ex.Message.PreventMassTags()}).").ConfigureAwait(false);
                throw;
            }
        }
    }
}
