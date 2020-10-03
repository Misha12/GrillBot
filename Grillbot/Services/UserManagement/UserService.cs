using Discord.WebSocket;
using DBUserChannel = Grillbot.Database.Entity.Users.UserChannel;
using Grillbot.Database.Repository;
using Grillbot.Models.Users;
using Grillbot.Services.MessageCache;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Grillbot.Helpers;
using Grillbot.Database.Enums.Includes;
using Microsoft.EntityFrameworkCore;
using Grillbot.Database.Enums;

namespace Grillbot.Services.UserManagement
{
    public partial class UserService
    {
        private IServiceProvider Services { get; }
        public Dictionary<string, DateTime> LastPointsCalculatedAt { get; set; }
        private static readonly object locker = new object();
        private IMessageCache MessageCache { get; }
        private DiscordSocketClient DiscordClient { get; }
        private UserSearchService UserSearchService { get; }
        private BotState BotState { get; }

        public UserService(IServiceProvider services, IMessageCache messageCache, DiscordSocketClient discordClient, UserSearchService userSearchService,
            BotState botState)
        {
            Services = services;
            MessageCache = messageCache;
            LastPointsCalculatedAt = new Dictionary<string, DateTime>();
            DiscordClient = discordClient;
            UserSearchService = userSearchService;
            BotState = botState;
        }

        public async Task<List<DiscordUser>> GetUsersList(WebAdminUserListFilter filter)
        {
            var users = new List<DiscordUser>();

            using var scope = Services.CreateScope();
            using var repository = scope.ServiceProvider.GetService<UsersRepository>();

            var guild = DiscordClient.GetGuild(filter.GuildID);

            if (guild == null)
                return users;

            var discordUsers = await UserSearchService.FindUsersAsync(guild, filter.UserQuery);
            var userIds = discordUsers.Select(o => o.Id).ToList();

            const int pageSize = 25;
            var skip = (filter.Page == 0 ? 0 : filter.Page - 1) * pageSize;

            var dbUsers = await repository.GetUsers(filter.Order, filter.SortDesc, filter.GuildID, userIds, filter.UsedInviteCode, skip, pageSize).ToListAsync();

            foreach (var user in dbUsers)
            {
                var mappedUser = await UserHelper.MapUserAsync(DiscordClient, BotState, user);
                if (mappedUser != null)
                    users.Add(mappedUser);
            }

            return users;
        }

        public async Task<DiscordUser> GetUserInfoAsync(SocketGuild guild, SocketUser user, bool full = false)
        {
            using var scope = Services.CreateScope();
            using var repository = scope.ServiceProvider.GetService<UsersRepository>();
            using var inviteRepository = scope.ServiceProvider.GetService<InviteRepository>();

            var userID = await repository.FindUserIDFromDiscordIDAsync(guild.Id, user.Id);

            if (userID == null)
                return null;

            var includes = UsersIncludes.Channels | UsersIncludes.UnverifyLogIncoming;

            if (full)
                includes |= UsersIncludes.Birthday | UsersIncludes.Statistics;

            var entity = await repository.GetUserAsync(userID.Value, includes);

            if (!string.IsNullOrEmpty(entity.UsedInviteCode))
                entity.UsedInvite = await inviteRepository.FindInviteAsync(entity.UsedInviteCode);

            return await UserHelper.MapUserAsync(DiscordClient, BotState, entity);
        }

        public async Task<DiscordUser> GetUserInfoAsync(long userID, bool full = false)
        {
            using var scope = Services.CreateScope();
            using var repository = scope.ServiceProvider.GetService<UsersRepository>();
            using var inviteRepository = scope.ServiceProvider.GetService<InviteRepository>();

            var includes = UsersIncludes.Channels | UsersIncludes.UnverifyLogIncoming;

            if (full)
                includes |= UsersIncludes.Birthday | UsersIncludes.Statistics;

            var entity = await repository.GetUserAsync(userID, includes);

            if (!string.IsNullOrEmpty(entity.UsedInviteCode))
                entity.UsedInvite = await inviteRepository.FindInviteAsync(entity.UsedInviteCode);

            return await UserHelper.MapUserAsync(DiscordClient, BotState, entity);
        }

