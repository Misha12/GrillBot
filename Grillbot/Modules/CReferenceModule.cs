using System.Threading.Tasks;
using Discord.Commands;
using Grillbot.Exceptions;
using Grillbot.Services;
using Grillbot.Services.Preconditions;

namespace Grillbot.Modules
{
    [Name("Hledani dokumentace")]
    [RequirePermissions("CReference")]
    public class CReferenceModule : BotModuleBase
    {
        private CReferenceService Service { get; }

        public CReferenceModule(CReferenceService service)
        {
            Service = service;
        }

        [Command("cref")]
        [Summary("Najde cpprefence stranku hledaneho tematu")]
        public async Task FindCReferenceAsync(string topic)
        {
            try
            {
                var message = await Service.GetReferenceUrlAsync(topic);
                await ReplyAsync(message);
            }
            catch (NotFoundException e)
            {
                await ReplyAsync(e.Message);
            }
        }
    }
}