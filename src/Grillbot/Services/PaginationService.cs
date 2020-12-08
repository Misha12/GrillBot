using Discord;
using Discord.WebSocket;
using Grillbot.Extensions.Discord;
using Grillbot.Helpers;
using Grillbot.Models.Embed.PaginatedEmbed;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Grillbot.Services
{
    public class PaginationService
    {
        private Dictionary<ulong, PaginatedEmbed> Embeds { get; }
        private DiscordSocketClient DiscordClient { get; }

        public PaginationService(DiscordSocketClient discordClient)
        {
            Embeds = new Dictionary<ulong, PaginatedEmbed>();
            DiscordClient = discordClient;
        }

        public void AddEmbed(IMessage message, PaginatedEmbed embed)
        {
            Embeds.Add(message.Id, embed);
        }

        public void DeleteEmbed(ulong messageID)
        {
            if (Embeds.ContainsKey(messageID))
                Embeds.Remove(messageID);
        }

        public async Task SendPaginatedMessage(PaginatedEmbed embed, Func<Embed, Task<IUserMessage>> replyAsync)
        {
            var embedData = embed.RenderEmbed();
            var message = await replyAsync(embedData);

            if (embed.Pages.Count > 1)
            {
                if (embed.Pages.Count > 2)
                    await message.AddReactionAsync(EmojiHelper.TrackPrevious);

                await message.AddReactionAsync(EmojiHelper.ArrowBackward);
                await message.AddReactionAsync(EmojiHelper.ArrowForward);

                if (embed.Pages.Count > 2)
                    await message.AddReactionAsync(EmojiHelper.TrackNext);

                AddEmbed(message, embed);
            }
        }

        public async Task HandleReactionAsync(SocketReaction reaction)
        {
            if (!CheckAndParseReaction(reaction, out var emoji, out var embed))
                return;

            bool changed = false;
            if (emoji.Equals(EmojiHelper.TrackPrevious))
                changed = embed.FirstPage();
            else if (emoji.Equals(EmojiHelper.ArrowBackward))
                changed = embed.PrevPage();
            else if (emoji.Equals(EmojiHelper.ArrowForward))
                changed = embed.NextPage();
            else if (emoji.Equals(EmojiHelper.TrackNext))
                changed = embed.LastPage();

            if (changed)
            {
                var embedData = embed.RenderEmbed();
                await reaction.Message.Value.ModifyAsync(o => o.Embed = embedData);
            }

            await reaction.Message.Value.RemoveReactionAsync(reaction.Emote, reaction.User.Value);
        }

        private bool CheckAndParseReaction(SocketReaction reaction, out Emoji emoji, out PaginatedEmbed embed)
        {
            emoji = null;
            embed = null;

            // Message check
            if (!reaction.Message.IsSpecified || reaction.Message.Value.Author.IsUser() || reaction.Message.Value.Author.Id != DiscordClient.CurrentUser.Id)
                return false;

            // Reaction check
            if (reaction.Emote is not Emoji _emoji)
                return false;

            // Embed check
            if (reaction.Message.Value.Embeds.Count == 0 || !Embeds.TryGetValue(reaction.MessageId, out embed) || embed.ResponseFor.Id != reaction.UserId)
                return false;

            emoji = _emoji;
            return true;
        }
    }
}
