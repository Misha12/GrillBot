using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Grillbot.Database.Repository;
using Grillbot.Extensions;
using Grillbot.Extensions.Discord;
using Grillbot.Models.Duck;
using Grillbot.Models.Embed;
using Grillbot.Services;
using Grillbot.Services.Config.Models;
using Grillbot.Services.Preconditions;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace Grillbot.Modules
{
    [Name("Stav Kachny")]
    [Group("kachna")]
    [RequirePermissions]
    public class DuckModule : BotModuleBase
    {
        private BotLoggingService Logger { get; }

        public DuckModule(IOptions<Configuration> config, BotLoggingService logger,
            ConfigRepository repository) : base(config, repository)
        {
            Logger = logger;
        }

        [Command("", true)]
        public async Task GetDuckInfoAsync()
        {
            var config = GetMethodConfig<DuckConfig>("kachna", "");

            await DoAsync(async () =>
            {
                var client = new HttpClient
                {
                    BaseAddress = new Uri(config.IsKachnaOpenApiBase)
                };

                HttpResponseMessage resp;

                try
                {
                    resp = await client.GetAsync("api/duck/currentState");
                }
                catch (Exception ex)
                {
                    Logger.Write(LogSeverity.Error, "Request na IsKachnaOpen skončil špatně (nepodařilo se navázat spojení nebo jiná výjimka.) ", exception: ex);

                    throw new ArgumentException("Nepodařilo se zjistit stav Kachny. Zkus " +
                                                config.IsKachnaOpenApiBase);
                }

                if (!resp.IsSuccessStatusCode)
                {
                    Logger.Write(LogSeverity.Warning, $"Request na IsKachnaOpen skončil špatně (HTTP {(int)resp.StatusCode}).\n{await resp.Content.ReadAsStringAsync()}");

                    throw new ArgumentException("Nepodařilo se zjistit stav Kachny. Zkus " +
                                                config.IsKachnaOpenApiBase);
                }

                var json = await resp.Content.ReadAsStringAsync();
                var dto = JsonConvert.DeserializeObject<CurrentState>(json);
                var user = await Context.Guild.GetUserFromGuildAsync(Context.User.Id);
                var embed = MakeEmbed(dto, user, config.EnableBeersOnTap);
                await ReplyAsync(embed: embed.Build()).ConfigureAwait(false);
            }).ConfigureAwait(false);
        }

        private BotEmbed MakeEmbed(CurrentState state, IUser user, bool enableBeers)
        {
            var embed = new BotEmbed(user, null, null, user.GetUserAvatarUrl());
            var sb = new StringBuilder();

            switch (state.State)
            {
                case DuckState.Private:
                case DuckState.Closed:
                    sb.Append("Kachna je zavřená.");

                    if (state.NextOpeningDateTime.HasValue)
                    {
                        var left = state.NextOpeningDateTime.Value - DateTime.Now;
                        sb.Append(" Do další otvíračky zbývá ").Append(left.ToCzechLongTimeString()).Append('.');

                        if (!string.IsNullOrEmpty(state.Note))
                        {
                            embed.AddField("Poznámka", state.Note, false);
                        }
                    }
                    else if (state.NextStateDateTime.HasValue && state.State != DuckState.Private)
                    {
                        if (string.IsNullOrEmpty(state.Note))
                        {
                            embed.AddField("A co dál?",
                                $"Další otvíračka není naplánovaná, ale tento stav má skončit {state.NextStateDateTime:dd. MM. v HH:mm}. Co bude pak, to nikdo neví.",
                                false);
                        }
                        else
                        {
                            embed.AddField("A co dál?", state.Note, false);
                        }
                    }
                    else
                    {
                        sb.Append(" Další otvíračka není naplánovaná.");

                        if (!string.IsNullOrEmpty(state.Note))
                        {
                            embed.AddField("Poznámka", state.Note, false);
                        }
                    }

                    break;
                case DuckState.OpenBar:
                    sb.Append("Kachna je otevřená!");
                    embed.AddField("Otevřeno", state.LastChange.ToString("HH:mm"), true);

                    if (state.ExpectedEnd.HasValue)
                    {
                        var left = state.ExpectedEnd.Value - DateTime.Now;

                        sb.Append(" Do konce zbývá ").Append(left.ToCzechLongTimeString()).Append('.');
                        embed.AddField("Zavíráme", state.ExpectedEnd.Value.ToString("HH:mm"), true);
                    }

                    if (enableBeers && state.BeersOnTap?.Length > 0)
                    {
                        var beerSb = new StringBuilder();
                        foreach (var b in state.BeersOnTap)
                        {
                            beerSb.AppendLine(b);
                        }

                        embed.AddField("Aktuálně na čepu", beerSb.ToString(), false);
                    }

                    if (!string.IsNullOrEmpty(state.Note))
                    {
                        embed.AddField("Poznámka", state.Note, false);
                    }

                    break;
                case DuckState.OpenChillzone:
                    sb.Append("Kachna je otevřená v režimu chillzóna až do ").AppendFormat("{0:HH:mm}", state.ExpectedEnd.Value).Append('!');
                    if (!string.IsNullOrEmpty(state.Note))
                    {
                        embed.AddField("Poznámka", state.Note, false);
                    }

                    break;
                case DuckState.OpenEvent:
                    sb.Append("V Kachně právě probíhá akce „").Append(state.EventName).Append("“.");
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return embed.WithTitle(sb.ToString());
        }
    }
}