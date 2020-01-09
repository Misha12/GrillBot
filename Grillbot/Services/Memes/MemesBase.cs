using Discord.Commands;
using Grillbot.Services.Config;
using Grillbot.Services.Config.Models;
using System.Threading.Tasks;

namespace Grillbot.Services.Memes
{
    public abstract class MemesBase : IConfigChangeable
    {
        protected Configuration Config { get; set; }

        protected MemesBase(Configuration config)
        {
            Config = config;
        }

        public abstract bool CanExecute(SocketCommandContext context);
        public abstract Task ExecuteAsync(SocketCommandContext context);

        public virtual void OnConfigChange(Configuration newConfig) { }

        public void ConfigChanged(Configuration newConfig)
        {
            OnConfigChange(newConfig);
            Config = newConfig;
        }
    }
}
