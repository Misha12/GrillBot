using Discord.WebSocket;
using DBUserChannel = Grillbot.Database.Entity.Users.UserChannel;
using DBDiscordUser = Grillbot.Database.Entity.Users.DiscordUser;
using Grillbot.Database.Repository;
using Grillbot.Extensions.Discord;
using Grillbot.Helpers;
using Grillbot.Models.Users;
using Grillbot.Services.Initiable;
using Grillbot.Services.MessageCache;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Discord;

namespace Grillbot.Services.UserManagement
{
    public class UserService : IDisposable, IInitiable
    {
        private ILogger<UserService> Logger { get; }
        private IServiceProvider Services { get; }
        public Dictionary<string, DBDiscordUser> Users { get; private set; }
        private HashSet<string> Changes { get; set; }
        public Dictionary<string, DateTime> LastPointsCalculatedAt { get; set; }
        private static readonly object locker = new object();
        private Timer SyncTimer { get; set; }
        private Random Random { get; }
        private IMessageCache MessageCache { get; }
        private DiscordSocketClient DiscordClient { get; }

        public UserService(ILogger<UserService> logger, IServiceProvider services, IMessageCache messageCache, DiscordSocketClient discordClient)
        {
            Logger = logger;
            Services = services;
            MessageCache = messageCache;
            Users = new Dictionary<string, DBDiscordUser>();
            Changes = new HashSet<string>();
            LastPointsCalculatedAt = new Dictionary<string, DateTime>();
            Random = new Random();
            DiscordClient = discordClient;
        }

        public async Task<List<DiscordUser>> GetUsersList(WebAdminUserOrder order, bool desc)
        {
            var users = new List<DiscordUser>();

            foreach (var user in Users.Values)
            {
                var guild = DiscordClient.GetGuild(user.GuildIDSnowflake);
                if (guild == null) continue;

                var socketUser = await guild.GetUserFromGuildAsync(user.UserIDSnowflake);
                if (socketUser == null) continue;

                users.Add(new DiscordUser(guild, socketUser, user));
            }

            switch (order)
            {
                case WebAdminUserOrder.MessageCount:
                    users = (desc ? users.OrderByDescending(o => o.TotalMessageCount) : users.OrderBy(o => o.TotalMessageCount)).ToList();
                    break;
                case WebAdminUserOrder.Points:
                    users = (desc ? users.OrderByDescending(o => o.Points) : users.OrderBy(o => o.Points)).ToList();
                    break;
                case WebAdminUserOrder.Reactions:
                    if (desc)
                    {
                        users = users
                            .OrderByDescending(o => o.GivenReactionsCount)
                            .ThenByDescending(o => o.ObtainedReactionsCount)
                            .ToList();
                    }
                    else
                    {
                        users = users
                            .OrderBy(o => o.GivenReactionsCount)
                            .ThenBy(o => o.ObtainedReactionsCount)
                            .ToList();
                    }
                    break;
                case WebAdminUserOrder.Server:
                    users = (desc ? users.OrderByDescending(o => o.Guild.Id) : users.OrderBy(o => o.Guild.Id)).ToList();
                    break;
                default:
                    users = (desc ? users.OrderByDescending(o => o.User.Id) : users.OrderBy(o => o.User.Id)).ToList();
                    break;
            }

            return users;
        }

        public async Task<DiscordUser> GetUserAsync(SocketGuild guild, SocketUser user)
        {
            var key = GenerateKey(guild, user);
            return await GetUserAsync(key);
        }

        public async Task<DiscordUser> GetUserAsync(string key)
        {
            if (!Users.ContainsKey(key))
                return null;

            var user = Users[key];
            var guild = DiscordClient.GetGuild(user.GuildIDSnowflake);

            if (guild == null)
                return null;

            var socketUser = await guild.GetUserFromGuildAsync(user.UserIDSnowflake);

            if (socketUser == null)
                return null;

            return new DiscordUser(guild, socketUser, user);
        }

        public void Dispose()
        {
            SyncTimer.Dispose();
            SyncTimerCallback(null);
        }

        public void Init()
        {
            using var repository = Services.GetService<UsersRepository>();
            Users = repository.GetAllUsers().ToDictionary(o => $"{o.UserID}|{o.GuildID}", o => o);

            var syncPeriod = GrillBotService.DatabaseSyncPeriod;
            SyncTimer = new Timer(SyncTimerCallback, null, syncPeriod, syncPeriod);
            Logger.LogInformation("User data loaded from database. (Rows: {0})", Users.Count);
        }

        public async Task InitAsync()
        {
        }

        private void SyncTimerCallback(object _)
        {
            lock (locker)
            {
                if (Changes.Count == 0) return;

                var itemsForUpdate = Users.Where(o => Changes.Contains(o.Key)).Select(o => o.Value).ToList();
                using var repository = Services.GetService<UsersRepository>();
                repository.UpdateDatabase(itemsForUpdate);

                Changes.Clear();
                Logger.LogInformation("User info was synchronized with database. (Updated records: {0})", itemsForUpdate.Count);
            }
        }

