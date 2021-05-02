using Discord;
using Discord.Commands;
using Grillbot.Attributes;
using System.Net.Http;
using System.Threading.Tasks;

namespace Grillbot.Modules
{
    [Group("system")]
    [Name("Interní správa bota")]
    [ModuleID(nameof(SystemModule))]
    public class SystemModule : BotModuleBase
    {
        [Command("send")]
        [Summary("Odeslání zprávy do kanálu.")]
        public async Task SendAsync(ITextChannel textChannel, [Remainder] string message)
        {
            await textChannel.SendMessageAsync(message);

            using var httpClient = new HttpClient();
            foreach (var attachment in Context.Message.Attachments)
            {
                using var response = await httpClient.GetAsync(attachment.Url);

                if (!response.IsSuccessStatusCode)
                    continue;

                using var stream = await response.Content.ReadAsStreamAsync();
                await textChannel.SendFileAsync(stream, attachment.Filename, isSpoiler: attachment.IsSpoiler());
            }
        }
    }
}
