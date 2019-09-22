using Discord;
using Discord.WebSocket;
using Grillbot.Extensions;
using Grillbot.Repository;
using Grillbot.Repository.Entity;
using Grillbot.Services.Config.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Grillbot.Services
{
    public class TempUnverifyService
    {
        private List<TempUnverifyItem> Data { get; set; }
        private Configuration Config { get; }
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
                try
                {
                    var guild = Client.GetGuild(Convert.ToUInt64(unverify.GuildID));
                    if (guild == null) return;

                    var user = guild.GetUser(Convert.ToUInt64(unverify.UserID));
                    if (user == null) return;

                    var rolesToReturn = unverify.DeserializedRolesToReturn;
                    var roles = guild.Roles.Where(o => rolesToReturn.Contains(o.Name)).ToList();
                    user.AddRolesAsync(roles).GetAwaiter().GetResult();

                    LoggingService.WriteToLog($"ReturnAccess User: {user.Username}#{user.Discriminator} ({user.Id}) " +
                        $"Roles: {string.Join(", ", rolesToReturn)}");
                }
                finally
                {
                    using (var repository = new TempUnverifyRepository(Config))
                    {
                        repository.RemoveItemAsync(unverify.ID).GetAwaiter().GetResult();
                    }

                    unverify.Dispose();
                    Data.RemoveAll(o => o.ID == unverify.ID);
                }
            }
        }

        public async Task<string> RemoveAccess(List<SocketGuildUser> users, string time, string data)
        {
            var reason = ParseReason(data);
            var unverifyTime = ParseUnverifyTime(time);
            var unverifiedPersons = new List<TempUnverifyItem>();

            using (var repository = new TempUnverifyRepository(Config))
            {
                foreach (var user in users)
                {
                    var person = await RemoveAccess(repository, user, unverifyTime, reason);
                    unverifiedPersons.Add(person);
                }
            }

            unverifiedPersons.ForEach(o => o.InitTimer(ReturnAccess));
            Data.AddRange(unverifiedPersons);

            return FormatMessageToChannel(users, unverifiedPersons, reason);
        }

        private async Task<TempUnverifyItem> RemoveAccess(TempUnverifyRepository repository, SocketGuildUser user, long unverifyTime, string reason)
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

        public async Task<EmbedBuilder> ListPersons(string callerUsername, string callerAvatarUrl)
        {
            using (var repository = new TempUnverifyRepository(Config))
            {
                var persons = await repository.GetAllItems().ToListAsync();

                if (persons.Count == 0)
                    throw new ArgumentException("Nikdo zatím nemá odebraný přístup.");

                var embedBuilder = new EmbedBuilder()
                    .WithColor(Color.Blue)
                    .WithCurrentTimestamp()
                    .WithTitle("Seznam osob s odebraným přístupem.")
                    .WithThumbnailUrl(Client.CurrentUser.GetAvatarUrl())
                    .WithFooter($"Odpověď pro uživatele: {callerUsername}", callerAvatarUrl);

                foreach (var person in persons)
                {
                    var guild = Client.GetGuild(Convert.ToUInt64(person.GuildID));
                    var user = guild.GetUser(Convert.ToUInt64(person.UserID));

                    if (user == null)
                    {
                        person.Dispose();
                        await repository.RemoveItemAsync(person.ID);
                        Data.RemoveAll(o => o.ID == person.ID);
                        continue;
                    }

                    var desc = $"ID: {person.ID}\nDo kdy: {person.GetEndDatetime().ToLocaleDatetime()}\nRole: {string.Join(", ", person.DeserializedRolesToReturn)}";
                    embedBuilder.AddField(o => o.WithName($"{user.Username}#{user.Discriminator}").WithValue(desc));
                }

                return embedBuilder;
            }
        }

        public async Task<string> ReturnAccess(int id)
        {
            using (var repository = new TempUnverifyRepository(Config))
            {
                var item = await repository.FindItemByIDAsync(id);

                if (item == null)
                    throw new ArgumentException($"Odebrání rolí s ID {id} nebylo v databázi nalezeno.");

                ReturnAccess(item);

                var guild = Client.GetGuild(Convert.ToUInt64(item.GuildID));
                var user = guild.GetUser(Convert.ToUInt64(item.UserID));

                if (user == null)
                    throw new ArgumentException($"Uživatel s ID **{item.UserID}** nebyl na serveru **{guild.Name}** nalezen.");

                return $"Předčasné odebrání rolí pro uživatele **{user.Username}#{user.Discriminator}** bylo dokončeno.";
            }
        }

        public async Task<string> UpdateUnverify(int id, string time)
        {
            var unverifyTime = ParseUnverifyTime(time);
            var item = Data.FirstOrDefault(o => o.ID == id);

            if (item == null)
                throw new ArgumentException($"Reset pro ID {id} nelze provést. Záznam nebyl nalezen");

            using(var repository = new TempUnverifyRepository(Config))
            {
                await repository.UpdateTimeAsync(id, unverifyTime);
            }

            item.TimeFor = unverifyTime;
            item.StartAt = DateTime.Now;
            item.ReInitTimer(ReturnAccess);

            return $"Reset času pro záznam o dočasném odebrání rolí s ID **{id}** byl úspěšně aktualizován. " +
                $"Role budou navráceny **{item.GetEndDatetime().ToLocaleDatetime()}**.";
        }
    }
}
