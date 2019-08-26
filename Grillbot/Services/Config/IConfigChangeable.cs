using Microsoft.Extensions.Configuration;

namespace Grillbot.Services.Config
{
    public interface IConfigChangeable
    {
        void ConfigChanged(IConfiguration newConfig);
    }
}
