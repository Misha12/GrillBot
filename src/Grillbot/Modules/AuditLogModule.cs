using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Grillbot.Attributes;
using Grillbot.Extensions;
using Grillbot.Extensions.Discord;
using Grillbot.Services;
using Grillbot.Services.Audit;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.Xml;
using System.Threading.Tasks;

namespace Grillbot.Modules
{
    [Group("log")]
    [ModuleID(nameof(AuditLogModule))]
    [Name("Ovládání logování")]
    public class AuditLogModule : BotModuleBase
    {
        public AuditLogModule(IServiceProvider provider) : base(provider: provider)
        {
        }

        [Command("import")]
        [Summary("Importuje data z původní generace logování.")]
        public async Task ImportOldMessagesAsync(ISocketMessageChannel loggerChannel)
        {
            using (Context.Channel.EnterTypingState())
            {
                var infoMessage = await ReplyAsync("Příprava importu.");
                var progressBar = new ProgressBar();

                uint totalMessages = 0;
                uint toProcess = 0;

                try
                {
                    if (loggerChannel == null)
                        throw new ArgumentException("Nepodařilo se vyhledat kanál s logy.");

                    var toProcessMessages = new List<IMessage>();
                    var opts = new RequestOptions() { RetryMode = RetryMode.AlwaysRetry, Timeout = 30000 };
                    var messagesQuery = loggerChannel.GetMessagesAsync(int.MaxValue, options: opts);
                    await using var enumerator = messagesQuery.GetAsyncEnumerator();

                    while (await enumerator.MoveNextAsync())
                    {
                        try
                        {
                            var current = enumerator.Current;

                            totalMessages += (uint)current.Count;

                            var filterQuery = current.Where(o => o.Author.IsBot && o.Embeds.Count > 0 && (o.Source == MessageSource.Bot || o.Source == MessageSource.User));
                            toProcessMessages.AddRange(filterQuery);

                            toProcess = (uint)toProcessMessages.Count;
                        }
                        finally
                        {
                            var percentage = Math.Round((double)toProcess / totalMessages * 100.0, 2);
                            progressBar.Value = percentage;

                            await infoMessage.ModifyAsync(o => o.Content = $"Příprava importu\nBude importováno {toProcess.FormatWithSpaces()} / {totalMessages.FormatWithSpaces()} " +
                                                                           $"({percentage} %)\n" + progressBar.ToString());
                        }
                    }

                    await infoMessage.ModifyAsync(o => o.Content = $"Probíhá import\nZachyceno potenciálních logů: {toProcess.FormatWithSpaces()} / {totalMessages.FormatWithSpaces()} ({Math.Round((double)toProcess / totalMessages * 100.0, 2)} %)");

                    using var service = GetService<AuditService>();
                    var result = await service.Service.ImportLogsAsync(toProcessMessages, Context.Guild,
                        async imported =>
                        {
                            var percentage = Math.Round((double)imported / toProcess * 100, 2);
                            progressBar.Value = percentage;

                            await infoMessage.ModifyAsync(o => o.Content = $"Probíhá import\nZachyceno potenciálních logů: {toProcess.FormatWithSpaces()} / {totalMessages.FormatWithSpaces()} ({Math.Round((double)toProcess / totalMessages * 100.0, 2)} %)\n" +
                                                                           $"Importováno: {imported.FormatWithSpaces()} / {toProcess.FormatWithSpaces()} ({percentage} %)\n" +
                                                                           progressBar.ToString());
                        });

                    var msg = $"Import dokončen.\nNačteno: {toProcess.FormatWithSpaces()} / {totalMessages.FormatWithSpaces()} ({Math.Round((double)toProcess / totalMessages * 100.0, 2)} %)\n" +
                        $"Uloženo: {result.Item1.FormatWithSpaces()} / {result.Item2.FormatWithSpaces()} ({Math.Round((double)result.Item1 / result.Item2 * 100, 2)} %)\n" + progressBar.ToString();
                    await infoMessage.ModifyAsync(o => o.Content = msg);
                }
                catch (Exception ex)
                {
                    await infoMessage.ModifyAsync(o => o.Content = $"Import se nezdařil. Žádná data nebyla uložena.\n```{ex.GetFullMessage()}```");
                    throw;
                }
            }
        }
    }
}
