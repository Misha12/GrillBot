using Discord.Commands;
using Grillbot.Database.Repository;
using Grillbot.Helpers;
using Grillbot.Services.Config.Models;
using Microsoft.Extensions.Caching.Memory;
using System;

namespace Grillbot.Services.Channelboard
{
    public class ChannelboardWeb
    {
        private ConfigRepository ConfigRepository { get; }
        private IMemoryCache Cache { get; set; }

        public ChannelboardWeb(ConfigRepository repository, IMemoryCache cache)
        {
            ConfigRepository = repository;
            Cache = cache;
        }

        public string GetWebUrl(SocketCommandContext context)
        {
            var random = new Random();
            var config = ConfigRepository.FindConfig(context.Guild.Id, "", "channelboardweb").GetData<ChannelboardConfig>();
            
            var key = StringHelper.CreateRandomString(random.Next(0, 50));
            var item = new ChannelboardWebItem() { GuildID = context.Guild.Id, UserID = context.User.Id };

            Cache.Set(key, item, DateTimeOffset.Now.AddHours(1));
            return string.Format(config.WebUrl, $"?key={key}");
        }

        public ChannelboardWebItem GetItem(string key) => !string.IsNullOrEmpty(key) && Cache.TryGetValue(key, out ChannelboardWebItem item) ? item : null;
    }
}
