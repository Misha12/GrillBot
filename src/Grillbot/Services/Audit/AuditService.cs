using Discord;
using Discord.Commands;
using Discord.Rest;
using Discord.WebSocket;
using Grillbot.Database;
using Grillbot.Database.Entity.AuditLog;
using Grillbot.Database.Enums.Includes;
using Grillbot.Enums;
using Grillbot.Extensions.Discord;
using Grillbot.Helpers;
using Grillbot.Models;
using Grillbot.Models.Audit;
using Grillbot.Models.Embed;
using Grillbot.Services.BackgroundTasks;
using Grillbot.Services.Config;
using Grillbot.Services.MessageCache;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Grillbot.Services.Audit
{
    public class AuditService : IBackgroundTaskObserver, IBackgroundTaskScheduleable
    {
        private IGrillBotRepository GrillBotRepository { get; }
        private UserSearchService UserSearchService { get; }
        private DiscordSocketClient Client { get; }
        private BotState BotState { get; }
        private IMessageCache MessageCache { get; }
        private ConfigurationService ConfigurationService { get; }

        public AuditService(IGrillBotRepository grillBotRepository, UserSearchService userSearchService, DiscordSocketClient client, BotState botState,
            IMessageCache messageCache, ConfigurationService configurationService)
        {
            GrillBotRepository = grillBotRepository;
            UserSearchService = userSearchService;
            Client = client;
            BotState = botState;
            MessageCache = messageCache;
            ConfigurationService = configurationService;
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
                UserId = userId
            };

            entity.SetData(CommandAuditData.CreateDbItem(context, command.Value));
            await GrillBotRepository.AddAsync(entity);
            await GrillBotRepository.CommitAsync();
        }

        public async Task LogUserLeftAsync(SocketGuildUser user)
        {
            if (user == null || user.Id == Client.CurrentUser.Id) // User is unknown or self.
                return;

            var ban = await user.Guild.FindBanAsync(user);
            RestAuditLogEntry dcAuditLogItem;

            if (ban != null)
            {
                dcAuditLogItem = (await user.Guild.GetAuditLogDataAsync(actionType: ActionType.Ban))?
                    .FirstOrDefault(o => (o.Data as BanAuditLogData)?.Target.Id == user.Id);
            }
            else
            {
                dcAuditLogItem = (await user.Guild.GetAuditLogDataAsync(actionType: ActionType.Kick))?
                    .FirstOrDefault(o => (o.Data as KickAuditLogData)?.Target.Id == user.Id);
            }

            long? executor = null;
            if (dcAuditLogItem != null)
                executor = await UserSearchService.GetUserIDFromDiscordUserAsync(user.Guild, dcAuditLogItem.User);

            var entity = new AuditLogItem()
            {
                Type = AuditLogType.UserLeft,
                CreatedAt = DateTime.Now,
                GuildIdSnowflake = user.Guild.Id,
                UserId = executor
            };

            entity.SetData(UserLeftAuditData.Create(user.Guild, user, ban != null, ban?.Reason));
            await GrillBotRepository.AddAsync(entity);
            await GrillBotRepository.CommitAsync();
        }

        public async Task LogUserJoinAsync(SocketGuildUser user)
        {
            if (user == null || !user.IsUser())
                return;

            var userEntity = await GrillBotRepository.UsersRepository.GetOrCreateUserAsync(user.Guild.Id, user.Id, UsersIncludes.None);
            await GrillBotRepository.CommitAsync();

            var entity = new AuditLogItem()
            {
                Type = AuditLogType.UserJoined,
                CreatedAt = DateTime.Now,
                GuildIdSnowflake = user.Guild.Id,
                UserId = userEntity.ID
            };

            entity.SetData(UserJoinedAuditData.Create(user.Guild));
            await GrillBotRepository.AddAsync(entity);
            await GrillBotRepository.CommitAsync();
        }

        public async Task LogMessageEditedAsync(Cacheable<IMessage, ulong> before, SocketMessage after, ISocketMessageChannel channel, SocketGuild guild)
        {
            var oldMessage = before.HasValue ? before.Value : MessageCache.Get(before.Id);
            if (!IsMessageEdited(oldMessage, after)) return;

            var userId = await UserSearchService.GetUserIDFromDiscordUserAsync(guild, after.Author);

            var entity = new AuditLogItem()
            {
                Type = AuditLogType.MessageEdited,
                CreatedAt = DateTime.Now,
                GuildIdSnowflake = guild.Id,
                UserId = userId
            };

            entity.SetData(MessageEditedAuditData.Create(channel, oldMessage, after));
            await GrillBotRepository.AddAsync(entity);
            await GrillBotRepository.CommitAsync();

            MessageCache.Update(after);
        }

        private bool IsMessageEdited(IMessage before, IMessage after)
        {
            return before != null && after != null && before.Author.IsUser() && before.Content != after.Content;
        }

        public async Task LogMessageDeletedAsync(Cacheable<IMessage, ulong> message, ISocketMessageChannel channel, SocketGuild guild)
        {
            var deletedMessage = message.HasValue ? message.Value : MessageCache.TryRemove(message.Id);

            var entity = new AuditLogItem()
            {
                Type = AuditLogType.MessageDeleted,
                CreatedAt = DateTime.Now,
                GuildIdSnowflake = guild.Id,
            };

            if (deletedMessage == null)
                entity.SetData(MessageDeletedAuditData.Create(channel));
            else
                await ProcessMessageDeletedWithCacheAsync(entity, channel, deletedMessage, guild);

            if (MessageCache.Exists(message.Id))
                MessageCache.TryRemove(message.Id);

            await MessageCache.AppendAroundAsync(channel, message.Id, 100);
            await GrillBotRepository.AddAsync(entity);
            await GrillBotRepository.CommitAsync();
        }

        private async Task ProcessMessageDeletedWithCacheAsync(AuditLogItem entity, ISocketMessageChannel channel, IMessage message, SocketGuild guild)
        {
            entity.SetData(MessageDeletedAuditData.Create(channel, message));

            var auditLog = (await guild.GetAuditLogDataAsync(actionType: ActionType.MessageDeleted)).Find(o =>
            {
                var data = (MessageDeleteAuditLogData)o.Data;
                return data.Target.Id == message.Author.Id && data.ChannelId == channel.Id;
            });

            entity.UserId = await UserSearchService.GetUserIDFromDiscordUserAsync(guild, auditLog?.User ?? message.Author);

            if (message.Attachments.Count > 0)
            {
                foreach (var attachment in message.Attachments.Where(o => o.Size < 10 * 1024 * 1024)) // Max 10MB
                {
                    var fileContent = await attachment.DownloadFileAsync();

                    if (fileContent == null)
                        continue;

                    entity.Files.Add(new Database.Entity.File()
                    {
                        Content = fileContent,
                        Filename = $"{Path.GetFileNameWithoutExtension(attachment.Filename)}_{attachment.Id}{Path.GetExtension(attachment.Filename)}"
                    });
                }
            }
        }

        public async Task ProcessBoostChangeAsync(SocketGuildUser before, SocketGuildUser after)
        {
            var boosterRoleId = ConfigurationService.GetValue(GlobalConfigItems.ServerBoosterRoleId);

            if (string.IsNullOrEmpty(boosterRoleId) || after.Roles.SequenceEqual(before.Roles))
                return;

            var boosterRoleIdValue = Convert.ToUInt64(boosterRoleId);
            var hasBefore = before.Roles.Any(o => o.Id == boosterRoleIdValue);
            var hasAfter = after.Roles.Any(o => o.Id == boosterRoleIdValue);

            if (!hasBefore && hasAfter)
                await NotifyBoostChangeAsync(after, "Uživatel na serveru je nyní Server Booster.");
            else if (hasBefore && !hasAfter)
                await NotifyBoostChangeAsync(after, "Uživatel na serveru již není Server Booster.");
        }

        private async Task NotifyBoostChangeAsync(SocketGuildUser user, string message)
        {
            var adminChannelId = ConfigurationService.GetValue(GlobalConfigItems.AdminChannel);
            if (string.IsNullOrEmpty(adminChannelId))
                return;

            var channel = Client.GetChannel(Convert.ToUInt64(adminChannelId));

            if (channel == null)
                return;

            var embed = new BotEmbed(title: message, color: new Color(255, 0, 207))
                .AddField("Uživatel", user?.ToString() ?? "Neznámý", false)
                .WithThumbnail(user?.GetUserAvatarUrl())
                .WithFooter($"UserId: {user?.Id ?? 0}", null);

            await (channel as ISocketMessageChannel)?.SendMessageAsync(embed: embed.Build());
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
                var auditItem = await AuditItem.CreateAsync(guild, item, user, MessageCache);

                items.Add(auditItem);
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

            return new PaginationInfo(queryFilter.Skip, filter.Page, totalCount);
        }

        private async Task<AuditLogQueryFilter> CreateQueryFilterAsync(LogsFilter filter, SocketGuild guild)
        {
            var users = await UserSearchService.FindUsersAsync(guild, filter.UserQuery);
            var userIds = users != null ? (await UserSearchService.ConvertUsersToIDsAsync(users)).Select(o => o.Value).Where(o => o != null).Select(o => (long)o) : null;

            var botAccounts = filter.IgnoreBots ? await guild.GetBotsAsync() : new List<SocketGuildUser>();
            var botAccountIds = (await UserSearchService.ConvertUsersToIDsAsync(botAccounts)).Select(o => o.Value).Where(o => o != null).Select(o => (long)o);

            if (filter.Page < 0)
                filter.Page = 0;

            var types = filter.GetSelectedTypes();

            return new AuditLogQueryFilter()
            {
                From = filter.From,
                GuildId = filter.GuildId.ToString(),
                Skip = (filter.Page == 0 ? 0 : filter.Page - 1) * PaginationInfo.DefaultPageSize,
                SortDesc = filter.SortDesc,
                Take = PaginationInfo.DefaultPageSize,
                To = filter.To,
                Types = types.ToArray(),
                UserIds = userIds?.ToList(),
                IgnoredIds = botAccountIds.ToList()
            };
        }

        public async Task DeleteItemAsync(long id)
        {
            var item = await GrillBotRepository.AuditLogs.FindItemByIdAsync(id);

            if (item == null)
                return;

            if (item.Files.Count > 0)
                GrillBotRepository.RemoveCollection(item.Files);

            GrillBotRepository.Remove(item);
            await GrillBotRepository.CommitAsync();
        }

        public async Task TriggerBackgroundTaskAsync(object data)
        {
            if (data is not DownloadAuditLogBackgroundTask task)
                return;

            var guild = Client.GetGuild(task.GuildId);

            if (guild == null)
                return;

            if (!AuditServiceHelper.IsTypeDefined(task.ActionType))
                return;

            var logs = await guild.GetAuditLogDataAsync(100, task.ActionType);
            if (logs.Count == 0)
                return;

            var auditLogType = AuditServiceHelper.AuditLogTypeMap[task.ActionType];
            var logIds = (await GrillBotRepository.AuditLogs.GetLastAuditLogIdsQuery(guild.Id, auditLogType).ToListAsync())
                .ConvertAll(o => Convert.ToUInt64(o));

            foreach (var log in logs)
            {
                if (logIds.Contains(log.Id))
                    continue;

                var userId = await GetOrCreateUserId(guild, log.User);
                var item = new AuditLogItem()
                {
                    CreatedAt = log.CreatedAt.LocalDateTime,
                    DcAuditLogIdSnowflake = log.Id,
                    UserId = userId,
                    GuildIdSnowflake = guild.Id,
                    Type = auditLogType
                };

                var logMappingMethod = AuditServiceHelper.AuditLogDataMap[task.ActionType];

                if (logMappingMethod != null)
                {
                    var mappedItem = logMappingMethod(log.Data);

                    if (mappedItem != null)
                        item.SetData(mappedItem);
                }

                await GrillBotRepository.AddAsync(item);
            }

            await GrillBotRepository.CommitAsync();
        }

        public bool CanScheduleTask(DateTime lastScheduleAt)
        {
            return (DateTime.Now - lastScheduleAt).TotalMinutes >= 10.0D; // Every 10 minutes
        }

        public List<BackgroundTask> GetBackgroundTasks()
        {
            var types = new[]
            {
                ActionType.GuildUpdated,
                ActionType.EmojiCreated,
                ActionType.EmojiDeleted,
                ActionType.EmojiUpdated,
                ActionType.OverwriteCreated,
                ActionType.OverwriteDeleted,
                ActionType.OverwriteUpdated,
                ActionType.Prune,
                ActionType.MemberUpdated,
                ActionType.MemberRoleUpdated,
                ActionType.RoleCreated,
                ActionType.RoleDeleted,
                ActionType.RoleUpdated,
                ActionType.WebhookCreated,
                ActionType.WebhookDeleted,
                ActionType.WebhookUpdated,
                ActionType.MessagePinned,
                ActionType.MessageUnpinned
            };

            return Client.Guilds.SelectMany(g => types.Select(type => (BackgroundTask)new DownloadAuditLogBackgroundTask(g, type))).ToList();
        }

        public async Task<int> ClearOldDataAsync(DateTime before, SocketGuild guild)
        {
            var oldData = await GrillBotRepository.AuditLogs.GetAuditLogsBeforeDate(before, guild.Id).ToListAsync();

            foreach (var item in oldData.Where(o => o.Files.Count > 0))
            {
                GrillBotRepository.RemoveCollection(item.Files);
            }

            GrillBotRepository.RemoveCollection(oldData);
            await GrillBotRepository.CommitAsync();

            return oldData.Count;
        }

        private async Task<long> GetOrCreateUserId(SocketGuild guild, IUser user)
        {
            var userId = await UserSearchService.GetUserIDFromDiscordUserAsync(guild, user);

            if (userId != null)
                return userId.Value;

            var entity = await GrillBotRepository.UsersRepository.CreateAndGetUserAsync(guild.Id, user.Id);
            return entity.ID;
        }

        public async Task<Dictionary<AuditLogType, int>> GetStatisticsPerType()
        {
            var stats = await GrillBotRepository.AuditLogs.GetStatsPerTypeQuery().ToListAsync();
            return stats.ToDictionary(o => o.Item1, o => o.Item2);
        }
    }
}
