using Discord.Rest;
using Discord.WebSocket;
using Grillbot.Database.Enums;
using Grillbot.Extensions;
using Grillbot.Extensions.Discord;
using Grillbot.Models.Channelboard;
using Grillbot.Models.Reminder;
using Grillbot.Models.Unverify;
using Grillbot.Services.InviteTracker;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DBDiscordUser = Grillbot.Database.Entity.Users.DiscordUser;

namespace Grillbot.Models.Users
{
    public class DiscordUser
    {
        public long ID { get; set; }
        public ulong DiscordId { get; set; }
        public SocketGuild Guild { get; set; }
        public SocketGuildUser User { get; set; }
        public long Points { get; set; }
        public long GivenReactionsCount { get; set; }
        public long ObtainedReactionsCount { get; set; }
        public bool WebAdminAccess { get; set; }
        public bool ApiAccess { get; set; }
        public List<ChannelStatItem> Channels { get; set; }
        public DateTime? Birthday { get; set; }
        public long TotalMessageCount => Channels.Sum(o => o.Count);
        public int? WebAdminLoginCount { get; set; }
        public InviteModel UsedInvite { get; set; }
        public List<UnverifyLogItem> UnverifyHistory { get; set; }
        public List<UnverifyLogItem> GivenUnverifyHistory { get; set; }
        public long Flags { get; set; }
        public DateTime? UnverifyEndsAt { get; set; }
        public List<RemindItem> Reminders { get; set; }
        public List<InviteModel> CreatedInvites { get; set; }
        public string UnverifyImunityGroup { get; set; }
        public DateTime? WebAdminBannedTo { get; set; }

        #region ReadOnlyProperties

        public bool IsBotAdmin => (Flags & (long)UserFlags.BotAdmin) != 0;
        public bool IsWebAdminBanned => WebAdminBannedTo >= DateTime.Now;

        #endregion

        public static async Task<DiscordUser> CreateAsync(SocketGuild guild, SocketGuildUser user, DBDiscordUser dbUser, DiscordSocketClient discordClient,
            RestApplication botAppInfo)
        {
            var result = new DiscordUser()
            {
                ApiAccess = !string.IsNullOrEmpty(dbUser.ApiToken),
                Guild = guild,
                User = user,
                ID = dbUser.ID,
                Points = dbUser.Points,
                GivenReactionsCount = dbUser.GivenReactionsCount,
                ObtainedReactionsCount = dbUser.ObtainedReactionsCount,
                WebAdminAccess = !string.IsNullOrEmpty(dbUser.WebAdminPassword),
                Birthday = dbUser.Birthday,
                WebAdminLoginCount = dbUser.WebAdminLoginCount,
                Flags = dbUser.Flags,
                UnverifyEndsAt = dbUser.Unverify?.EndDateTime,
                UnverifyImunityGroup = dbUser.UnverifyImunityGroup,
                DiscordId = dbUser.UserIDSnowflake,
                WebAdminBannedTo = dbUser.WebAdminBannedTo
            };

            if (user != null && botAppInfo.Owner.Id == user.Id)
                result.Flags |= (long)UserFlags.BotAdmin;

            result.Channels = dbUser.Channels
                .Select(o => new ChannelStatItem(guild.GetChannel(o.ChannelIDSnowflake), o))
                .Where(o => o.Channel != null)
                .ToList();

            if (dbUser.UsedInvite != null)
            {
                SocketGuildUser inviteCreator = null;
                if (dbUser.UsedInvite.Creator != null)
                    inviteCreator = await guild.GetUserFromGuildAsync(dbUser.UsedInvite.Creator.UserIDSnowflake);

                result.UsedInvite = new InviteModel(dbUser.UsedInvite, inviteCreator, dbUser.UsedInviteCode == guild.VanityURLCode);
            }
            else if(!string.IsNullOrEmpty(dbUser.UsedInviteCode))
            {
                result.UsedInvite = new InviteModel(dbUser.UsedInviteCode);
            }

            result.UnverifyHistory = dbUser.IncomingUnverifyOperations
                .Select(o => new UnverifyLogItem(o, discordClient))
                .ToList();

            result.GivenUnverifyHistory = dbUser.OutgoingUnverifyOperations
                .Select(o => new UnverifyLogItem(o, discordClient))
                .ToList();

            result.Reminders = new List<RemindItem>();
            foreach(var remind in dbUser.Reminders)
            {
                var fromUser = remind.FromUser != null ? await guild.GetUserFromGuildAsync(remind.FromUser.UserIDSnowflake) : null;
                result.Reminders.Add(new RemindItem(remind, fromUser));
            }

            result.CreatedInvites = dbUser.CreatedInvites.Select(o => new InviteModel(o, user, false, o.UsedUsers.Count)).ToList();

            return result;
        }

        public string FormatReactions()
        {
            var given = GivenReactionsCount.FormatWithSpaces();
            var obtained = ObtainedReactionsCount.FormatWithSpaces();

            return $"{given} / {obtained}";
        }

        public ChannelStatItem GetMostActiveChannel()
        {
            return Channels.FirstOrDefault();
        }

        public ChannelStatItem GetLastActiveChannel()
        {
            return Channels
                .OrderByDescending(o => o.LastMessageAt)
                .FirstOrDefault();
        }

        public List<string> GetDetailFlags()
        {
            var flags = new List<string>();

            if (WebAdminAccess)
                flags.Add("WebAdminAccess");

            if (ApiAccess)
                flags.Add("ApiAccess");

            if (User.IsGuildOwner())
                flags.Add("GuildOwner");

            if (Birthday != null)
                flags.Add("Birthday");

            if (IsBotAdmin)
                flags.Add("BotAdmin");

            return flags;
        }
    }
}
