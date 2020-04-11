using Discord.Commands;
using Grillbot.Database.Repository;
using Grillbot.Helpers;
using Grillbot.Models.Channelboard;
using Grillbot.Models.Config.Dynamic;
using Microsoft.Extensions.Caching.Memory;
using System;

namespace Grillbot.Services.Channelboard
{
    public class ChannelboardWeb
    {
        private ConfigRepository ConfigRepository { get; }
        private IMemoryCache Cache { get; }

        public ChannelboardWeb(ConfigRepository repository, IMemoryCache cache)
        {
            ConfigRepository = repository;
            Cache = cache;
        }

        public string GetWebUrl(SocketCommandContext context)
        {
            var random = new Random();
            var config = ConfigRepository.FindConfig(context.Guild.Id, "", "channelboardweb").GetData<ChannelboardConfig>();

            var key = StringHelper.CreateRandomString(random.Next(10, 50));
            var item = new ChannelboardWebCacheItem() { GuildID = context.Guild.Id, UserID = context.User.Id };

            Cache.Set(key, item, DateTimeOffset.Now.AddHours(1));
            return string.Format(config.WebUrl, $"?key={key}");
        }

        public ChannelboardWebCacheItem GetItem(string key)
        {
            if (!string.IsNullOrEmpty(key) && Cache.TryGetValue(key, out ChannelboardWebCacheItem item))
                return item;

            return null;
        }
    }
}
