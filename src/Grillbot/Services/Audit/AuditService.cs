using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Grillbot.Database;
using Grillbot.Database.Entity.AuditLog;
using Grillbot.Enums;
using Grillbot.Helpers;
using Grillbot.Models;
using Grillbot.Models.Audit;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Grillbot.Services.Audit
{
    public class AuditService
    {
        private IGrillBotRepository GrillBotRepository { get; }
        private UserSearchService UserSearchService { get; }
        private DiscordSocketClient Client { get; }
        private BotState BotState { get; }

        public AuditService(IGrillBotRepository grillBotRepository, UserSearchService userSearchService, DiscordSocketClient client, BotState botState)
        {
            GrillBotRepository = grillBotRepository;
            UserSearchService = userSearchService;
            Client = client;
            BotState = botState;
        }

        public async Task LogCommandAsync(Optional<CommandInfo> command, ICommandContext context)
        {
            if (context.Guild == null || !command.IsSpecified)
                return;

            var userId = await UserSearchService.GetUserIDFromDiscordUserAsync(context.Guild, context.User);

            var entity = new AuditLogItem()
            {
                Type = AuditLogType.Command,
                CreatedAt = DateTime.Now,
                GuildIdSnowflake = context.Guild.Id,
                UserId = userId,
                Data = JObject.FromObject(CommandAuditData.CreateDbItem(context, command.Value))
            };

            await GrillBotRepository.AddAsync(entity);
            await GrillBotRepository.CommitAsync();
        }

        public async Task LogUserLeftAsync(SocketGuildUser user)
        {
            if (user == null)
                return;

            var userId = await UserSearchService.GetUserIDFromDiscordUserAsync(user.Guild, user);

            var entity = new AuditLogItem()
            {
                Type = AuditLogType.UserLeft
            };
        }

        public async Task<List<AuditItem>> GetAuditLogsAsync(LogsFilter filter)
        {
            var guild = Client.GetGuild(filter.GuildId);

            if (guild == null)
                return new List<AuditItem>();

            var queryFilter = await CreateQueryFilterAsync(filter, guild);
            var data = await GrillBotRepository.AuditLogs.GetAuditLogsQuery(queryFilter)
                .Skip(queryFilter.Skip).Take(queryFilter.Take).ToListAsync();

            var items = new List<AuditItem>();
            foreach (var item in data)
            {
                var user = item.User == null ? null : await UserHelper.MapUserAsync(Client, BotState, item.User);
                items.Add(AuditItem.Create(guild, item, user));
            }

            return items;
        }

        public async Task<PaginationInfo> GetPaginationInfoAsync(LogsFilter filter)
        {
            var guild = Client.GetGuild(filter.GuildId);

            if (guild == null)
                return new PaginationInfo();

            var queryFilter = await CreateQueryFilterAsync(filter, guild);
            var totalCount = await GrillBotRepository.AuditLogs.GetAuditLogsQuery(queryFilter).CountAsync();

            return new PaginationInfo()
            {
                Page = filter.Page,
                CanNext = queryFilter.Skip + PaginationInfo.DefaultPageSize < totalCount,
                CanPrev = queryFilter.Skip != 0,
                PagesCount = (int)System.Math.Ceiling(totalCount / (double)PaginationInfo.DefaultPageSize)
            };
        }

        private async Task<AuditLogQueryFilter> CreateQueryFilterAsync(LogsFilter filter, SocketGuild guild)
        {
            var users = await UserSearchService.FindUsersAsync(guild, filter.UserQuery);
            var userIds = (await UserSearchService.ConvertUsersToIDsAsync(users)).Select(o => o.Value).Where(o => o != null).Select(o => (long)o);

            if (filter.Page < 0)
                filter.Page = 0;

            return new AuditLogQueryFilter()
            {
                From = filter.From,
                GuildId = filter.GuildId.ToString(),
                IncludeAnonymous = filter.IncludeAnonymous,
                Order = filter.Order,
                Skip = (filter.Page == 0 ? 0 : filter.Page - 1) * PaginationInfo.DefaultPageSize,
                SortDesc = filter.SortDesc,
                Take = PaginationInfo.DefaultPageSize,
                To = filter.To,
                Type = filter.Type,
                UserIds = userIds.ToList()
            };
        }
    }
}
