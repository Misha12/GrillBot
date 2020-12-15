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
using Grillbot.Models.Audit.DiscordAuditLog;
using Grillbot.Models.Embed;
using Grillbot.Services.BackgroundTasks;
using Grillbot.Services.Config;
using Grillbot.Services.MessageCache;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
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
            if (user == null)
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
                foreach (var attachment in message.Attachments)
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

            var botAccounts = filter.IgnoreBots ? await guild.GetBotsAsync() : new List<SocketGuildUser>();
            var botAccountIds = (await UserSearchService.ConvertUsersToIDsAsync(botAccounts)).Select(o => o.Value).Where(o => o != null).Select(o => (long)o);

            if (filter.Page < 0)
                filter.Page = 0;

            return new AuditLogQueryFilter()
            {
                From = filter.From,
                GuildId = filter.GuildId.ToString(),
                Skip = (filter.Page == 0 ? 0 : filter.Page - 1) * PaginationInfo.DefaultPageSize,
                SortDesc = filter.SortDesc,
                Take = PaginationInfo.DefaultPageSize,
                To = filter.To,
                Type = filter.Type,
                UserIds = userIds.ToList(),
                IgnoredIds = botAccountIds.ToList()
            };
        }

        public async Task<Database.Entity.File> GetFileAsync(string filename)
        {
            return await GrillBotRepository.AuditLogs.FindFileByFilenameAsync(filename);
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

            var logIdsQuery = GrillBotRepository.AuditLogs.GetLastAuditLogIdsQuery(guild.Id);
            var lastLogIds = (await logIdsQuery.ToListAsync()).ConvertAll(o => Convert.ToUInt64(o));

            foreach (var type in task.ActionTypes)
            {
                if (!AuditServiceHelper.IsTypeDefined(type))
                    continue;

                var logs = await guild.GetAuditLogDataAsync(100, type);

                if (logs.Count == 0)
                    continue;

                foreach (var log in logs)
                {
                    if (lastLogIds.Contains(log.Id))
                        continue;

                    var userId = await GetOrCreateUserId(guild, log.User);
                    var item = new AuditLogItem()
                    {
                        CreatedAt = log.CreatedAt.LocalDateTime,
                        DcAuditLogIdSnowflake = log.Id,
                        UserId = userId,
                        GuildIdSnowflake = guild.Id,
                        Type = AuditServiceHelper.AuditLogTypeMap[type]
                    };

                    var logMappingMethod = AuditServiceHelper.AuditLogDataMap[type];

                    if (logMappingMethod != null)
                    {
                        var mappedItem = logMappingMethod(log.Data);

                        if (mappedItem != null)
                            item.SetData(mappedItem);
                    }

                    await GrillBotRepository.AddAsync(item);
                }
            }

            await GrillBotRepository.CommitAsync();
        }

        public bool CanScheduleTask(DateTime lastScheduleAt)
        {
            return (DateTime.Now - lastScheduleAt).TotalMinutes >= 5.0D; // Every 5 minute
        }

        public List<BackgroundTask> GetBackgroundTasks()
        {
            return Client.Guilds.Select(o => (BackgroundTask)new DownloadAuditLogBackgroundTask(o)).ToList();
        }

        public async Task<Tuple<int, int>> ImportLogsAsync(List<IMessage> messages, SocketGuild guild, Func<int, Task> onChange)
        {
            int importedMessages = 0;

            foreach (var message in messages)
            {
                var entity = new AuditLogItem()
                {
                    CreatedAt = message.CreatedAt.LocalDateTime,
                    GuildIdSnowflake = guild.Id
                };

                var embed = message.Embeds.First();

                if (string.IsNullOrEmpty(embed.Title))
                    continue;

                switch (embed.Title)
                {
                    case var val when Regex.IsMatch(val, @".*Připojil\s*se\s*uživatel.*", RegexOptions.IgnoreCase):
                        {
                            entity.Type = AuditLogType.UserJoined;
                            entity.SetData(UserJoinedAuditData.Create(Convert.ToInt32(embed.Fields[2].Value)));

                            var user = await guild.GetUserFromGuildAsync(embed.Fields[0].Value);
                            if (user == null) continue;

                            entity.UserId = await GetOrCreateUserId(guild, user);
                        }
                        break;
                    case var val when Regex.IsMatch(val, @".*Zpráva\s*byla\s*upravena.*", RegexOptions.IgnoreCase):
                        {
                            entity.Type = AuditLogType.MessageEdited;

                            var auditData = MessageEditedAuditData.Create(embed.Fields);
                            if (auditData == null) continue;
                            entity.SetData(auditData);

                            var user = await guild.GetUserFromGuildAsync(embed.Fields[0].Value);
                            if (user == null) continue;

                            entity.UserId = await GetOrCreateUserId(guild, user);
                        }
                        break;
                    case var val when Regex.IsMatch(val, @".*Uživatel\s*opustil\s*server.*", RegexOptions.IgnoreCase):
                        {
                            entity.Type = AuditLogType.UserLeft;

                            var auditData = UserLeftAuditData.Create(embed.Fields);
                            if (auditData == null) continue;
                            entity.SetData(auditData);

                            entity.UserId = await GetOrCreateUserId(guild, Client.CurrentUser);
                        }
                        break;
                    case var val when Regex.IsMatch(val, @".*Zpráva\s*byla\s*odebrána.*", RegexOptions.IgnoreCase):
                        {
                            entity.Type = AuditLogType.MessageDeleted;

                            var auditData = MessageDeletedAuditData.Create(embed.Fields);
                            if (auditData == null) continue;
                            entity.SetData(auditData);

                            var user = (await guild.GetUserFromGuildAsync(embed.Fields[3].Value) ?? (IUser)await guild.GetUserFromGuildAsync(auditData.Author.Username, auditData.Author.Discriminator)) ?? Client.CurrentUser;

                            entity.UserId = await GetOrCreateUserId(guild, user);
                        }
                        break;
                    case var val when Regex.IsMatch(val, @".*Zpráva\s*nebyla\s*nalezena\s*v\s*cache.*", RegexOptions.IgnoreCase):
                        {
                            entity.Type = AuditLogType.MessageDeleted;

                            var auditData = MessageDeletedAuditData.Create(embed.Fields[0]);
                            if (auditData == null) continue;
                            entity.SetData(auditData);

                            entity.UserId = await GetOrCreateUserId(guild, Client.CurrentUser);
                        }
                        break;
                    case var val when Regex.IsMatch(val, @".*Uživatel\s*na\s*serveru\s*byl\s*aktualizován.*", RegexOptions.IgnoreCase):
                        {
                            entity.Type = AuditLogType.MemberUpdated;

                            var user = await guild.GetUserFromGuildAsync(embed.Fields[0].Value);
                            if (user == null) continue;

                            var auditData = AuditMemberUpdated.Create(embed.Fields, user);
                            if (auditData == null) continue;
                            entity.SetData(auditData);

                            entity.UserId = await GetOrCreateUserId(guild, Client.CurrentUser);
                        }
                        break;
                    default:
                        continue;
                }

                await GrillBotRepository.AddAsync(entity);

                importedMessages++;
                if (importedMessages % 5 == 0)
                    await onChange(importedMessages);
            }

            await GrillBotRepository.CommitAsync();
            return Tuple.Create(importedMessages, messages.Count);
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
    }
}
