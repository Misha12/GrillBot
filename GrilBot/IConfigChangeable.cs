using Microsoft.Extensions.Configuration;

namespace GrilBot
{
    public interface IConfigChangeable
    {
        void ConfigChanged(IConfigurationRoot newConfig);
    }
}
