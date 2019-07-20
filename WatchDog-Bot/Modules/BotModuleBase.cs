using Discord;
using Discord.Commands;

namespace WatchDog_Bot.Modules
{
    public abstract class BotModuleBase : ModuleBase<SocketCommandContext>
    {
        protected void AddInlineEmbedField(EmbedBuilder embed, string name, object value) =>
            embed.AddField(o => { o.Name = name; o.Value = value; o.IsInline = true; });
    }
}
