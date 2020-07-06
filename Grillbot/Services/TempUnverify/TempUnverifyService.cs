using Discord;
using Discord.WebSocket;
using Grillbot.Database.Entity;
using Grillbot.Database.Repository;
using Grillbot.Extensions;
using Grillbot.Models.Config.Dynamic;
using Grillbot.Services.Initiable;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Grillbot.Services.TempUnverify
{
    public partial class TempUnverifyService : IInitiable
    {
        private List<TempUnverifyItem> Data { get; }
        private ILogger<TempUnverifyService> Logger { get; }
        private DiscordSocketClient Client { get; }
        private IServiceProvider Provider { get; }
        public List<IUser> ReturningAcccessFor { get; }

        public TempUnverifyService(DiscordSocketClient client, ILogger<TempUnverifyService> logger, IServiceProvider provider)
        {
            Data = new List<TempUnverifyItem>();
            ReturningAcccessFor = new List<IUser>();
            Logger = logger;
            Client = client;
            Provider = provider;
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

            using var scope = Provider.CreateScope();
            using var repository = scope.ServiceProvider.GetService<TempUnverifyRepository>();
            var items = await repository.GetAllItems(null).ToListAsync();

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

        public TempUnverifyConfig GetConfig(SocketGuild guild)
        {
            using var scope = Provider.CreateScope();
            using var configRepository = scope.ServiceProvider.GetService<ConfigRepository>();
            var config = configRepository.FindConfig(guild.Id, "unverify", "");
            return config.GetData<TempUnverifyConfig>();
        }
    }
}