        public void IncrementMessage(SocketGuildUser guildUser, SocketGuild guild, SocketGuildChannel channel)
        {
            var key = GenerateKey(guild, guildUser);

            lock (locker)
            {
                CreateUserIfNotExists(guild, guildUser, key);

                var user = Users[key];
                var channelEntity = user.Channels.FirstOrDefault(o => o.ChannelIDSnowflake == channel.Id);

                if (channelEntity == null)
                {
                    channelEntity = new DBUserChannel()
                    {
                        ChannelIDSnowflake = channel.Id,
                        Count = 1,
                        LastMessageAt = DateTime.Now,
                        UserID = user.ID,
                        DiscordUserIDSnowflake = guildUser.Id
                    };

                    user.Channels.Add(channelEntity);
                }
                else
                {
                    channelEntity.Count++;
                    channelEntity.LastMessageAt = DateTime.Now;
                }

                if (CanIncrementPoints(guild, guildUser))
                {
                    user.Points += Random.Next(15, 25);

                    if (LastPointsCalculatedAt.ContainsKey(key))
                        LastPointsCalculatedAt[key] = DateTime.Now;
                    else
                        LastPointsCalculatedAt.Add(key, DateTime.Now);
                }

                Changes.Add(key);
            }
        }

        public void DecrementMessage(SocketGuildUser guildUser, SocketGuild guild, SocketGuildChannel channel)
        {
            var key = GenerateKey(guild, guildUser);

            lock (locker)
            {
                if (!Users.ContainsKey(key))
                    return;

                var user = Users[key];
                var channelEntity = user.Channels.FirstOrDefault(o => o.ChannelIDSnowflake == channel.Id);

                if (channelEntity == null || channelEntity.Count == 0)
                    return;

                channelEntity.Count--;
                Changes.Add(key);
            }
        }

        private bool CanIncrementPoints(SocketGuild guild, SocketGuildUser user)
        {
            var key = GenerateKey(guild, user);

            lock (locker)
            {
                if (!LastPointsCalculatedAt.ContainsKey(key))
                    return true;

                var lastMessageAt = LastPointsCalculatedAt[key];

                return (DateTime.Now - lastMessageAt).TotalMinutes > 1.0;
            }
        }
        private string GenerateKey(IGuild guild, IUser user) => GenerateKey(guild.Id, user.Id);
        private string GenerateKey(ulong guildID, ulong userID) => $"{userID}|{guildID}";

        private void CreateUserIfNotExists(SocketGuild guild, SocketGuildUser user, string key)
        {
            if (!Users.ContainsKey(key))
            {
                Users.Add(key, new Database.Entity.Users.DiscordUser()
                {
                    GuildIDSnowflake = guild.Id,
                    UserIDSnowflake = user.Id
                });
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
            var authorKey = GenerateKey(author.Guild, author);
            var reactingUserKey = GenerateKey(reactingUser.Guild, reactingUser);

            lock (locker)
            {
                if (authorKey == reactingUserKey)
                    return; // Author add reaction to his message.

                CreateUserIfNotExists(author.Guild, author, authorKey);
                CreateUserIfNotExists(reactingUser.Guild, reactingUser, reactingUserKey);

                var authorUserEntity = Users[authorKey];
                var reactingUserEntity = Users[reactingUserKey];

                authorUserEntity.ObtainedReactionsCount++;
                reactingUserEntity.GivenReactionsCount++;

                Changes.Add(authorKey);
                Changes.Add(reactingUserKey);
            }
        }

        private void DecrementReaction(SocketGuildUser author, SocketGuildUser reactingUser)
        {
            var authorKey = GenerateKey(author.Guild, author);
            var reactingUserKey = GenerateKey(reactingUser.Guild, reactingUser);

            lock (locker)
            {
                if (authorKey == reactingUserKey)
                    return; // Author add reaction to his message.

                CreateUserIfNotExists(author.Guild, author, authorKey);
                CreateUserIfNotExists(reactingUser.Guild, reactingUser, reactingUserKey);

                var authorUserEntity = Users[authorKey];
                var reactingUserEntity = Users[reactingUserKey];

                if (authorUserEntity.ObtainedReactionsCount > 0)
                    authorUserEntity.ObtainedReactionsCount--;

                if (reactingUserEntity.GivenReactionsCount > 0)
                    reactingUserEntity.GivenReactionsCount--;

                Changes.Add(authorKey);
                Changes.Add(reactingUserKey);
            }
        }

        public string AddUserToWebAdmin(SocketGuild guild, SocketGuildUser user, string password = null)
        {
            var userKey = GenerateKey(guild, user);

            lock (locker)
            {
                CreateUserIfNotExists(guild, user, userKey);
                var userEntity = Users[userKey];

                var plainPassword = string.IsNullOrEmpty(password) ? StringHelper.CreateRandomString(20) : password;
                userEntity.WebAdminPassword = BCrypt.Net.BCrypt.HashPassword(plainPassword);

                Changes.Add(userKey);
                return plainPassword;
            }
        }

        public void RemoveUserFromWebAdmin(SocketGuild guild, SocketGuildUser user)
        {
            var userKey = GenerateKey(guild, user);

            lock (locker)
            {
                CreateUserIfNotExists(guild, user, userKey);

                var userEntity = Users[userKey];

                if (string.IsNullOrEmpty(userEntity.WebAdminPassword))
                    throw new ArgumentException("Tento uživatel neměl přístup.");

                userEntity.WebAdminPassword = null;
                Changes.Add(userKey);
            }
        }

        public bool AuthenticateWebAccess(SocketGuild guild, SocketGuildUser user, string password)
        {
            var userKey = GenerateKey(guild, user);

            lock (locker)
            {
                CreateUserIfNotExists(guild, user, userKey);

                var userEntity = Users[userKey];

                if (string.IsNullOrEmpty(userEntity.WebAdminPassword))
                    return false;

                return BCrypt.Net.BCrypt.Verify(password, userEntity.WebAdminPassword);
            }
        }

        public void RemoveChannel(string userKey, ulong channelID)
        {
            lock (locker)
            {
                var user = Users[userKey];
                var channel = user.Channels.FirstOrDefault(o => o.ChannelIDSnowflake == channelID);

                if (channel != null)
                    user.Channels.Remove(channel);
            }
        }
    }
}
