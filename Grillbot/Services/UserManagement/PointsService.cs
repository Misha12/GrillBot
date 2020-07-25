﻿using Discord;
using Discord.WebSocket;
using Grillbot.Database.Repository;
using Grillbot.Extensions.Discord;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Grillbot.Services.UserManagement
{
    public class PointsService : IDisposable
    {
        private UsersRepository UsersRepository { get; }
        private ILogger<PointsService> Logger { get; }
        private BotState BotState { get; }
        private Random Random { get; }

        public PointsService(UsersRepository usersRepository, ILogger<PointsService> logger, BotState botState)
        {
            UsersRepository = usersRepository;
            Logger = logger;
            BotState = botState;
            Random = new Random();
        }

        public Tuple<long, int> GetPoints(IGuild guild, IUser user)
        {
            var userEntity = UsersRepository.GetUser(guild.Id, user.Id, false, false, false, false, false);

            if (userEntity == null)
                return null;

            var position = UsersRepository.CalculatePointsPosition(guild.Id, userEntity.Points) + 1;
            return new Tuple<long, int>(userEntity.Points, position);
        }

        public void GivePoints(IUser fromUser, IUser toUser, IGuild guild, long amount)
        {
            Logger.LogInformation($"User {fromUser.GetFullName()} gives {toUser.GetFullName()} {amount} points in guild {guild}");
            var userEntity = UsersRepository.GetOrCreateUser(guild.Id, toUser.Id, false, false, false, false, false);

            userEntity.Points += amount;
            UsersRepository.SaveChanges();
        }

        public long TransferPoints(IGuild guild, IUser from, IUser to, long amount = -1)
        {
            if (from == to)
                throw new InvalidOperationException("Nelze převést body mezi stejnými účty.");

            var fromUserEntity = UsersRepository.GetUser(guild.Id, from.Id, false, false, false, false, false);

            if (fromUserEntity == null)
                throw new InvalidOperationException("Nelze převést body z účtu, který ještě neexistuje v databázi.");

            var toUserEntity = UsersRepository.GetOrCreateUser(guild.Id, to.Id, false, false, false, false, false);

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

            var user = UsersRepository.GetOrCreateUser(guild.Id, reaction.UserId, false, false, false, false, false);
            user.Points += Random.Next(0, 5);

            UsersRepository.SaveChanges();
            UpdateLastCalculation(guild, reaction.User.Value, 0.5d);
        }

        public void IncrementPoints(SocketGuild guild, SocketMessage message)
        {
            if (!CanIncrementPoints(guild, message.Author, 1.0d))
                return;

            var user = UsersRepository.GetOrCreateUser(guild.Id, message.Author.Id, false, false, false, false, false);
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
