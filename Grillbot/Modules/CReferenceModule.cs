using System.Threading.Tasks;
using Discord.Commands;
using Grillbot.Exceptions;
using Grillbot.Extensions;
using Grillbot.Services;
using Grillbot.Services.Permissions.Preconditions;

namespace Grillbot.Modules
{
    [RequirePermissions]
    [Name("Hledani dokumentace")]
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
                var message = await Service.GetReferenceUrlAsync(topic).ConfigureAwait(false);
                await ReplyAsync(message.PreventMassTags()).ConfigureAwait(false);
            }
            catch (NotFoundException e)
            {
                await ReplyAsync(e.Message.PreventMassTags()).ConfigureAwait(false);
            }
        }
    }
}