using Discord;
using Discord.WebSocket;
using Grillbot.Extensions;
using Grillbot.Extensions.Discord;
using Grillbot.Repository;
using Grillbot.Repository.Entity;
using Grillbot.Repository.Entity.UnverifyLog;
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
    public class TempUnverifyService : IConfigChangeable
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
                Data.ForEach(o => o.Dispose());
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

                var user = guild.GetUserFromGuildAsync(unverify.UserID).Result;
                if (user == null)
                {
                    var admin = Client.GetUser(Config.MethodsConfig.TempUnverify.MainAdminSnowflake);
                    var pmChannel = admin.GetOrCreateDMChannelAsync().GetAwaiter().GetResult();

                    var dataToPM = new { unverify, guildName = guild.Name };
                    var content = $"```json\n{JsonConvert.SerializeObject(dataToPM, Formatting.Indented)}```";
                    pmChannel.SendMessageAsync(content).GetAwaiter().GetResult();

                    return;
                }

                ReturnAccessToPublicChannels(user, guild).GetAwaiter().GetResult();

                var rolesToReturn = unverify.DeserializedRolesToReturn;
                var roles = guild.Roles.Where(o => rolesToReturn.Contains(o.Name)).ToList();

                var isAutoRemove = (unverify.GetEndDatetime() - DateTime.Now).Ticks <= 0;

                if (isAutoRemove)
                {
                    using (var repository = new TempUnverifyRepository(Config))
                    {
                        var data = new UnverifyLogRemove()
                        {
                            Overrides = unverify.DeserializedChannelOverrides,
                            Roles = unverify.DeserializedRolesToReturn
                        };

                        data.SetUser(user);

                        repository
                            .LogOperationAsync(UnverifyLogOperation.AutoRemove, Client.CurrentUser, guild, data)
                            .GetAwaiter()
                            .GetResult();
                    }
                }

                LoggingService.WriteToLog($"ReturnAccess User: {user.GetShortName()} ({user.Id}) Roles: {string.Join(", ", rolesToReturn)}");
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

        public async Task<string> RemoveAccessAsync(List<SocketGuildUser> users, string time, string data, SocketGuild guild,
            SocketUser fromUser)
        {
            CheckIfCanStartUnverify(users, guild, false);

            var reason = ParseReason(data);
            var unverifyTime = ParseUnverifyTime(time);
            var unverifiedPersons = new List<TempUnverifyItem>();

            using (var repository = new TempUnverifyRepository(Config))
            {
                foreach (var user in users)
                {
                    var person = await RemoveAccessAsync(repository, user, unverifyTime, reason, fromUser, guild, false)
                        .ConfigureAwait(false);
                    unverifiedPersons.Add(person);
                }
            }

            unverifiedPersons.ForEach(o => o.InitTimer(ReturnAccess));
            Data.AddRange(unverifiedPersons);

            return FormatMessageToChannel(users, unverifiedPersons, reason);
        }

        public void CheckIfCanStartUnverify(List<SocketGuildUser> users, SocketGuild guild, bool self)
        {
            var owner = users.Find(o => o.Id == guild.OwnerId);

            if (owner != null)
                throw new ArgumentException("Nelze provést odebrání přístupu, protože se mezi uživateli nachází vlastník serveru.");

            var botMaxRolePosition = guild.CurrentUser.Roles.Max(o => o.Position);
            foreach (var user in users)
            {
                if (Data.Exists(o => o.UserID == user.Id.ToString()))
                    throw new ArgumentException($"Nelze provést odebrání rolí, protože uživatel **{user.GetFullName()}** již má unverify.");

                if (user.Id == guild.CurrentUser.Id)
                    throw new ArgumentException("Nelze provést odebrání přístupu, protože tagnutý uživatel jsem já.");

                var usersMaxRolePosition = user.Roles.Max(o => o.Position);

                if (usersMaxRolePosition > botMaxRolePosition && !self)
                {
                    var higherRoles = user.Roles.Where(o => o.Position > botMaxRolePosition).Select(o => o.Name);

                    throw new ArgumentException($"Nelze provést odebírání přístupu, protože uživatel **{user.GetShortName()}** má vyšší role " +
                        $"**({string.Join(", ", higherRoles)})**.");
                }

                if (Config.IsUserBotAdmin(user.Id) && !self)
                    throw new ArgumentException($"Nelze provést odebrání přístupu, protože uživatel **{user.GetShortName()}** je administrátor bota.");
            }
        }

        private async Task<TempUnverifyItem> RemoveAccessAsync(TempUnverifyRepository repository, SocketGuildUser user,
            long unverifyTime, string reason, SocketUser fromUser, SocketGuild guild, bool selfUnverify)
        {
            if (selfUnverify)
                reason = "Self unverify";

            var rolesToRemove = user.Roles.Where(o => !o.IsEveryone && !o.IsManaged).ToList();

            if (selfUnverify)
            {
                var botMaxRolePosition = guild.GetUser(Client.CurrentUser.Id).Roles.Max(o => o.Position);
                rolesToRemove = rolesToRemove.Where(o => o.Position < botMaxRolePosition).ToList();
            }

            var rolesToRemoveNames = rolesToRemove.Select(o => o.Name).ToList();
            var overrides = GetChannelOverrides(user);

            var data = new UnverifyLogSet()
            {
                Overrides = overrides,
                Roles = rolesToRemoveNames,
                StartAt = DateTime.Now,
                TimeFor = unverifyTime.ToString(),
                Reason = reason
            };

            data.SetUser(user);
            await repository.LogOperationAsync(UnverifyLogOperation.Set, fromUser, guild, data).ConfigureAwait(false);

            await LoggingService.WriteToLogAsync($"RemoveAccess {unverifyTime} secs (Roles: {string.Join(", ", rolesToRemoveNames)}, " +
                $"ExtraChannels: {string.Join(", ", overrides.Select(o => $"{o.ChannelId} => AllowVal: {o.AllowValue}, DenyVal => {o.DenyValue}"))}), " +
                $"{user.GetShortName()} ({user.Id}) Reason: {(string.IsNullOrEmpty(reason) ? "-" : reason)}").ConfigureAwait(false);

            await user.RemoveRolesAsync(rolesToRemove).ConfigureAwait(false);

            foreach (var channelOverride in overrides)
            {
                var channel = user.Guild.GetChannel(channelOverride.ChannelIdSnowflake);
                await channel?.RemovePermissionOverwriteAsync(user);
            }

            await RemoveAccessToPublicChannels(user, guild).ConfigureAwait(false);

            var unverify = await repository.AddItemAsync(rolesToRemoveNames, user.Id, user.Guild.Id, unverifyTime, overrides, reason).ConfigureAwait(false);

            var formatedPrivateMessage = GetFormatedPrivateMessage(user, unverify, reason);
            await user.SendPrivateMessageAsync(formatedPrivateMessage).ConfigureAwait(false);
            return unverify;
        }

        private async Task RemoveAccessToPublicChannels(SocketGuildUser user, SocketGuild guild)
        {
            foreach(var channel in guild.Channels)
            {
                var channelUser = channel.GetUser(user.Id);

                if(channelUser != null)
                {
                    var perms = new OverwritePermissions(sendMessages: PermValue.Deny);
                    await channel.AddPermissionOverwriteAsync(user, perms).ConfigureAwait(false);
                }
            }
        }

        private async Task ReturnAccessToPublicChannels(SocketGuildUser user, SocketGuild guild)
        {
            foreach(var channel in guild.Channels)
            {
                var overwrite = channel.GetPermissionOverwrite(user);

                if(overwrite != null)
                {
                    await channel.RemovePermissionOverwriteAsync(user).ConfigureAwait(false);
                }
            }
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
                .Append(string.Join(", ", users.Select(o => o.GetFullName())))
                .Append("** bylo dokončeno. Role budou navráceny **")
                .Append(unverifyItems[0].GetEndDatetime().ToLocaleDatetime())
                .Append("**");

            if (!string.IsNullOrEmpty(reason))
                builder.AppendLine().Append("Důvod: ").Append(reason);

            return builder.ToString();
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
                    .WithThumbnailUrl(Client.CurrentUser.GetUserAvatarUrl())
                    .WithFooter($"Odpověď pro uživatele: {caller.Item1}", caller.Item2);

            foreach (var person in items)
            {
                var guild = Client.GetGuild(Convert.ToUInt64(person.GuildID));

                var desc = $"ID: {person.ID}\nDo kdy: {person.GetEndDatetime().ToLocaleDatetime()}\n" +
                    $"Role: {string.Join(", ", person.DeserializedRolesToReturn)}\n" +
                    $"Extra kanály: {BuildChannelOverrideList(person.DeserializedChannelOverrides, guild)}\n" +
                    $"Důvod: {person.Reason}";

                var user = await guild.GetUserFromGuildAsync(person.UserID).ConfigureAwait(false);

                string username = user != null ? user.GetShortName() : $"Neznámý uživatel {person.UserID}";
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

        public async Task<string> ReturnAccessAsync(int id, SocketUser fromUser)
        {
            using (var repository = new TempUnverifyRepository(Config))
            {
                var item = await repository.FindItemByIDAsync(id).ConfigureAwait(false);

                if (item == null)
                    throw new ArgumentException($"Odebrání přístupu s ID {id} nebylo v databázi nalezeno.");

                var guild = Client.GetGuild(Convert.ToUInt64(item.GuildID));
                var user = await guild.GetUserFromGuildAsync(item.UserID).ConfigureAwait(false);

                if (user == null)
                    throw new ArgumentException($"Uživatel s ID **{item.UserID}** nebyl na serveru **{guild.Name}** nalezen.");

                var data = new UnverifyLogRemove()
                {
                    Overrides = item.DeserializedChannelOverrides,
                    Roles = item.DeserializedRolesToReturn
                };
                data.SetUser(user);

                await repository.LogOperationAsync(UnverifyLogOperation.Remove, fromUser, guild, data).ConfigureAwait(false);

                ReturnAccess(item);
                return $"Předčasné vrácení přístupu pro uživatele **{user.GetShortName()}** bylo dokončeno.";
            }
        }

        public async Task<string> UpdateUnverifyAsync(int id, string time, SocketUser fromUser)
        {
            var unverifyTime = ParseUnverifyTime(time);
            var item = Data.Find(o => o.ID == id);

            if (item == null)
                throw new ArgumentException($"Reset pro ID {id} nelze provést. Záznam nebyl nalezen");

            var guild = Client.GetGuild(Convert.ToUInt64(item.GuildID));
            var user = await guild.GetUserFromGuildAsync(item.UserID);

            var logData = new UnverifyLogUpdate() { TimeFor = $"{time} ({unverifyTime})" };
            logData.SetUser(user);

            using (var repository = new TempUnverifyRepository(Config))
            {
                await repository.LogOperationAsync(UnverifyLogOperation.Update, fromUser, guild, logData).ConfigureAwait(false);
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

        public async Task<string> SetSelfUnverify(SocketGuildUser user, SocketGuild guild, string time)
        {
            CheckIfCanStartUnverify(new List<SocketGuildUser>() { user }, guild, true);

            var unverifyTime = ParseUnverifyTime(time);
            TempUnverifyItem unverify;
            using (var repository = new TempUnverifyRepository(Config))
            {
                unverify = await RemoveAccessAsync(repository, user, unverifyTime, null, user, guild, true)
                    .ConfigureAwait(false);
            }

            unverify.InitTimer(ReturnAccess);
            Data.Add(unverify);

            return FormatMessageToChannel(
                new List<SocketGuildUser> { user },
                new List<TempUnverifyItem>() { unverify },
                null
            );
        }

        public async Task<string> UpdateSelfUnverify(SocketGuildUser user, string time)
        {
            var unverify = Data.Find(o => o.UserID == user.Id.ToString());

            if (unverify == null)
                throw new ArgumentException($"Reset pro uživatele {user.GetFullName()} nelze provést. Záznam nebyl nalezen");

            var message = await UpdateUnverifyAsync(unverify.ID, time, user).ConfigureAwait(false);
            return message;
        }

        public string SelfUnverifyStatus(SocketGuildUser user)
        {
            var unverify = Data.Find(o => o.UserID == user.Id.ToString());

            if (unverify == null)
                throw new ArgumentException("Nelze zjistit stav unverify. Záznam nebyl nalezen.");

            var unverifyTimeLeft = (unverify.GetEndDatetime() - DateTime.Now).ToString(@"hh\:mm\:ss");
            return string.Join(Environment.NewLine, new[]
            {
                $"Přístup odebrán **{unverify.StartAt.ToLocaleDatetime()}**",
                $"Přístup bude vrácen **{unverify.GetEndDatetime().ToLocaleDatetime()}** (za **{unverifyTimeLeft}**)"
            });
        }

        public async Task<string> ReturnSelfUnverifyAccess(SocketGuildUser user)
        {
            var unverify = Data.Find(o => o.UserID == user.Id.ToString());

            if (unverify == null)
                throw new ArgumentException($"Nelze provést vrácení přístupu. Záznam nebyl v databázi nalezen.");

            return await ReturnAccessAsync(unverify.ID, user).ConfigureAwait(false);
        }
    }
}
