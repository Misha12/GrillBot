using Discord;
using Discord.Net;
using Discord.WebSocket;
using Grillbot.Extensions;
using Grillbot.Repository;
using Grillbot.Repository.Entity;
using Grillbot.Services.Config;
using Grillbot.Services.Config.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Grillbot.Services
{
    public class TempUnverifyService : ServicesBase, IConfigChangeable
    {
        private List<TempUnverifyItem> Data { get; }
        private Configuration Config { get; set; }
        private BotLoggingService LoggingService { get; }
        private DiscordSocketClient Client { get; }

        public TempUnverifyService(IOptions<Configuration> config, BotLoggingService loggingService, DiscordSocketClient client)
        {
            Data = new List<TempUnverifyItem>();
            Config = config.Value;
            LoggingService = loggingService;
            Client = client;
        }

        public async Task InitAsync()
        {
            if (Data.Count > 0)
            {
                foreach (var item in Data)
                {
                    item.Dispose();
                }

                Data.Clear();
            }

            int processedCount = 0;
            int waitingCount = 0;

            using (var repository = new TempUnverifyRepository(Config))
            {
                var items = await repository.GetAllItems().ToListAsync().ConfigureAwait(false);

                foreach (var item in items)
                {
                    if (item.GetEndDatetime() < DateTime.Now)
                    {
                        ReturnAccess(item);
                        processedCount++;
                    }
                    else
                    {
                        item.InitTimer(ReturnAccess);
                        Data.Add(item);
                        waitingCount++;
                    }
                }
            }

            await LoggingService.WriteToLogAsync($"TempUnverify loaded. ReturnedAccessCount: {processedCount}, WaitingCount: {waitingCount}").ConfigureAwait(false);
        }

        private void ReturnAccess(object item)
        {
            if (item is TempUnverifyItem unverify)
            {
                var guild = Client.GetGuild(Convert.ToUInt64(unverify.GuildID));
                if (guild == null) return;

                var user = GetUserFromGuildAsync(guild, unverify.UserID).Result;
                if (user == null)
                {
                    var admin = Client.GetUser(Config.MethodsConfig.TempUnverify.MainAdminSnowflake);
                    var pmChannel = admin.GetOrCreateDMChannelAsync().GetAwaiter().GetResult();

                    var dataToPM = new
                    {
                        unverify,
                        guildName = guild.Name
                    };

                    var content = $"```json\n{JsonConvert.SerializeObject(dataToPM, Formatting.Indented)}```";
                    pmChannel.SendMessageAsync(content).GetAwaiter().GetResult();

                    return;
                }

                var rolesToReturn = unverify.DeserializedRolesToReturn;
                var roles = guild.Roles.Where(o => rolesToReturn.Contains(o.Name)).ToList();

                LoggingService.WriteToLog($"ReturnAccess User: {user.Username}#{user.Discriminator} ({user.Id}) " +
                    $"Roles: {string.Join(", ", rolesToReturn)}");

                user.AddRolesAsync(roles).GetAwaiter().GetResult();

                foreach (var channelOverride in unverify.DeserializedChannelOverrides)
                {
                    var channel = guild.GetChannel(channelOverride.ChannelIdSnowflake);

                    if (channel != null)
                        channel.AddPermissionOverwriteAsync(user, channelOverride.GetPermissions()).GetAwaiter().GetResult();
                }

                using (var repository = new TempUnverifyRepository(Config))
                {
                    repository.RemoveItem(unverify.ID);
                }

                unverify.Dispose();
                Data.RemoveAll(o => o.ID == unverify.ID);
            }
        }

        private async Task<SocketGuildUser> GetUserFromGuildAsync(SocketGuild guild, string userId)
        {
            var idOfUser = Convert.ToUInt64(userId);
            var user = guild.GetUser(idOfUser);

            if (user == null)
            {
                await guild.DownloadUsersAsync().ConfigureAwait(false);
                user = guild.GetUser(idOfUser);
            }

            return user;
        }

        public async Task<string> RemoveAccessAsync(List<SocketGuildUser> users, string time, string data, SocketGuild guild)
        {
            CheckIfCanStartUnverify(users, guild);

            var reason = ParseReason(data);
            var unverifyTime = ParseUnverifyTime(time);
            var unverifiedPersons = new List<TempUnverifyItem>();

            using (var repository = new TempUnverifyRepository(Config))
            {
                foreach (var user in users)
                {
                    var person = await RemoveAccessAsync(repository, user, unverifyTime, reason).ConfigureAwait(false);
                    unverifiedPersons.Add(person);
                }
            }

            unverifiedPersons.ForEach(o => o.InitTimer(ReturnAccess));
            Data.AddRange(unverifiedPersons);

            return FormatMessageToChannel(users, unverifiedPersons, reason);
        }

        public void CheckIfCanStartUnverify(List<SocketGuildUser> users, SocketGuild guild)
        {
            var owner = users.Find(o => o.Id == guild.OwnerId);

            if (owner != null)
                throw new ArgumentException("Nelze provést odebrání přístupu, protože se mezi uživateli nachází vlastník serveru.");

            var botMaxRolePosition = guild.CurrentUser.Roles.Max(o => o.Position);
            foreach (var user in users)
            {
                if (user.Id == guild.CurrentUser.Id)
                    throw new ArgumentException("Nelze provést odebrání přístupu, protože tagnutý uživatel jsem já.");

                var usersMaxRolePosition = user.Roles.Max(o => o.Position);

                if (usersMaxRolePosition > botMaxRolePosition)
                {
                    var higherRoles = user.Roles.Where(o => o.Position > botMaxRolePosition).Select(o => o.Name);

                    throw new ArgumentException($"Nelze provést odebírání přístupu, protože uživatel **{user.Username}#{user.Discriminator}** má vyšší role " +
                        $"**({string.Join(", ", higherRoles)})**.");
                }

                if (Config.IsUserBotAdmin(user.Id))
                    throw new ArgumentException($"Nelze provést odebrání přístupu, protože uživatel **{user.Username}#{user.Discriminator}** je administrátor bota.");
            }
        }

        private async Task<TempUnverifyItem> RemoveAccessAsync(TempUnverifyRepository repository, SocketGuildUser user, long unverifyTime, string reason)
        {
            var rolesToRemove = user.Roles.Where(o => !o.IsEveryone && !o.IsManaged).ToList();
            var rolesToRemoveNames = rolesToRemove.Select(o => o.Name).ToList();
            var overrides = GetChannelOverrides(user);

            await LoggingService.WriteToLogAsync($"RemoveAccess {unverifyTime} secs (Roles: {string.Join(", ", rolesToRemoveNames)}, " +
                $"ExtraChannels: {string.Join(", ", overrides.Select(o => $"{o.ChannelId} => AllowVal: {o.AllowValue}, DenyVal => {o.DenyValue}"))}), " +
                $"{user.Username}#{user.Discriminator} ({user.Id}) Reason: {(string.IsNullOrEmpty(reason) ? "-" : reason)}").ConfigureAwait(false);

            await user.RemoveRolesAsync(rolesToRemove).ConfigureAwait(false);

            foreach (var channelOverride in overrides)
            {
                var channel = user.Guild.GetChannel(channelOverride.ChannelIdSnowflake);
                await channel?.RemovePermissionOverwriteAsync(user);
            }

            var unverify = await repository.AddItemAsync(rolesToRemoveNames, user.Id, user.Guild.Id, unverifyTime, overrides, reason).ConfigureAwait(false);

            await SendPrivateMessage(user, unverify, reason).ConfigureAwait(false);
            return unverify;
        }

        private List<ChannelOverride> GetChannelOverrides(SocketGuildUser user)
        {
            var overrides = new List<ChannelOverride>();

            foreach (var channel in user.Guild.Channels)
            {
                var permissions = channel.GetPermissionOverwrite(user);

                if (permissions != null)
                    overrides.Add(new ChannelOverride(channel.Id, permissions.Value));
            }

            return overrides;
        }

        /// <summary>
        /// Returns time for unverify in seconds;
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        private long ParseUnverifyTime(string time)
        {
            var timeWithoutSuffix = time.Substring(0, time.Length - 1);

            if (!timeWithoutSuffix.All(o => char.IsDigit(o)))
                throw new ArgumentException("Neplatný časový formát.");

            var convertedTime = Convert.ToInt64(timeWithoutSuffix);

            if (time.EndsWith("s"))
            {
                // Seconds
                if (convertedTime < 30)
                    throw new ArgumentException("Minimální čas pro unverify ve vteřinách je 30 sec");

                return convertedTime;
            }
            else if (time.EndsWith("m"))
            {
                // Minutes
                if (convertedTime <= 0)
                    throw new ArgumentException("Minimální čas pro unverify v minutách je 1 minuta.");

                return ConvertTimeSpanToSeconds(TimeSpan.FromMinutes(convertedTime));
            }
            else if (time.EndsWith("h"))
            {
                // Hours
                if (convertedTime <= 0)
                    throw new ArgumentException("Minimální čas pro unverify v hodinách je 1 hodina.");

                return ConvertTimeSpanToSeconds(TimeSpan.FromHours(convertedTime));
            }
            else if (time.EndsWith("d"))
            {
                // Days
                if (convertedTime <= 0)
                    throw new ArgumentException("Minimální čas pro unverify v dnech je 1 den.");

                return ConvertTimeSpanToSeconds(TimeSpan.FromDays(convertedTime));
            }
            else
            {
                throw new ArgumentException("Nepodporovaný časový formát.");
            }
        }

        private long ConvertTimeSpanToSeconds(TimeSpan timeSpan) => Convert.ToInt64(Math.Round(timeSpan.TotalSeconds));

        private string ParseReason(string data)
        {
            const string errorMessage = "Nemůžu bezdůvodně odebrat přístup. Uveď důvod (`unverify {time} {reason} [{tags}]`)";

            if (data.StartsWith("<@"))
                throw new ArgumentException(errorMessage);

            var reason = data.Split("<@", StringSplitOptions.RemoveEmptyEntries)[0].Trim();

            if (string.IsNullOrEmpty(reason))
                throw new ArgumentException(errorMessage);

            return reason;
        }

        private string GetFormatedPrivateMessage(SocketGuildUser user, TempUnverifyItem item, string reason)
        {
            var builder = new StringBuilder();

            builder
                .Append("Byly ti dočasně odebrány všechny role na serveru **")
                .Append(user.Guild.Name)
                .Append("**. Navráceny ti budou **")
                .Append(item.GetEndDatetime().ToLocaleDatetime())
                .Append("**.");

            if (!string.IsNullOrEmpty(reason))
                builder.AppendLine().Append("Důvod: ").Append(reason);

            return builder.ToString();
        }

        private string FormatMessageToChannel(List<SocketGuildUser> users, List<TempUnverifyItem> unverifyItems, string reason)
        {
            var builder = new StringBuilder();

            builder
                .Append("Dočasné odebrání přístupu pro uživatele **")
                .Append(string.Join(", ", users.Select(o => $"{o.Username}#{o.Discriminator}")))
                .Append("** bylo dokončeno. Role budou navráceny **")
                .Append(unverifyItems[0].GetEndDatetime().ToLocaleDatetime())
                .Append("**");

            if (!string.IsNullOrEmpty(reason))
                builder.AppendLine().Append("Důvod: ").Append(reason);

            return builder.ToString();
        }

        private bool CanSendDM(SocketGuildUser user) => !user.IsBot && !user.IsWebhook;

        private async Task SendPrivateMessage(SocketGuildUser user, TempUnverifyItem unverify, string reason)
        {
            if (!CanSendDM(user)) return;

            try
            {
                var dmChannel = await user.GetOrCreateDMChannelAsync().ConfigureAwait(false);
                await dmChannel.SendMessageAsync(GetFormatedPrivateMessage(user, unverify, reason)).ConfigureAwait(false);
            }
            catch (HttpException ex)
            {
                if (ex.DiscordCode.HasValue && ex.DiscordCode.Value == 50007)
                    return; // User have disabled PM.

                throw;
            }
        }

        public async Task<EmbedBuilder> ListPersonsAsync(string callerUsername, string callerAvatarUrl)
        {
            using (var repository = new TempUnverifyRepository(Config))
            {
                var persons = await repository.GetAllItems().ToListAsync().ConfigureAwait(false);

                if (persons.Count == 0)
                    throw new ArgumentException("Nikdo zatím nemá odebraný přístup.");

                return await CreateListPersonsAsync(persons, new Tuple<string, string>(callerUsername, callerAvatarUrl)).ConfigureAwait(false);
            }
        }

        public async Task<EmbedBuilder> GetPersonUnverifyStatus(string callerUsername, string callerAvatarUrl, ulong searchedUserID)
        {
            using (var repository = new TempUnverifyRepository(Config))
            {
                var person = await repository.FindUnverifyByUserID(searchedUserID).ConfigureAwait(false);

                if (person == null)
                    throw new ArgumentException($"Uživatel s ID {searchedUserID} zatím nemá žádné unverify.");

                return await CreateListPersonsAsync(new List<TempUnverifyItem>() { person }, new Tuple<string, string>(callerUsername, callerAvatarUrl)).ConfigureAwait(false);
            }
        }

        private async Task<EmbedBuilder> CreateListPersonsAsync(List<TempUnverifyItem> items, Tuple<string, string> caller)
        {
            var embedBuilder = new EmbedBuilder()
                    .WithColor(Color.Blue)
                    .WithCurrentTimestamp()
                    .WithTitle("Seznam osob s odebraným přístupem.")
                    .WithThumbnailUrl(Client.CurrentUser.GetAvatarUrl())
                    .WithFooter($"Odpověď pro uživatele: {caller.Item1}", caller.Item2);

            foreach (var person in items)
            {
                var guild = Client.GetGuild(Convert.ToUInt64(person.GuildID));

                var desc = $"ID: {person.ID}\nDo kdy: {person.GetEndDatetime().ToLocaleDatetime()}\n" +
                    $"Role: {string.Join(", ", person.DeserializedRolesToReturn)}\n" +
                    $"Extra kanály: {BuildChannelOverrideList(person.DeserializedChannelOverrides, guild)}\n" +
                    $"Důvod: {person.Reason}";

                var user = await GetUserFromGuildAsync(guild, person.UserID).ConfigureAwait(false);

                string username;
                if (user != null)
                    username = $"{user.Username}#{user.Discriminator}";
                else
                    username = $"Neznámý uživatel {person.UserID}";

                embedBuilder.AddField(o => o.WithName(username).WithValue(desc));
            }

            return embedBuilder;
        }

        private string BuildChannelOverrideList(List<ChannelOverride> overrides, SocketGuild guild)
        {
            if (overrides.Count == 0)
                return "-";

            var builder = new List<string>();

            foreach (var channelOverride in overrides)
            {
                var channel = guild.GetChannel(channelOverride.ChannelIdSnowflake);

                if (channel != null)
                    builder.Add(channel.Name);
            }

            return string.Join(", ", builder);
        }

        public async Task<string> ReturnAccessAsync(int id)
        {
            using (var repository = new TempUnverifyRepository(Config))
            {
                var item = await repository.FindItemByIDAsync(id).ConfigureAwait(false);

                if (item == null)
                    throw new ArgumentException($"Odebrání přístupu s ID {id} nebylo v databázi nalezeno.");

                ReturnAccess(item);

                var guild = Client.GetGuild(Convert.ToUInt64(item.GuildID));
                var user = await GetUserFromGuildAsync(guild, item.UserID).ConfigureAwait(false);

                if (user == null)
                    throw new ArgumentException($"Uživatel s ID **{item.UserID}** nebyl na serveru **{guild.Name}** nalezen.");

                return $"Předčasné vrácení přístupu pro uživatele **{user.Username}#{user.Discriminator}** bylo dokončeno.";
            }
        }

        public async Task<string> UpdateUnverifyAsync(int id, string time)
        {
            var unverifyTime = ParseUnverifyTime(time);
            var item = Data.Find(o => o.ID == id);

            if (item == null)
                throw new ArgumentException($"Reset pro ID {id} nelze provést. Záznam nebyl nalezen");

            using (var repository = new TempUnverifyRepository(Config))
            {
                await repository.UpdateTimeAsync(id, unverifyTime).ConfigureAwait(false);
            }

            item.TimeFor = unverifyTime;
            item.StartAt = DateTime.Now;
            item.ReInitTimer(ReturnAccess);

            return $"Reset času pro záznam o dočasném odebrání přístupu s ID **{id}** byl úspěšně aktualizován. " +
                $"Role budou navráceny **{item.GetEndDatetime().ToLocaleDatetime()}**.";
        }

        public void ConfigChanged(Configuration newConfig)
        {
            Config = newConfig;
        }
    }
}
