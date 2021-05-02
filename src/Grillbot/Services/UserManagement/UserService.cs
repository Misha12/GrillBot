using Discord.WebSocket;
using Grillbot.Database;
using Grillbot.Database.Enums;
using Grillbot.Database.Enums.Includes;
using Grillbot.Extensions.Discord;
using Grillbot.Helpers;
using Grillbot.Models;
using Grillbot.Models.BotStatus;
using Grillbot.Models.Users;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Grillbot.Services.UserManagement
{
    public class UserService
    {
        private IGrillBotRepository GrillBotRepository { get; }
        private DiscordSocketClient DiscordClient { get; }
        private BotState BotState { get; }
        private SearchService SearchService { get; }

        public UserService(IGrillBotRepository grillBotRepository, DiscordSocketClient client, BotState botState,
            SearchService searchService)
        {
            GrillBotRepository = grillBotRepository;
            DiscordClient = client;
            BotState = botState;
            SearchService = searchService;
        }

        /// <summary>
        /// Gets user from API token. This method returns only partial data.
        /// </summary>
        public async Task<DiscordUser> GetUserAsync(string apiToken)
        {
            var entity = await GrillBotRepository.UsersRepository.FindUserByApiTokenAsync(apiToken);

            if (entity == null)
                return null;

            return await UserHelper.MapUserAsync(DiscordClient, BotState, entity);
        }

        /// <summary>
        /// Gets complete information about user
        /// </summary>
        public async Task<DiscordUser> GetUserAsync(SocketGuild guild, SocketUser user)
        {
            var userId = await SearchService.GetUserIDFromDiscordUserAsync(guild, user);

            if (userId == null)
                return null;

            return await GetUserAsync(userId.Value);
        }

        /// <summary>
        /// Gets complete information about user.
        /// </summary>
        public async Task<DiscordUser> GetUserAsync(long userId)
        {
            const UsersIncludes includes = UsersIncludes.Unverify | UsersIncludes.UsedInvite;

            var entity = await GrillBotRepository.UsersRepository.GetUserAsync(userId, includes, true);

            if (entity == null)
                return null;

            var channelsQuery = GrillBotRepository.ChannelStatsRepository.GetChannelsOfUser(entity.ID)
                .OrderByDescending(o => o.Count)
                .ThenByDescending(o => o.LastMessageAt)
                .AsNoTracking();
            entity.Channels = channelsQuery.ToHashSet();

            var incomingUnverifiesQuery = GrillBotRepository.UnverifyRepository.GetIncomingUnverifies(entity.ID)
                .Where(o => o.Operation == UnverifyLogOperation.Selfunverify || o.Operation == UnverifyLogOperation.Unverify)
                .OrderByDescending(o => o.ID)
                .AsNoTracking();
            entity.IncomingUnverifyOperations = incomingUnverifiesQuery.ToHashSet();

            var outgoingUnverifiesQuery = GrillBotRepository.UnverifyRepository.GetOutgoingUnverifies(entity.ID)
                .Where(o => o.Operation == UnverifyLogOperation.Unverify)
                .OrderByDescending(o => o.ID)
                .AsNoTracking();
            entity.OutgoingUnverifyOperations = outgoingUnverifiesQuery.ToHashSet();

            var createdInvitesQuery = GrillBotRepository.InviteRepository.GetInvitesOfUser(entity.ID)
                .OrderByDescending(o => o.UsedUsers.Count)
                .ThenByDescending(o => o.Code)
                .AsNoTracking();
            entity.CreatedInvites = createdInvitesQuery.ToHashSet();

            var remindersQuery = GrillBotRepository.ReminderRepository.GetRemindersOfUser(entity.ID)
                .OrderByDescending(o => o.At)
                .ThenByDescending(o => o.RemindID)
                .AsNoTracking();
            entity.Reminders = remindersQuery.ToHashSet();

            return await UserHelper.MapUserAsync(DiscordClient, BotState, entity);
        }

        public async Task ToggleBotAdminAsync(long userId)
        {
            var entity = await GrillBotRepository.UsersRepository.GetUserAsync(userId, UsersIncludes.None);
            entity.IsBotAdmin = !entity.IsBotAdmin;

            await GrillBotRepository.CommitAsync();
        }

        public async Task<bool> IsBotAdminAsync(SocketGuild guild, SocketUser user)
        {
            if (user.Id == BotState.AppInfo.Owner.Id) return true;

            var entity = await GrillBotRepository.UsersRepository.GetUserAsync(guild.Id, user.Id, UsersIncludes.None, true);
            return entity != null && entity.IsBotAdmin;
        }

        public async Task<PaginationInfo> GetPaginationInfo(WebAdminUserListFilter filter)
        {
            var guild = DiscordClient.GetGuild(filter.GuildID);

            if (guild == null)
                return new PaginationInfo();

            var users = await SearchService.FindUsersAsync(guild, filter.UserQuery);
            if (filter.IgnoreMissing && users == null)
                users = guild.Users.ToList();

            var userIdsData = await SearchService.ConvertUsersToIDsAsync(users);
            var userIds = userIdsData?.Where(o => o.Value != null).Select(o => o.Value.Value).ToList();
            var queryFilter = filter.CreateQueryFilter(guild);

            var totalCount = await GrillBotRepository.UsersRepository.GetUsersQuery(queryFilter, userIds, UsersIncludes.None)
                .CountAsync();

            if (filter.Page < 0)
                filter.Page = 0;

            var skip = (filter.Page == 0 ? 0 : filter.Page - 1) * PaginationInfo.DefaultPageSize;
            return new PaginationInfo(skip, filter.Page, totalCount);
        }

        public async Task<List<DiscordUser>> GetUsersList(WebAdminUserListFilter filter)
        {
            var guild = DiscordClient.GetGuild(filter.GuildID);

            if (guild == null)
                return new List<DiscordUser>();

            var users = await SearchService.FindUsersAsync(guild, filter.UserQuery);
            if (filter.IgnoreMissing && users == null)
                users = guild.Users.ToList();

            var userIdsData = await SearchService.ConvertUsersToIDsAsync(users);
            var userIds = userIdsData?.Where(o => o.Value != null).Select(o => o.Value.Value).ToList();
            var queryFilter = filter.CreateQueryFilter(guild);

            var dbUsers = await GrillBotRepository.UsersRepository.GetUsersQuery(queryFilter, userIds, UsersIncludes.None)
                .Skip((filter.Page == 0 ? 0 : filter.Page - 1) * PaginationInfo.DefaultPageSize).Take(PaginationInfo.DefaultPageSize)
                .ToListAsync();

            var result = new List<DiscordUser>();
            foreach (var user in dbUsers)
            {
                var mappedUser = await UserHelper.MapUserAsync(DiscordClient, BotState, user);
                if (mappedUser != null)
                    result.Add(mappedUser);
            }

            return result;
        }

        public async Task<List<WebStatItem>> GetWebStatsAsync()
        {
            var query = GrillBotRepository.UsersRepository.GetWebStatisticsQuery();
            var statistics = await query.ToListAsync();

            var result = new List<WebStatItem>();
            foreach (var entity in statistics)
            {
                var guild = DiscordClient.GetGuild(entity.GuildIdSnowflake);
                var user = await guild?.GetUserFromGuildAsync(entity.UserIdSnowflake);

                if (user == null)
                    continue;

                result.Add(new WebStatItem(guild, user, entity));
            }

            return result;
        }

        public async Task UnblockUserAsync(long id)
        {
            var entity = await GrillBotRepository.UsersRepository.GetUserAsync(id, UsersIncludes.None);

            if (entity == null || entity.WebAdminBannedTo == null)
                return;

            entity.WebAdminBannedTo = null;
            entity.FailedLoginCount = 0;

            await GrillBotRepository.CommitAsync();
        }
    }
}
