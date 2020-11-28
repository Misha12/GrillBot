using Discord;
using Discord.WebSocket;
using Grillbot.Extensions.Discord;
using Grillbot.Services;
using Grillbot.Services.Initiable;
using Grillbot.Services.Reminder;
using Grillbot.Services.Statistics;
using Grillbot.Services.UserManagement;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace Grillbot.Handlers
{
    public class ReactionAddedHandler : IDisposable, IInitiable
    {
        private DiscordSocketClient Client { get; }
        private InternalStatistics InternalStatistics { get; }
        private PaginationService PaginationService { get; }
        private IServiceProvider Provider { get; }

        public ReactionAddedHandler(DiscordSocketClient client, InternalStatistics internalStatistics, PaginationService paginationService,
            IServiceProvider provider)
        {
            Client = client;
            InternalStatistics = internalStatistics;
            PaginationService = paginationService;
            Provider = provider;
        }

        private async Task OnReactionAddedAsync(Cacheable<IUserMessage, ulong> message, ISocketMessageChannel channel, SocketReaction reaction)
        {
            InternalStatistics.IncrementEvent("ReactionAdded");

            using var scope = Provider.CreateScope();
            await scope.ServiceProvider.GetService<EmoteStats>().IncrementFromReactionAsync(reaction);

            if (reaction.User.IsSpecified && reaction.User.Value.IsUser())
            {
                await PaginationService.HandleReactionAsync(reaction);
                await scope.ServiceProvider.GetService<UserReactionsService>().IncrementReactionStatsAsync(reaction);
                await scope.ServiceProvider.GetService<ReminderService>().HandleRemindCopyAsync(reaction);

                if (channel is SocketTextChannel textChannel)
                    await scope.ServiceProvider.GetService<PointsService>().IncrementPointsAsync(textChannel.Guild, reaction);

                if (message.HasValue)
                    await scope.ServiceProvider.GetService<ReminderService>().PostponeReminderAsync(message.Value, reaction);
            }
        }

        public void Dispose()
        {
            Client.ReactionAdded -= OnReactionAddedAsync;
        }

        public void Init()
        {
            Client.ReactionAdded += OnReactionAddedAsync;
        }

        public async Task InitAsync() { }
    }
}