using Discord.Commands;
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
            var roleNames = user.User.Roles.Where(o => !o.IsEveryone).OrderByDescending(o => o.Position).Select(o => o.Name);

            var embed = new BotEmbed(context.User, roleWithColor?.Color, "Informace o uživateli", user.User.GetUserAvatarUrl());

            var joinedAt = user.User.JoinedAt?.LocalDateTime.ToLocaleDatetime();
            var joinPosition = await user.Guild.CalculateJoinPositionAsync(user.User);
            var selfUnverifies = user.UnverifyHistory.Where(o => o.IsSelfUnverify);

            embed
                .AddField("ID", user.User.Id.ToString(), true)
                .AddField("Jméno", user.User.GetFullName(), true)
                .AddField("Stav", user.User.Status.ToString(), true)
                .AddField("Založen", user.User.CreatedAt.LocalDateTime.ToLocaleDatetime(), true)
                .AddField("Připojen (Pořadí)", $"{joinedAt} ({joinPosition})", true)
                .AddField("Umlčen (Klient/Server)", $"{user.User.IsMuted().TranslateToCz()}/{user.User.IsSelfMuted().TranslateToCz()}", true)
                .AddField("Role", string.Join(", ", roleNames), false);

            if (user.User.PremiumSince != null)
                embed.AddField("Boost od", user.User.PremiumSince.Value.LocalDateTime.ToLocaleDatetime(), true);

            embed
                .AddField("Body", user.Points.FormatWithSpaces(), true)
                .AddField("Reakce (Rozdané/Získané)", user.FormatReactions(), true)
                .AddField("Počet zpráv", user.TotalMessageCount.FormatWithSpaces(), true)
                .AddField("Počet unverify (z toho self)", $"{user.UnverifyHistory.Count.FormatWithSpaces()} ({selfUnverifies.Count().FormatWithSpaces()})", true);

            return embed;
        }
    }
}
