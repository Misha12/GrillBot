using Discord;
using Discord.WebSocket;
using Grillbot.Models.PaginatedEmbed;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Grillbot.Services
{
    public class PaginationService
    {
        private Dictionary<ulong, PaginatedEmbed> Embeds { get; set; }

        private Emoji FirstPageEmoji => new Emoji("⏮️");
        private Emoji PrevPageEmoji => new Emoji("◀️");
        private Emoji NextPageEmoji => new Emoji("▶️");
        private Emoji LastPageEmoji => new Emoji("⏭️");

        public PaginationService()
        {
            Embeds = new Dictionary<ulong, PaginatedEmbed>();
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
                    await message.AddReactionAsync(FirstPageEmoji);

                await message.AddReactionAsync(PrevPageEmoji);
                await message.AddReactionAsync(NextPageEmoji);

                if (embed.Pages.Count > 2)
                    await message.AddReactionAsync(LastPageEmoji);

                AddEmbed(message, embed);
            }
        }

        public async Task HandleReactionAsync(SocketReaction reaction)
        {
            if (!Embeds.TryGetValue(reaction.MessageId, out PaginatedEmbed embed)
                || !reaction.Message.IsSpecified
                || !(reaction.Emote is Emoji emoji)
                || embed.ResponseFor.Id != reaction.UserId)
                return;

            bool changed = false;
            if (emoji.Equals(FirstPageEmoji))
                changed = embed.FirstPage();
            else if (emoji.Equals(PrevPageEmoji))
                changed = embed.PrevPage();
            else if (emoji.Equals(NextPageEmoji))
                changed = embed.NextPage();
            else if (emoji.Equals(LastPageEmoji))
                changed = embed.LastPage();

            if (changed)
            {
                var embedData = embed.RenderEmbed();
                await reaction.Message.Value.ModifyAsync(o => o.Embed = embedData);
            }

            await reaction.Message.Value.RemoveReactionAsync(reaction.Emote, reaction.User.Value);
        }
    }
}
