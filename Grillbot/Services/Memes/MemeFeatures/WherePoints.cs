using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Grillbot.Extensions.Discord;
using Grillbot.Services.Config.Models;
using Grillbot.Services.TempUnverify;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Grillbot.Services.Memes.MemeFeatures
{
    public class WherePoints : MemesBase
    {
        private TempUnverifyService UnverifyService { get; }

        public WherePoints(Configuration config, TempUnverifyService unverifyService) : base(config)
        {
            UnverifyService = unverifyService;
        }

        public override bool CanExecute(SocketCommandContext context)
        {
            if (context.Guild == null)
                return false;

            if (!Config.MethodsConfig.Memes.AllowedChannels.Contains(context.Channel.Id.ToString()))
                return false;

            var regexes = new[]
            {
                "<OnlyForAuthorizedPersons>"
            };

            return regexes.All(r => Regex.IsMatch(context.Message.Content, r, RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.Multiline));
        }

        public override async Task ExecuteAsync(SocketCommandContext context)
        {
            var user = await context.Guild.GetUserFromGuildAsync(context.User.Id.ToString()).ConfigureAwait(false);

            if (user == null) // Strange situation.
                return;

            var username = user.GetFullName();
            var users = new List<SocketGuildUser>() { user };
            var unverifyTime = Config.MethodsConfig.Memes.UnverifyTime;
            var reason = Config.MethodsConfig.Memes.UnverifyReason.Replace("{@User}", username);

            var message = await UnverifyService.RemoveAccessAsync(users, unverifyTime, reason, context.Guild, context.User, true).ConfigureAwait(false);

            if(!string.IsNullOrEmpty(Config.MethodsConfig.Memes.WherePointsChannelID))
            {
                var wherePointsChannelID = Convert.ToUInt64(Config.MethodsConfig.Memes.WherePointsChannelID);
                var channel = context.Guild.GetChannel(wherePointsChannelID);
                await channel.AddPermissionOverwriteAsync(user, new OverwritePermissions(viewChannel: PermValue.Allow, sendMessages: PermValue.Allow)).ConfigureAwait(false);
            }

            await context.Channel.SendMessageAsync(message).ConfigureAwait(false);
            await context.Message.DeleteAsync().ConfigureAwait(false);
        }
    }
}
