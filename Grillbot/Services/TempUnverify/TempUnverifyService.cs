using Discord;
using Discord.WebSocket;
using Grillbot.Database.Entity;
using Grillbot.Database.Repository;
using Grillbot.Services.Config.Models;
using Grillbot.Services.Initiable;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Grillbot.Services.TempUnverify
{
    public partial class TempUnverifyService : IInitiable
    {
        private List<TempUnverifyItem> Data { get; }
        private Configuration Config { get; set; }
        private BotLoggingService Logger { get; }
        private DiscordSocketClient Client { get; }
        private TempUnverifyRepository Repository { get; }
        private ConfigRepository ConfigRepository { get; }

        public TempUnverifyService(IOptions<Configuration> config, BotLoggingService logger, DiscordSocketClient client,
            TempUnverifyRepository repository, ConfigRepository configRepository)
        {
            Data = new List<TempUnverifyItem>();
            Config = config.Value;
            Logger = logger;
            Client = client;
            Repository = repository;
            ConfigRepository = configRepository;
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

            var items = await Repository.GetAllItems().ToListAsync().ConfigureAwait(false);

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

            Logger.Write(LogSeverity.Info, $"TempUnverify loaded. ReturnedAccessCount: {processedCount}, WaitingCount: {waitingCount}");
        }

        public void Init() { }
    }
}
