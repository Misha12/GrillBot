using Discord;
using Discord.WebSocket;
using Grillbot.Database;
using Grillbot.Database.Enums.Includes;
using Grillbot.Extensions.Discord;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;

namespace Grillbot.Services.UserManagement
{
    public class PointsService
    {
        private ILogger<PointsService> Logger { get; }
        private BotState BotState { get; }
        private Random Random { get; }
        private PointsRenderService Renderer { get; }
        private IGrillBotRepository GrillBotRepository { get; }

        public PointsService(IGrillBotRepository grillBotRepository, ILogger<PointsService> logger, BotState botState, PointsRenderService renderer)
        {
            GrillBotRepository = grillBotRepository;
            Logger = logger;
            BotState = botState;
            Random = new Random();
            Renderer = renderer;
        }

        public async Task<Bitmap> GetPointsAsync(IGuild guild, IUser user)
        {
            var userEntity = await GrillBotRepository.UsersRepository.GetUserAsync(guild.Id, user.Id, UsersIncludes.None);

            if (userEntity == null)
                return null;

            var position = await GrillBotRepository.UsersRepository.CalculatePointsPositionAsync(guild.Id, userEntity.ID) + 1;
            return await Renderer.RenderAsync(user, position, userEntity.Points);
        }

        public async Task GivePointsAsync(IUser fromUser, IUser toUser, IGuild guild, long amount)
        {
            Logger.LogInformation($"User {fromUser.GetFullName()} gives {toUser.GetFullName()} {amount} points in guild {guild}");
            var userEntity = await GrillBotRepository.UsersRepository.GetOrCreateUserAsync(guild.Id, toUser.Id, UsersIncludes.None);

            userEntity.Points += amount;
            await GrillBotRepository.CommitAsync();
        }

        public async Task<long> TransferPointsAsync(IGuild guild, IUser from, IUser to, long amount = -1)
        {
            if (from == to)
                throw new InvalidOperationException("Nelze převést body mezi stejnými účty.");

            var fromUserEntity = await GrillBotRepository.UsersRepository.GetUserAsync(guild.Id, from.Id, UsersIncludes.None);

            if (fromUserEntity == null)
                throw new InvalidOperationException("Nelze převést body z účtu, který ještě neexistuje v databázi.");

            var toUserEntity = await GrillBotRepository.UsersRepository.GetOrCreateUserAsync(guild.Id, to.Id, UsersIncludes.None);

            var transferedPoints = amount > 0 ? System.Math.Min(amount, fromUserEntity.Points) : fromUserEntity.Points;

            Logger.LogInformation($"{from.GetFullName()} transfered {transferedPoints} points to {to.GetFullName()} in guild {guild}");

            toUserEntity.Points += transferedPoints;
            fromUserEntity.Points -= transferedPoints;

            await GrillBotRepository.CommitAsync();
            return transferedPoints;
        }

        public async Task<List<Tuple<ulong, long, int>>> GetPointsLeaderboardAsync(IGuild guild, bool asc = false, int page = 1)
        {
            const int limit = 10;

            var skip = (page <= 1 ? 0 : page - 1) * limit;
            var users = await GrillBotRepository.UsersRepository.GetUsersWithPointsOrder(guild.Id, skip, limit, asc).ToListAsync();
            return users.Select((o, i) => new Tuple<ulong, long, int>(o.UserIDSnowflake, o.Points, skip + i + 1)).ToList();
        }

        public async Task IncrementPointsAsync(SocketGuild guild, SocketReaction reaction)
        {
            if (!reaction.User.IsSpecified || !CanIncrementPoints(guild, reaction.User.Value, 0.5d))
                return;

            var user = await GrillBotRepository.UsersRepository.GetOrCreateUserAsync(guild.Id, reaction.UserId, UsersIncludes.None);
            user.Points += Random.Next(0, 10);

            await GrillBotRepository.CommitAsync();
            UpdateLastCalculation(guild, reaction.User.Value, 0.5d);
        }

        public async Task IncrementPointsAsync(SocketGuild guild, SocketMessage message)
        {
            if (!CanIncrementPoints(guild, message.Author, 1.0d) || string.IsNullOrEmpty(message.Content) || message.Content.Length < 5)
                return;

            var user = await GrillBotRepository.UsersRepository.GetOrCreateUserAsync(guild.Id, message.Author.Id, UsersIncludes.None);
            user.Points += Random.Next(15, 25);

            await GrillBotRepository.CommitAsync();
            UpdateLastCalculation(guild, message.Author, 1.0d);
        }

        private bool CanIncrementPoints(IGuild guild, IUser user, double limit)
        {
            var key = $"{guild.Id}|{user.Id}|{limit}";

            if (!BotState.LastPointsCalculation.ContainsKey(key))
                return true;

            var lastMessageAt = BotState.LastPointsCalculation[key];
            return (DateTime.Now - lastMessageAt).TotalMinutes >= limit;
        }

        private void UpdateLastCalculation(IGuild guild, IUser user, double limit)
        {
            var key = $"{guild.Id}|{user.Id}|{limit}";

            BotState.LastPointsCalculation[key] = DateTime.Now;
        }
    }
}
