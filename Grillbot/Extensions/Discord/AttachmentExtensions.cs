using Discord;
using System.Net.Http;
using System.Threading.Tasks;

namespace Grillbot.Extensions.Discord
{
    public static class AttachmentExtensions
    {
        public static async Task<byte[]> DownloadFileAsync(this IAttachment attachment)
        {
            using var httpClient = new HttpClient();

            try
            {
                return await httpClient.GetByteArrayAsync(attachment.Url);
            }
            catch (HttpRequestException)
            {
                try
                {
                    return await httpClient.GetByteArrayAsync(attachment.ProxyUrl);
                }
                catch (HttpRequestException)
                {
                    return null;
                }
            }
        }
    }
}
