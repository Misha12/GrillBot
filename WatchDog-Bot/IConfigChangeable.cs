using Microsoft.Extensions.Configuration;

namespace WatchDog_Bot
{
    public interface IConfigChangeable
    {
        void ConfigChanged(IConfigurationRoot newConfig);
    }
}
