using Discord;
using System;

namespace Grillbot.Extensions.Discord
{
    public static class EmbedFieldBuilderExtensions
    {
        public static EmbedFieldBuilder SetValue(this EmbedFieldBuilder builder, string value)
        {
            if (string.IsNullOrEmpty(value))
                throw new ArgumentNullException(nameof(value));

            if (value.Length < EmbedFieldBuilder.MaxFieldValueLength)
                return builder.WithValue(value);

            if (value.StartsWith("```") && value.EndsWith("```"))
            {
                var tmp = value[3..]
                    .Substring(0, value.Length - 3)
                    .Substring(0, EmbedFieldBuilder.MaxFieldValueLength - 9);

                value = $"```{tmp}```";
            }
            else if (value.StartsWith("`") && value.EndsWith("`"))
            {
                var tmp = value[1..]
                    .Substring(0, value.Length - 1)
                    .Substring(0, EmbedFieldBuilder.MaxFieldValueLength - 5);

                value = $"`{tmp}...`";
            }
            else
            {
                value = value.Substring(0, EmbedFieldBuilder.MaxFieldValueLength - 3) + "...";
            }

            return builder.WithValue(value);
        }
    }
}
