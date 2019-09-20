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
using System.Threading.Tasks;

namespace Grillbot.Services
{
    public class TempUnverifyService
    {
        private List<TempUnverifyItem> Data { get; set; }
        private Configuration Config { get; }
        private BotLoggingService LoggingService { get; }

        public TempUnverifyService(IOptions<Configuration> config, BotLoggingService loggingService)
        {
            Data = new List<TempUnverifyItem>();
            Config = config.Value;
            LoggingService = loggingService;
        }

        public async Task InitAsync()
        {
            using(var repository = new TempUnverifyRepository(Config))
            {
                var items = await repository.GetAllItems().ToListAsync();

                foreach(var item in items)
                {
                    if(item.GetEndDatetime() < DateTime.Now)
                    {
                        ReturnAccess(null);
                    }
                    else
                    {
                        item.InitTimer(ReturnAccess);
                        Data.Add(item);
                    }
                }
            }
        }

        private void ReturnAccess(object _)
        {

        }

        public async Task<string> SetUnverify(SocketGuildUser user, string time)
        {
            using(var repository = new TempUnverifyRepository(Config))
            {
                var unverifyTime = ParseUnverifyTime(time);

                var rolesToRemove = user.Roles.Where(o => !o.IsEveryone && !o.IsManaged).ToList();
                var rolesToRemoveNames = rolesToRemove.Select(o => o.Name).ToList();

                await LoggingService.WriteToLogAsync($"TempUnverify {time} (Roles: {string.Join(", ", rolesToRemoveNames)}), " +
                    $"{user.Username}#{user.Discriminator} ({user.Id})");

                await user.RemoveRolesAsync(rolesToRemove);

                var unverify = await repository.AddItemAsync(rolesToRemoveNames, user.Id, unverifyTime);

                unverify.InitTimer(ReturnAccess);
                Data.Add(unverify);

                await (await user.GetOrCreateDMChannelAsync()).SendMessageAsync(GetFormatedPrivateMessage(user, unverify));

                return $"Dočasné odebrání rolí pro uživatele **{user.Username}#{user.Discriminator}** bylo dokončeno, " +
                    $"role mu budou navráceny **{unverify.GetEndDatetime().ToLocaleDatetime()}**.";
            }
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

            if(time.EndsWith("s"))
            {
                // Seconds
                if (convertedTime < 10)
                    throw new ArgumentException("Minimální čas pro unverify ve vteřinách je 10 sec");

                return convertedTime;
            }
            else if(time.EndsWith("m"))
            {
                // Minutes
                if (convertedTime <= 0)
                    throw new ArgumentException("Minimální čas pro unverify v minutách je 1 minuta.");

                return ConvertTimeSpanToSeconds(TimeSpan.FromMinutes(convertedTime));
            }
            else if(time.EndsWith("h"))
            {
                // Hours
                if (convertedTime <= 0)
                    throw new ArgumentException("Minimální čas pro unverify v hodinách je 1 hodina.");

                return ConvertTimeSpanToSeconds(TimeSpan.FromHours(convertedTime));
            }
            else if(time.EndsWith("d"))
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

        private string GetFormatedPrivateMessage(SocketGuildUser user, TempUnverifyItem item)
        {
            return $"Byly ti dočasně odebrány všechny role na serveru **{user.Guild.Name}**. " +
                $"Navráceny ti budou **{item.GetEndDatetime().ToLocaleDatetime()}**.";
        }
    }
}
