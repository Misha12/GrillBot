using Discord;
using Discord.WebSocket;
using Grillbot.Database.Enums.Includes;
using Grillbot.Database.Repository;
using Grillbot.Extensions.Discord;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;

namespace Grillbot.Services.UserManagement
{
    public class PointsService : IDisposable
    {
        private UsersRepository UsersRepository { get; }
        private ILogger<PointsService> Logger { get; }
        private BotState BotState { get; }
        private Random Random { get; }
        private PointsRenderService Renderer { get; }

        public PointsService(UsersRepository usersRepository, ILogger<PointsService> logger, BotState botState, PointsRenderService renderer)
        {
            UsersRepository = usersRepository;
            Logger = logger;
            BotState = botState;
            Random = new Random();
            Renderer = renderer;
        }

        public async Task<Bitmap> GetPointsAsync(IGuild guild, IUser user)
        {
            var userEntity = UsersRepository.GetUser(guild.Id, user.Id, UsersIncludes.None);

            if (userEntity == null)
                return null;

            var position = UsersRepository.CalculatePointsPosition(guild.Id, userEntity.ID) + 1;
            return await Renderer.RenderAsync(user, position, userEntity.Points);
        }

        public void GivePoints(IUser fromUser, IUser toUser, IGuild guild, long amount)
        {
            Logger.LogInformation($"User {fromUser.GetFullName()} gives {toUser.GetFullName()} {amount} points in guild {guild}");
            var userEntity = UsersRepository.GetOrCreateUser(guild.Id, toUser.Id, UsersIncludes.None);

            userEntity.Points += amount;
            UsersRepository.SaveChanges();
        }

        public long TransferPoints(IGuild guild, IUser from, IUser to, long amount = -1)
        {
            if (from == to)
                throw new InvalidOperationException("Nelze převést body mezi stejnými účty.");

            var fromUserEntity = UsersRepository.GetUser(guild.Id, from.Id, UsersIncludes.None);

            if (fromUserEntity == null)
                throw new InvalidOperationException("Nelze převést body z účtu, který ještě neexistuje v databázi.");

            var toUserEntity = UsersRepository.GetOrCreateUser(guild.Id, to.Id, UsersIncludes.None);

            var transferedPoints = amount > 0 ? System.Math.Min(amount, fromUserEntity.Points) : fromUserEntity.Points;

            Logger.LogInformation($"{from.GetFullName()} transfered {transferedPoints} points to {to.GetFullName()} in guild {guild}");

            toUserEntity.Points += transferedPoints;
            fromUserEntity.Points -= transferedPoints;

            UsersRepository.SaveChanges();
            return transferedPoints;
        }

        public List<Tuple<ulong, long, int>> GetPointsLeaderboard(IGuild guild, bool asc = false, int page = 1)
        {
            const int limit = 10;

            var skip = (page <= 1 ? 0 : page - 1) * limit;
            var users = UsersRepository.GetUsersWithPointsOrder(guild.Id, skip, limit, asc).ToList();
            return users.Select((o, i) => new Tuple<ulong, long, int>(o.UserIDSnowflake, o.Points, skip + i + 1)).ToList();
        }

        public void IncrementPoints(SocketGuild guild, SocketReaction reaction)
        {
            if (!reaction.User.IsSpecified || !CanIncrementPoints(guild, reaction.User.Value, 0.5d))
                return;

            var user = UsersRepository.GetOrCreateUser(guild.Id, reaction.UserId, UsersIncludes.None);
            user.Points += Random.Next(0, 10);

            UsersRepository.SaveChanges();
            UpdateLastCalculation(guild, reaction.User.Value, 0.5d);
        }

        public void IncrementPoints(SocketGuild guild, SocketMessage message)
        {
            if (!CanIncrementPoints(guild, message.Author, 1.0d) || string.IsNullOrEmpty(message.Content) || message.Content.Length < 5)
                return;

            var user = UsersRepository.GetOrCreateUser(guild.Id, message.Author.Id, UsersIncludes.None);
            user.Points += Random.Next(15, 25);

            UsersRepository.SaveChanges();
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

            if (BotState.LastPointsCalculation.ContainsKey(key))
                BotState.LastPointsCalculation[key] = DateTime.Now;
            else
                BotState.LastPointsCalculation.Add(key, DateTime.Now);

        }

        public void Dispose()
        {
            UsersRepository.Dispose();
        }
    }
}
