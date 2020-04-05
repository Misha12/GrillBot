using Discord;
using Discord.WebSocket;
using Grillbot.Database.Entity;
using Grillbot.Database.Repository;
using Grillbot.Models.Config;
using Grillbot.Services.Initiable;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Grillbot.Services.TempUnverify
{
    public partial class TempUnverifyService : IInitiable
    {
        private List<TempUnverifyItem> Data { get; }
        private Configuration Config { get; set; }
        private ILogger<TempUnverifyService> Logger { get; }
        private DiscordSocketClient Client { get; }
        private TempUnverifyRepository Repository { get; }
        private ConfigRepository ConfigRepository { get; }
        private TempUnverifyFactories Factories { get; }

        public TempUnverifyService(IOptions<Configuration> config, DiscordSocketClient client, TempUnverifyRepository repository,
            ConfigRepository configRepository, TempUnverifyFactories factories, ILogger<TempUnverifyService> logger)
        {
            Data = new List<TempUnverifyItem>();
            Config = config.Value;
            Logger = logger;
            Client = client;
            Repository = repository;
            ConfigRepository = configRepository;
            Factories = factories;
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

            Logger.LogInformation($"TempUnverify loaded. ReturnedAccessCount: {processedCount}, WaitingCount: {waitingCount}");
        }

        public void Init() { }

        private List<ulong> GetCurrentUnverifiedUserIDs()
        {
            return Data.Select(o => o.UserIDSnowflake).ToList();
        }
    }
}
