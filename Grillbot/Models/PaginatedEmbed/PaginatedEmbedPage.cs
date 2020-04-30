using Discord;
using System;
using System.Collections.Generic;

namespace Grillbot.Models.PaginatedEmbed
{
    public class PaginatedEmbedPage
    {
        public List<EmbedFieldBuilder> Fields { get; }
        public string Title { get; set; }

        public PaginatedEmbedPage(string title, List<EmbedFieldBuilder> fields = null)
        {
            Title = title;
            Fields = fields ?? new List<EmbedFieldBuilder>();
        }

        public void AddField(EmbedFieldBuilder builder)
        {
            if (Fields.Count == EmbedBuilder.MaxFieldCount)
                throw new ArgumentOutOfRangeException($"Maximium is 25 fields per page.");

            Fields.Add(builder);
        }
    }
}
