using Discord.Commands;
using Grillbot.Database.Enums;
using Grillbot.Extensions;
using Grillbot.Extensions.Discord;
using Grillbot.Models.Embed;
using Grillbot.Models.Users;
using System.Linq;
using System.Threading.Tasks;

namespace Grillbot.Helpers
{
    public static class UserInfoHelper
    {
        public static async Task<BotEmbed> CreateSimpleEmbedAsync(DiscordUser user, SocketCommandContext context)
        {
            var roleWithColor = user.User.Roles.FindHighestRoleWithColor();
            var embed = new BotEmbed(context.User, roleWithColor?.Color, "Informace o uživateli", user.User.GetUserAvatarUrl());

            var joinedAt = user.User.JoinedAt?.LocalDateTime.ToLocaleDatetime();
            var joinPosition = await user.Guild.CalculateJoinPositionAsync(user.User);
            var selfUnverifies = user.UnverifyHistory.Where(o => o.Operation == UnverifyLogOperation.Selfunverify);

            embed
                .AddField("ID", $"{user.User.Id} ({user.ID})", true)
                .AddField("Jméno", user.User.GetFullName(), true)
                .AddField("Stav", user.User.Status.ToString(), true)
                .AddField("Založen", user.User.CreatedAt.LocalDateTime.ToLocaleDatetime(), true)
                .AddField("Připojen (Pořadí)", $"{joinedAt} ({joinPosition})", true);

            if (user.User.VoiceChannel != null)
                embed.AddField("Umlčen (Klient/Server)", $"{user.User.IsSelfMuted().TranslateToCz()}/{user.User.IsMuted().TranslateToCz()}", true);

            var roles = user.User.Roles.Where(o => !o.IsEveryone).OrderByDescending(o => o.Position).Select(o => o.Mention);
            embed
                .AddField("Role", !roles.Any() ? "Nejsou" : string.Join(", ", roles), false);

            if (user.User.PremiumSince != null)
                embed.AddField("Boost od", user.User.PremiumSince.Value.LocalDateTime.ToLocaleDatetime(), true);

            embed
                .AddField("Body", user.Points.FormatWithSpaces(), true)
                .AddField("Reakce (Rozdané/Získané)", user.FormatReactions(), true)
                .AddField("Počet zpráv", user.TotalMessageCount.FormatWithSpaces(), true)
                .AddField("Počet unverify (z toho self)", $"{user.UnverifyHistory.Count.FormatWithSpaces()} ({selfUnverifies.Count().FormatWithSpaces()})", true);

            if (user.UsedInvite != null)
            {
                if (user.UsedInvite.Code == context.Guild.VanityURLCode)
                {
                    embed.AddField("Použitá pozvánka", $"Vanity invite ({user.UsedInvite.Code})", false);
                }
                else
                {
                    var inviteCreator = user.UsedInvite.Creator?.GetFullName() ?? "Neznámý uživatel";
                    var createdAtDateTime = user.UsedInvite.CreatedAt?.LocalDateTime;
                    var createdAt = createdAtDateTime == null ? "Nevím kdy" : createdAtDateTime.Value.ToLocaleDatetime();

                    embed.AddField("Použitá pozvánka", $"Kód: **{user.UsedInvite.Code}**\nVytvořil: **{inviteCreator} ({createdAt})**", false);
                }
            }

            var clients = user.User.ActiveClients.Select(o => o.ToString());
            if (clients.Any())
                embed.AddField("Aktivní klienti", string.Join(", ", clients), false);

            return embed;
        }
    }
}
