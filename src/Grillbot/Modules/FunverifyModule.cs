using Discord.Commands;
using Grillbot.Attributes;
using Grillbot.Database;
using Grillbot.Models.Config.Dynamic;
using Grillbot.Services.Unverify;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Grillbot.Modules
{
    [Group("funverify")]
    [Name("Fake odebírání přístupu")]
    [ModuleID(nameof(FunverifyModule))]
    public class FunverifyModule : BotModuleBase
    {
        public FunverifyModule(IServiceProvider provider) : base(provider: provider)
        {
        }

        [Command("")]
        [Summary("Dočasné falešné odebrání přístupu.")]
        public async Task SetFunverifyAsync(string time, [Remainder] string reasonAndMentions = null)
        {
            using (Context.Channel.EnterTypingState())
            {
                var usersToUnverify = Context.Message.MentionedUsers.Where(f => f != null).ToList();
                if (usersToUnverify.Count == 0)
                    return;

                using var repository = GetService<IGrillBotRepository>();
                var config = await repository.Service.ConfigRepository.FindConfigAsync(Context.Guild.Id, "funverify", null, false);

                if (config == null)
                    return;

                using var unverify = GetService<UnverifyService>();
                var messages = await unverify.Service.SetUnverifyAsync(usersToUnverify, time, reasonAndMentions, Context.Guild, Context.User, true);
                await ReplyChunkedAsync(messages, 1);

                var configData = config.GetData<FunverifyConfig>();
                await Task.Delay(TimeSpan.FromSeconds(configData.SecsTimeout));
                await ReplyAsync(configData.KappaLulEmote);
            }
        }
    }
}
