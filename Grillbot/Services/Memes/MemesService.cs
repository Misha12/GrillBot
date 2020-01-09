using Discord.Commands;
using Grillbot.Services.Config;
using Grillbot.Services.Config.Models;
using Grillbot.Services.Memes.MemeFeatures;
using Grillbot.Services.TempUnverify;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Grillbot.Services.Memes
{
    public class MemesService : IConfigChangeable
    {
        private List<MemesBase> Methods { get; }

        public MemesService(TempUnverifyService tempUnverifyService, IOptions<Configuration> config)
        {
            Methods = new List<MemesBase>()
            {
                new WherePoints(config.Value, tempUnverifyService)
            };
        }

        public async Task ProcessAsync(SocketCommandContext context)
        {
            foreach(var method in Methods)
            {
                if(method.CanExecute(context))
                {
                    await method.ExecuteAsync(context).ConfigureAwait(false);
                }
            }
        }

        public void ConfigChanged(Configuration newConfig)
        {
            Methods.ForEach(o => o.OnConfigChange(newConfig));
        }
    }
}
