using Discord;
using Discord.WebSocket;
using Grillbot.Database;
using Grillbot.Database.Enums.Includes;
using Grillbot.Services.MessageCache;
using System;
using System.Threading.Tasks;

namespace Grillbot.Services.UserManagement
{
    public class UserReactionsService
    {
        private IMessageCache MessageCache { get; }
        private IGrillBotRepository GrillBotRepository { get; }

        public UserReactionsService(IMessageCache messageCache, IGrillBotRepository grillBotRepository)
        {
            MessageCache = messageCache;
            GrillBotRepository = grillBotRepository;
        }

        public Task IncrementReactionStatsAsync(SocketReaction reaction)
        {
            return ProcessReactionAsync(reaction, IncrementReactionAsync);
        }

        public Task DecrementReactionStatsAsync(SocketReaction reaction)
        {
            return ProcessReactionAsync(reaction, DecrementReactionAsync);
        }

        private async Task ProcessReactionAsync(SocketReaction reaction, Func<SocketGuildUser, SocketGuildUser, Task> asyncMethod)
        {
            if (!reaction.User.IsSpecified || reaction.User.Value is not SocketGuildUser reactingUser)
                return;

            var message = reaction.Message.IsSpecified ? (IMessage)reaction.Message.Value : null;
            if (!reaction.Message.IsSpecified)
                message = await MessageCache.GetAsync(reaction.Channel.Id, reaction.MessageId);

            if (message == null || message.Author is not SocketGuildUser author)
                return;

            if (author.Id == reactingUser.Id)
                return; // Author add or remove reaction on self message.

            await asyncMethod(author, reactingUser);
        }

        private async Task IncrementReactionAsync(SocketGuildUser author, SocketGuildUser reactor)
        {
            var authorEntity = await GrillBotRepository.UsersRepository.GetOrCreateUserAsync(author.Guild.Id, author.Id, UsersIncludes.None);
            var reactorEntity = await GrillBotRepository.UsersRepository.GetOrCreateUserAsync(reactor.Guild.Id, reactor.Id, UsersIncludes.None);

            authorEntity.ObtainedReactionsCount++;
            reactorEntity.GivenReactionsCount++;

            await GrillBotRepository.CommitAsync();
        }

        private async Task DecrementReactionAsync(SocketGuildUser author, SocketGuildUser reactor)
        {
            var authorEntity = await GrillBotRepository.UsersRepository.GetUserAsync(author.Guild.Id, author.Id, UsersIncludes.None);

            if (authorEntity != null && authorEntity.ObtainedReactionsCount > 0)
                authorEntity.ObtainedReactionsCount--;

            var reactorEntity = await GrillBotRepository.UsersRepository.GetUserAsync(reactor.Guild.Id, reactor.Id, UsersIncludes.None);

            if (reactorEntity != null && reactorEntity.GivenReactionsCount > 0)
                reactorEntity.GivenReactionsCount--;

            await GrillBotRepository.CommitAsync();
        }
    }
}
