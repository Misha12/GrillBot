using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Grillbot.Attributes;
using Grillbot.Database.Repository;
using Grillbot.Extensions;
using Grillbot.Extensions.Discord;
using Grillbot.Models.Embed;
using Grillbot.Services.AdminServices;
using Grillbot.Services.MessageCache;
using Grillbot.Services.Permissions.Preconditions;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Grillbot.Modules
{
    [RequirePermissions]
    [ModuleID("AdminModule")]
    [Name("Administrační funkce")]
    public class AdminModule : BotModuleBase
    {
        private IMessageCache MessageCache { get; }
        private PinManagement PinManagement { get; }

        public AdminModule(ConfigRepository config, IMessageCache messageCache, PinManagement pinManagement) : base(configRepository: config)
        {
            MessageCache = messageCache;
            PinManagement = pinManagement;
        }

        [Command("pinpurge")]
        [Summary("Hromadné odpinování zpráv.")]
        [Remarks("Poslední parametr skipCount je volitelný. Výchozí hodnota je 0.")]
        public async Task PinPurge(string channel, int takeCount, int skipCount = 0)
        {
            var mentionedChannel = Context.Message.MentionedChannels
                .OfType<SocketTextChannel>()
                .FirstOrDefault(o => $"<#{o.Id}>" == channel);

            if (mentionedChannel != null)
            {
                await PinManagement.PinPurgeAsync(mentionedChannel, takeCount, skipCount);
                return;
            }

            await ReplyAsync($"Odkazovaný textový kanál **{channel}** nebyl nalezen.");
        }

        [Command("guildStatus")]
        [Summary("Informace o serveru.")]
        public async Task GuildStatusAsync()
        {
            var guild = Context.Guild;

            var color = guild.Roles.FindHighestRoleWithColor()?.Color;
            var embed = new BotEmbed(Context.Message.Author, color, title: guild.Name)
                .WithThumbnail(guild.IconUrl)
                .WithFields(
                    new EmbedFieldBuilder().WithName("Počet kategorií").WithValue($"**{guild.CategoryChannels?.Count ?? 0}**").WithIsInline(true),
                    new EmbedFieldBuilder().WithName("Počet textových kanálů").WithValue($"**{guild.ComputeTextChannelsCount()}**").WithIsInline(true),
                    new EmbedFieldBuilder().WithName("Počet hlasových kanálů").WithValue($"**{guild.ComputeVoiceChannelsCount()}**").WithIsInline(true),
                    new EmbedFieldBuilder().WithName("Počet rolí").WithValue($"**{guild.Roles.Count}**").WithIsInline(true),
                    new EmbedFieldBuilder().WithName("Vytvořen").WithValue($"**{guild.CreatedAt.DateTime.ToLocaleDatetime()}**").WithIsInline(true),
                    new EmbedFieldBuilder().WithName("Vlastník").WithValue($"**{guild.Owner.GetFullName()}** ({guild.OwnerId})"),
                    new EmbedFieldBuilder().WithName("Systémový kanál").WithValue($"**{guild.SystemChannel?.Name ?? "None"}** ({guild.SystemChannel?.Id ?? 0})"),
                    new EmbedFieldBuilder().WithName("Uživatelé synchronizováni").WithValue($"**{(guild.HasAllMembers ? "Ano" : "Ne")}**").WithIsInline(true),
                    new EmbedFieldBuilder().WithName("Synchronizován").WithValue($"**{(guild.IsSynced ? "Ano" : "Ne")}**").WithIsInline(true),
                    new EmbedFieldBuilder().WithName("Počet uživatelů (v paměti)").WithValue($"**{guild.MemberCount}** (**{guild.Users.Count}**)").WithIsInline(true),
                    new EmbedFieldBuilder().WithName("Úroveň ověření").WithValue($"**{guild.VerificationLevel}**").WithIsInline(true),
                    new EmbedFieldBuilder().WithName("ID oblasti (Hovory)").WithValue($"**{guild.VoiceRegionId ?? "null"}**").WithIsInline(true),
                    new EmbedFieldBuilder().WithName("Úroveň MFA").WithValue($"**{guild.MfaLevel}**").WithIsInline(true),
                    new EmbedFieldBuilder().WithName("Filtr explicitního obsahu").WithValue($"**{guild.ExplicitContentFilter}**").WithIsInline(true),
                    new EmbedFieldBuilder().WithName("Výchozí notifikace").WithValue($"**{guild.DefaultMessageNotifications}**").WithIsInline(true),
                    new EmbedFieldBuilder().WithName("Extra funkce").WithValue(guild.Features.Count == 0 ? "-" : string.Join(", ", guild.Features)),
                    new EmbedFieldBuilder().WithName("Tier").WithValue(guild.PremiumTier.ToString()).WithIsInline(true),
                    new EmbedFieldBuilder().WithName("Počet boosterů").WithValue(guild.PremiumSubscriptionCount).WithIsInline(true)
                );

            await ReplyAsync(embed: embed.Build());
        }

        [Command("syncGuild")]
        [Summary("Synchronizace serveru s botem.")]
        public async Task SyncGuild()
        {
            try
            {
                await Context.Guild.SyncGuildAsync();
                await ReplyAsync("Synchronizace dokončena");
            }
            catch (Exception ex)
            {
                await ReplyAsync($"Synchronizace se nezdařila ({ex.Message.PreventMassTags()}).");
                throw;
            }
        }

        [Command("clear")]
        [Summary("Hromadné mazání zpráv.")]
        public async Task ClearMessagesAsync(int count)
        {
            var channel = Context.Message.Channel as ITextChannel;
            var options = new RequestOptions()
            {
                AuditLogReason = "Clear command",
                RetryMode = RetryMode.AlwaysRetry,
                Timeout = 30000
            };

            var messages = await channel.GetMessagesAsync(count, options: options).FlattenAsync();

            var olderTwoWeeks = messages.Where(o => (DateTime.UtcNow - o.CreatedAt).TotalDays >= 14.0);
            var newerTwoWeeks = messages.Where(o => (DateTime.UtcNow - o.CreatedAt).TotalDays < 14.0);

            await channel.DeleteMessagesAsync(newerTwoWeeks, options);

            foreach (var oldMessage in olderTwoWeeks)
            {
                await oldMessage.DeleteMessageAsync(options);
            }

            MessageCache.TryBulkDelete(messages.Select(o => o.Id));
            await ReplyAndDeleteAsync($"Počet smazaných zpráv: {messages.Count()}", deleteOptions: options);
        }
    }
}
