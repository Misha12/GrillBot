using Discord.Commands;
using Grillbot.Database;
using Grillbot.Helpers;
using Grillbot.Models.Channelboard;
using Grillbot.Models.Config.Dynamic;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Threading.Tasks;

namespace Grillbot.Services.Channelboard
{
    public class ChannelboardWeb
    {
        private IMemoryCache Cache { get; }
        private IGrillBotRepository GrillBotRepository { get; }

        public ChannelboardWeb(IMemoryCache cache, IGrillBotRepository grillBotRepository)
        {
            Cache = cache;
            GrillBotRepository = grillBotRepository;
        }

        public async Task<string> GetWebUrlAsync(SocketCommandContext context)
        {
            var random = new Random();
            var configData = await GrillBotRepository.ConfigRepository.FindConfigAsync(context.Guild.Id, "channelboard", "web");
            var config = configData.GetData<ChannelboardConfig>();

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
