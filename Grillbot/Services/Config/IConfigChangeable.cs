using Grillbot.Services.Config.Models;

namespace Grillbot.Services.Config
{
    public interface IConfigChangeable
    {
        void ConfigChanged(Configuration newConfig);
    }
}
