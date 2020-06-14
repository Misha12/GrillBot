using Discord;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Grillbot.Models.PaginatedEmbed
{
    public class PaginatedEmbedPage
    {
        public List<EmbedFieldBuilder> Fields { get; }
        public string Thumbnail { get; set; }
        public string Title { get; set; }

        public PaginatedEmbedPage(string title, List<EmbedFieldBuilder> fields = null, string thumbnail = null)
        {
            Title = title;
            Fields = fields ?? new List<EmbedFieldBuilder>();
            Thumbnail = thumbnail;
        }

        public void AddField(EmbedFieldBuilder builder)
        {
            if (Fields.Count == EmbedBuilder.MaxFieldCount)
                throw new ArgumentOutOfRangeException($"Maximium is 25 fields per page.");

            Fields.Add(builder);
        }

        public void AddField(string name, string value, bool inline = false)
        {
            var builder = new EmbedFieldBuilder()
                .WithName(name)
                .WithValue(value)
                .WithIsInline(inline);

            AddField(builder);
        }

        public void AddFields(IEnumerable<EmbedFieldBuilder> fields)
        {
            if(Fields.Count + fields.Count() >= EmbedBuilder.MaxFieldCount)
                throw new ArgumentOutOfRangeException($"Maximium is 25 fields per page.");

            Fields.AddRange(fields);
        }

        public bool AnyField()
        {
            return Fields.Count > 0;
        }
    }
}