        public void IncrementMessage(SocketGuildUser guildUser, SocketGuild guild, SocketGuildChannel channel)
        {
            lock (locker)
            {
                using var scope = Services.CreateScope();
                using var repository = scope.ServiceProvider.GetService<UsersRepository>();
                var user = repository.GetOrCreateUser(guild.Id, guildUser.Id, UsersIncludes.Channels);
                var channelEntity = user.Channels.FirstOrDefault(o => o.ChannelIDSnowflake == channel.Id);

                if (channelEntity == null)
                {
                    channelEntity = new DBUserChannel()
                    {
                        ChannelIDSnowflake = channel.Id,
                        Count = 1,
                        LastMessageAt = DateTime.Now
                    };

                    user.Channels.Add(channelEntity);
                }
                else
                {
                    channelEntity.Count++;
                    channelEntity.LastMessageAt = DateTime.Now;
                }

                repository.SaveChanges();
            }
        }

        public void DecrementMessage(SocketGuildUser guildUser, SocketGuild guild, SocketGuildChannel channel)
        {
            lock (locker)
            {
                using var scope = Services.CreateScope();
                using var repository = scope.ServiceProvider.GetService<UsersRepository>();
                var user = repository.GetUser(guild.Id, guildUser.Id, UsersIncludes.Channels);

                if (user == null)
                    return;

                var channelEntity = user.Channels.FirstOrDefault(o => o.ChannelIDSnowflake == channel.Id);
                if (channelEntity == null || channelEntity.Count == 0)
                    return;

                channelEntity.Count--;
                repository.SaveChanges();
            }
        }

        public void IncrementReaction(SocketReaction reaction)
        {
            ProcessReaction(reaction, IncrementReaction);
        }

        public void DecrementReaction(SocketReaction reaction)
        {
            ProcessReaction(reaction, DecrementReaction);
        }

        private void ProcessReaction(SocketReaction reaction, Action<SocketGuildUser, SocketGuildUser> action)
        {
            if (!reaction.User.IsSpecified || !(reaction.User.Value is SocketGuildUser reactingUser))
                return;

            if (reaction.Message.IsSpecified && reaction.Message.Value.Author is SocketGuildUser rawMsgAuthor)
            {
                action(rawMsgAuthor, reactingUser);
                return;
            }

            var message = MessageCache.Get(reaction.MessageId);
            if (message != null && message.Author is SocketGuildUser cachedMsgAuthor)
            {
                action(cachedMsgAuthor, reactingUser);
            }
        }

        private void IncrementReaction(SocketGuildUser author, SocketGuildUser reactingUser)
        {
            if (author.Id == reactingUser.Id)
                return; // Author add reaction to his message.

            lock (locker)
            {
                using var scope = Services.CreateScope();
                using var repository = scope.ServiceProvider.GetService<UsersRepository>();

                var authorEntity = repository.GetOrCreateUser(author.Guild.Id, author.Id, UsersIncludes.None);
                var reactingUserEntity = repository.GetOrCreateUser(reactingUser.Guild.Id, reactingUser.Id, UsersIncludes.None);

                authorEntity.ObtainedReactionsCount++;
                reactingUserEntity.GivenReactionsCount++;

                repository.SaveChanges();
            }
        }

        private void DecrementReaction(SocketGuildUser author, SocketGuildUser reactingUser)
        {
            if (author.Id == reactingUser.Id)
                return; // Author add reaction to his message.

            lock (locker)
            {
                using var scope = Services.CreateScope();
                using var repository = scope.ServiceProvider.GetService<UsersRepository>();

                var authorEntity = repository.GetUser(author.Guild.Id, author.Id, UsersIncludes.None);
                var reactingUserEntity = repository.GetUser(reactingUser.Guild.Id, reactingUser.Id, UsersIncludes.None);

                if (authorEntity != null && authorEntity.ObtainedReactionsCount > 0)
                    authorEntity.ObtainedReactionsCount--;

                if (reactingUserEntity != null && reactingUserEntity.GivenReactionsCount > 0)
                    reactingUserEntity.GivenReactionsCount--;

                repository.SaveChanges();
            }
        }

        public async Task SetAdminAsync(SocketGuild guild, SocketGuildUser user, bool isAdmin)
        {
            using var scope = Services.CreateScope();
            using var repository = scope.ServiceProvider.GetService<UsersRepository>();

            var entity = await repository.GetOrCreateUserAsync(guild.Id, user.Id, UsersIncludes.None);

            if (isAdmin)
                entity.Flags |= (long)UserFlags.BotAdmin;
            else
                entity.Flags &= ~(long)UserFlags.BotAdmin;

            await repository.SaveChangesAsync();
        }
    }
}
