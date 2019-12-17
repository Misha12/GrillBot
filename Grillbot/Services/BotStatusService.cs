using Discord;
using Discord.WebSocket;
using Grillbot.Extensions.Discord;
using Grillbot.Helpers;
using Grillbot.Models;
using Grillbot.Models.BotStatus;
using Grillbot.Modules;
using Grillbot.Repository;
using Grillbot.Repository.Entity;
using Grillbot.Services.Config.Models;
using Grillbot.Services.Statistics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Grillbot.Services
{
    public class BotStatusService
    {
        private Statistics.Statistics Statistics { get; }
        private IHostingEnvironment HostingEnvironment { get; }
        private Logger.Logger Logger { get; }
        private AutoReplyService AutoReplyService { get; }
        private Configuration Config { get; }
        private CalledEventStats CalledEventStats { get; }
        private DiscordSocketClient Client { get; }

        public BotStatusService(Statistics.Statistics statistics, IHostingEnvironment hostingEnvironment, Logger.Logger logger,
            AutoReplyService autoReplyService, IOptions<Configuration> config, CalledEventStats calledEventStats,
            DiscordSocketClient client)
        {
            Statistics = statistics;
            HostingEnvironment = hostingEnvironment;
            Logger = logger;
            AutoReplyService = autoReplyService;
            Config = config.Value;
            CalledEventStats = calledEventStats;
            Client = client;
        }

        public SimpleBotStatus GetSimpleStatus()
        {
            var process = Process.GetCurrentProcess();

            return new SimpleBotStatus()
            {
                RamUsage = FormatHelper.FormatAsSize(process.WorkingSet64),
                ActiveWebTokensCount = Statistics.ChannelStats.GetActiveWebTokensCount(),
                InstanceType = GetInstanceType(),
                StartTime = process.StartTime,
                ThreadStatus = GetThreadStatus(process),
                AvgReactTime = Statistics.GetAvgReactTime(),
                ActiveCpuTime = process.TotalProcessorTime
            };
        }

        public List<StatisticsData> GetCallStats() => Statistics.GetOrderedData();

        public Dictionary<string, uint> GetLoggerStats()
        {
            return Logger.Counters.OrderByDescending(o => o.Value).ToDictionary(o => o.Key, o => o.Value);
        }

        private string GetInstanceType()
        {
            if (HostingEnvironment.IsProduction()) return "Release";
            if (HostingEnvironment.IsStaging()) return "Staging";

            return "Development";
        }

        private string GetThreadStatus(Process process)
        {
            int sleepCount = 0;
            var sleepCounter = process.Threads.GetEnumerator();
            while (sleepCounter.MoveNext())
            {
                if ((sleepCounter.Current as ProcessThread)?.ThreadState == ThreadState.Wait)
                    sleepCount++;
            }

            return $"{FormatHelper.FormatWithSpaces(process.Threads.Count)} ({FormatHelper.FormatWithSpaces(sleepCount)} spí)";
        }

        public List<AutoReplyItem> GetAutoReplyItems() => AutoReplyService.GetItems();

        public Dictionary<string, string> GetCalledEventStats() => CalledEventStats.GetValues();

        public async Task<Dictionary<string, int>> GetDbReport()
        {
            using (var repository = new BotDbRepository(Config))
            {
                return await repository.GetTableRowsCount().ConfigureAwait(false);
            }
        }

        public async Task<List<Models.BotStatus.CommandLog>> GetCommandLogsAsync()
        {
            using (var repository = new LogRepository(Config))
            {
                var data = await repository.GetCommandLogsAsync(5).ConfigureAwait(false);
                var result = new List<Models.BotStatus.CommandLog>();

                foreach (var dbData in data)
                {
                    var item = new Models.BotStatus.CommandLog()
                    {
                        ID = dbData.ID,
                        CalledAt = dbData.CalledAt,
                        Command = dbData.Command,
                        FullCommand = dbData.FullCommand,
                        Group = dbData.Group
                    };

                    await SetCommandLogData(item, dbData).ConfigureAwait(false);
                    result.Add(item);
                }

                return result;
            }
        }

        public async Task<Models.BotStatus.CommandLog> GetCommandDetailAsync(string id)
        {
            using (var repository = new LogRepository(Config))
            {
                var data = await repository.GetCommandLogDetailAsync(Convert.ToInt64(id)).ConfigureAwait(false);

                if (data == null)
                    return null;

                var result = new Models.BotStatus.CommandLog()
                {
                    ID = data.ID,
                    CalledAt = data.CalledAt,
                    Command = data.Command,
                    FullCommand = data.FullCommand,
                    Group = data.Group
                };

                await SetCommandLogData(result, data).ConfigureAwait(false);
                return result;
            }
        }

        private async Task SetCommandLogData(Models.BotStatus.CommandLog item, Repository.Entity.CommandLog dbData)
        {
            if (dbData.GuildIDSnowflake != null)
            {
                var guild = Client.GetGuild(dbData.GuildIDSnowflake.Value);
                var user = await guild.GetUserFromGuildAsync(dbData.UserID).ConfigureAwait(false);
                var channel = guild.GetTextChannel(dbData.ChannelIDSnowflake);

                item.Username = user.GetShortName();
                item.GuildName = guild.Name;
                item.ChannelName = channel.Name;
            }
            else
            {
                var user = Client.GetUser(dbData.UserIDSnowflake);

                string channelName;
                var dcChannel = Client.GetChannel(dbData.ChannelIDSnowflake);

                if (dcChannel == null)
                {
                    var dm = await Client.GetDMChannelAsync(dbData.ChannelIDSnowflake).ConfigureAwait(false);
                    channelName = dm == null ? $"{dbData.ChannelID} (Unknown type)" : $"@{dm.Recipient}";
                }
                else
                {
                    if (dcChannel is SocketDMChannel dm)
                        channelName = $"@{dm.Recipient}";
                    else if (dcChannel is SocketGroupChannel groupChannel)
                        channelName = groupChannel.Name;
                    else
                        channelName = $"{dcChannel.Id} ({dcChannel.GetType().Name})";
                }

                item.ChannelName = channelName;
                item.Username = user.GetShortName();
            }
        }
    }
}