using Discord;
using Discord.WebSocket;
using Grillbot.Services;
using Grillbot.Services.Initiable;
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
        private EmoteStats EmoteStats { get; }
        private InternalStatistics InternalStatistics { get; }
        private PaginationService PaginationService { get; }
        private UserService UserService { get; }
        private IServiceProvider Provider { get; }

        public ReactionAddedHandler(DiscordSocketClient client, EmoteStats emoteStats, InternalStatistics internalStatistics,
            PaginationService paginationService, UserService userService, IServiceProvider provider)
        {
            Client = client;
            EmoteStats = emoteStats;
            InternalStatistics = internalStatistics;
            PaginationService = paginationService;
            UserService = userService;
            Provider = provider;
        }

        private async Task OnReactionAddedAsync(Cacheable<IUserMessage, ulong> message, ISocketMessageChannel channel, SocketReaction reaction)
        {
            InternalStatistics.IncrementEvent("ReactionAdded");

            EmoteStats.IncrementFromReaction(reaction);
            await PaginationService.HandleReactionAsync(reaction);
            UserService.IncrementReaction(reaction);

            if(channel is SocketTextChannel textChannel)
            {
                IncrementPoints(textChannel.Guild, reaction);
            }
        }

        private void IncrementPoints(SocketGuild guild, SocketReaction reaction)
        {
            using var scope = Provider.CreateScope();
            using var pointsService = scope.ServiceProvider.GetService<PointsService>();

            pointsService.IncrementPoints(guild, reaction);
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