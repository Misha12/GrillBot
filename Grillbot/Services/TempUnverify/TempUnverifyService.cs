using Discord.WebSocket;
using Grillbot.Repository;
using Grillbot.Repository.Entity;
using Grillbot.Services.Config;
using Grillbot.Services.Config.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Grillbot.Services.TempUnverify
{
    public partial class TempUnverifyService : IConfigChangeable
    {
        private List<TempUnverifyItem> Data { get; }
        private Configuration Config { get; set; }
        private BotLoggingService Logger { get; }
        private DiscordSocketClient Client { get; }

        public TempUnverifyService(IOptions<Configuration> config, BotLoggingService logger, DiscordSocketClient client)
        {
            Data = new List<TempUnverifyItem>();
            Config = config.Value;
            Logger = logger;
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

            await Logger.WriteAsync($"TempUnverify loaded. ReturnedAccessCount: {processedCount}, WaitingCount: {waitingCount}").ConfigureAwait(false);
        }

        public void ConfigChanged(Configuration newConfig)
        {
            Config = newConfig;
        }
    }
}
