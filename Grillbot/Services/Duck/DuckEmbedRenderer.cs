using Discord;
using Grillbot.Extensions;
using Grillbot.Extensions.Discord;
using Grillbot.Models.Config.Dynamic;
using Grillbot.Models.Duck;
using Grillbot.Models.Embed;
using System;
using System.Text;

namespace Grillbot.Services.Duck
{
    public class DuckEmbedRenderer
    {
        private StringBuilder TitleBuilder { get; }

        public DuckEmbedRenderer()
        {
            TitleBuilder = new StringBuilder();
        }

        public BotEmbed RenderEmbed(CurrentState currentState, IUser responseFor, DuckConfig duckConfig)
        {
            var embed = new BotEmbed(responseFor, thumbnail: responseFor.GetUserAvatarUrl());

            switch (currentState.State)
            {
                case DuckState.Private:
                case DuckState.Closed:
                    FormatPrivateOrClosed(currentState, embed);
                    break;
                case DuckState.OpenBar:
                    FormatOpenBar(currentState, duckConfig.EnableBeersOnTap, embed);
                    break;
                case DuckState.OpenChillzone:
                    FormatOpenCillzone(currentState, embed);
                    break;
                case DuckState.OpenEvent:
                    FormatOpenEvent(currentState);
                    break;
            }

            return embed.WithTitle(TitleBuilder.ToString());
        }

        private void FormatPrivateOrClosed(CurrentState currentState, BotEmbed embed)
        {
            TitleBuilder.Append("Kachna je zavřená.");

            if (currentState.NextOpeningDateTime.HasValue)
            {
                FormatWithNextOpening(currentState, embed);
                return;
            }

            if(currentState.NextOpeningDateTime.HasValue && currentState.State != DuckState.Private)
            {
                FormatWithNextOpeningNoPrivate(currentState, embed);
                return;
            }

            TitleBuilder.Append(" Další otvíračka není naplánovaná.");
            AddNoteToEmbed(embed, currentState.Note);
        }

        private void FormatWithNextOpeningNoPrivate(CurrentState currentState, BotEmbed embed)
        {
            if(string.IsNullOrEmpty(currentState.Note))
            {
                embed.AddField("A co dál?",
                                $"Další otvíračka není naplánovaná, ale tento stav má skončit {currentState.NextStateDateTime:dd. MM. v HH:mm}. Co bude pak, to nikdo neví.",
                                false);

                return;
            }

            AddNoteToEmbed(embed, currentState.Note, "A co dál?");
        }

        private void FormatWithNextOpening(CurrentState currentState, BotEmbed embed)
        {
            var left = currentState.NextOpeningDateTime.Value - DateTime.Now;

            TitleBuilder
                .Append(" Do další otvíračky zbývá ")
                .Append(left.ToCzechLongTimeString())
                .Append('.');

            AddNoteToEmbed(embed, currentState.Note);
        }

        private void FormatOpenBar(CurrentState currentState, bool enableBeers, BotEmbed embed)
        {
            TitleBuilder.Append("Kachna je otevřená!");
            embed.AddField("Otevřeno", currentState.LastChange.ToString("HH:mm"), true);

            if (currentState.ExpectedEnd.HasValue)
            {
                var left = currentState.ExpectedEnd.Value - DateTime.Now;

                TitleBuilder.Append(" Do konce zbývá ").Append(left.ToCzechLongTimeString()).Append('.');
                embed.AddField("Zavíráme", currentState.ExpectedEnd.Value.ToString("HH:mm"), true);
            }

            if (enableBeers && currentState.BeersOnTap?.Length > 0)
            {
                var beerSb = new StringBuilder();
                foreach (var b in currentState.BeersOnTap)
                {
                    beerSb.AppendLine(b);
                }

                embed.AddField("Aktuálně na čepu", beerSb.ToString(), false);
            }

            AddNoteToEmbed(embed, currentState.Note);
        }

        private void FormatOpenCillzone(CurrentState currentState, BotEmbed embed)
        {
            TitleBuilder
                .Append("Kachna je otevřená v režimu chillzóna až do ")
                .AppendFormat("{0:HH:mm}", currentState.ExpectedEnd.Value)
                .Append('!');

            AddNoteToEmbed(embed, currentState.Note);
        }

        private void FormatOpenEvent(CurrentState currentState)
        {
            TitleBuilder
                .Append("V Kachně právě probíhá akce „")
                .Append(currentState.EventName)
                .Append("“.");
        }

        private void AddNoteToEmbed(BotEmbed embed, string note, string title = "Poznámka")
        {
            if (!string.IsNullOrEmpty(note))
            {
                embed.AddField(title, note, false);
            }
        }
    }
}
