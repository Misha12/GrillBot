using Discord;
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
    public class TempUnverifyService : IConfigChangeable
    {
        private List<TempUnverifyItem> Data { get; set; }
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
                var items = await repository.GetAllItems().ToListAsync();

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

            await LoggingService.WriteToLogAsync($"TempUnverify loaded. ReturnedAccessCount: {processedCount}, WaitingCount: {waitingCount}");
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
                await guild.DownloadUsersAsync();
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
                    var person = await RemoveAccessAsync(repository, user, unverifyTime, reason);
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
                throw new ArgumentException("Nelze provést odebrání rolí, protože se mezi uživateli nachází vlastník serveru.");

            var botMaxRolePosition = guild.CurrentUser.Roles.Max(o => o.Position);
            foreach (var user in users)
            {
                var usersMaxRolePosition = user.Roles.Max(o => o.Position);

                if (usersMaxRolePosition > botMaxRolePosition)
                {
                    var higherRoles = user.Roles.Where(o => o.Position > botMaxRolePosition).Select(o => o.Name);

                    throw new ArgumentException($"Nelze provést odebírání rolí, protože uživatel **{user.Username}#{user.Discriminator}** má vyšší role " +
                        $"**({string.Join(", ", higherRoles)})**.");
                }

                if (Config.IsUserBotAdmin(user.Id))
                    throw new ArgumentException($"Nelze provést odebrání rolí, protože uživatel **{user.Username}#{user.Discriminator}** je administrátor bota.");
            }
        }

        private async Task<TempUnverifyItem> RemoveAccessAsync(TempUnverifyRepository repository, SocketGuildUser user, long unverifyTime, string reason)
        {
            var rolesToRemove = user.Roles.Where(o => !o.IsEveryone && !o.IsManaged).ToList();
            var rolesToRemoveNames = rolesToRemove.Select(o => o.Name).ToList();

            await LoggingService.WriteToLogAsync($"RemoveAccess {unverifyTime} secs (Roles: {string.Join(", ", rolesToRemoveNames)}), " +
                    $"{user.Username}#{user.Discriminator} ({user.Id}) Reason: {(string.IsNullOrEmpty(reason) ? "-" : reason)}");

            await user.RemoveRolesAsync(rolesToRemove);

            var unverify = await repository.AddItemAsync(rolesToRemoveNames, user.Id, user.Guild.Id, unverifyTime);

            if (CanSendDM(user))
            {
                var dmChannel = await user.GetOrCreateDMChannelAsync();
                await dmChannel.SendMessageAsync(GetFormatedPrivateMessage(user, unverify, reason));
            }

            return unverify;
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
            if (data.StartsWith("<@")) return null;
            return data.Split("<@", StringSplitOptions.RemoveEmptyEntries)[0].Trim();
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
                .Append("Dočasné odebrání rolí pro uživatele **")
                .Append(string.Join(", ", users.Select(o => $"{o.Username}#{o.Discriminator}")))
                .Append("** bylo dokončeno. Role budou navráceny **")
                .Append(unverifyItems[0].GetEndDatetime().ToLocaleDatetime())
                .Append("**");

            if (!string.IsNullOrEmpty(reason))
                builder.AppendLine().Append("Důvod: ").Append(reason);

            return builder.ToString();
        }

        private bool CanSendDM(SocketGuildUser user) => !user.IsBot && !user.IsWebhook;

        public async Task<EmbedBuilder> ListPersonsAsync(string callerUsername, string callerAvatarUrl)
        {
            using (var repository = new TempUnverifyRepository(Config))
            {
                var persons = await repository.GetAllItems().ToListAsync();

                if (persons.Count == 0)
                    throw new ArgumentException("Nikdo zatím nemá odebraný přístup.");

                return await CreateListPersonsAsync(persons, new Tuple<string, string>(callerUsername, callerAvatarUrl));
            }
        }

        public async Task<EmbedBuilder> GetPersonUnverifyStatus(string callerUsername, string callerAvatarUrl, ulong searchedUserID)
        {
            using (var repository = new TempUnverifyRepository(Config))
            {
                var person = await repository.FindUnverifyByUserID(searchedUserID);

                if (person == null)
                    throw new ArgumentException($"Uživatel s ID {searchedUserID} zatím nemá žádné unverify.");

                return await CreateListPersonsAsync(new List<TempUnverifyItem>() { person }, new Tuple<string, string>(callerUsername, callerAvatarUrl));
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
                var desc = $"ID: {person.ID}\nDo kdy: {person.GetEndDatetime().ToLocaleDatetime()}\nRole: {string.Join(", ", person.DeserializedRolesToReturn)}";

                var guild = Client.GetGuild(Convert.ToUInt64(person.GuildID));
                var user = await GetUserFromGuildAsync(guild, person.UserID);

                string username;
                if (user != null)
                    username = $"{user.Username}#{user.Discriminator}";
                else
                    username = $"Neznámý uživatel {person.UserID}";

                embedBuilder.AddField(o => o.WithName(username).WithValue(desc));
            }

            return embedBuilder;
        }

        public async Task<string> ReturnAccessAsync(int id)
        {
            using (var repository = new TempUnverifyRepository(Config))
            {
                var item = await repository.FindItemByIDAsync(id);

                if (item == null)
                    throw new ArgumentException($"Odebrání rolí s ID {id} nebylo v databázi nalezeno.");

                ReturnAccess(item);

                var guild = Client.GetGuild(Convert.ToUInt64(item.GuildID));
                var user = await GetUserFromGuildAsync(guild, item.UserID);

                if (user == null)
                    throw new ArgumentException($"Uživatel s ID **{item.UserID}** nebyl na serveru **{guild.Name}** nalezen.");

                return $"Předčasné vrácení rolí pro uživatele **{user.Username}#{user.Discriminator}** bylo dokončeno.";
            }
        }

        public async Task<string> UpdateUnverifyAsync(int id, string time)
        {
            var unverifyTime = ParseUnverifyTime(time);
            var item = Data.FirstOrDefault(o => o.ID == id);

            if (item == null)
                throw new ArgumentException($"Reset pro ID {id} nelze provést. Záznam nebyl nalezen");

            using (var repository = new TempUnverifyRepository(Config))
            {
                await repository.UpdateTimeAsync(id, unverifyTime);
            }

            item.TimeFor = unverifyTime;
            item.StartAt = DateTime.Now;
            item.ReInitTimer(ReturnAccess);

            return $"Reset času pro záznam o dočasném odebrání rolí s ID **{id}** byl úspěšně aktualizován. " +
                $"Role budou navráceny **{item.GetEndDatetime().ToLocaleDatetime()}**.";
        }

        public void ConfigChanged(Configuration newConfig)
        {
            Config = newConfig;
        }
    }
}
