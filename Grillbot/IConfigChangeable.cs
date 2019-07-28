using Microsoft.Extensions.Configuration;

namespace Grillbot
{
    public interface IConfigChangeable
    {
        void ConfigChanged(IConfiguration newConfig);
    }
}
