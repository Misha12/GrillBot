using Discord;
using System.Collections.Generic;

namespace Grillbot.Models.Embed.PaginatedEmbed
{
    public class PaginatedEmbed
    {
        public List<PaginatedEmbedPage> Pages { get; set; }
        public int CurrentPage { get; set; } = 1;

        public Color? Color { get; set; }
        public IUser ResponseFor { get; set; }
        public string Thumbnail { get; set; }
        public string Title { get; set; }

        public Discord.Embed RenderEmbed()
        {
            if (CurrentPage > Pages.Count)
                return null;

            var page = Pages[CurrentPage - 1];
            var botEmbed = new BotEmbed(ResponseFor, Color, page.Title, Thumbnail ?? page.Thumbnail)
                .WithFields(page.Fields)
                .PrependFooter($"Strana {CurrentPage}/{Pages.Count}");

            if (!string.IsNullOrEmpty(Title))
                botEmbed.WithAuthor(Title);

            return botEmbed.Build();
        }

        public bool NextPage()
        {
            if (CurrentPage + 1 > Pages.Count)
                return false;

            CurrentPage++;
            return true;
        }

        public bool PrevPage()
        {
            if (CurrentPage - 1 < 1)
                return false;

            CurrentPage--;
            return true;
        }

        public bool FirstPage()
        {
            if (CurrentPage == 1)
                return false;

            CurrentPage = 1;
            return true;
        }

        public bool LastPage()
        {
            if (CurrentPage == Pages.Count)
                return false;

            CurrentPage = Pages.Count;
            return true;
        }
    }
}
