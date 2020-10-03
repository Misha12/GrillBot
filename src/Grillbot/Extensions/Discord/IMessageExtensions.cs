using Discord;
using Discord.Net;
using System.Net;
using System.Threading.Tasks;

namespace Grillbot.Extensions.Discord
{
    public static class IMessageExtensions
    {
        public static async Task DeleteMessageAsync(this IMessage message, RequestOptions options = null)
        {
            try
            {
                await message.DeleteAsync(options);
            }
            catch(HttpException ex)
            {
                if (ex.HttpCode == HttpStatusCode.NotFound)
                    return;

                throw;
            }
        }
    }
}
