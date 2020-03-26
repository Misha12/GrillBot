using Discord.WebSocket;
using Grillbot.Database.Repository;
using Grillbot.Extensions.Discord;
using Grillbot.Messages;
using Grillbot.Services.Config.Models;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Grillbot.Services.TempUnverify
{
    public class TempUnverifyChecker
    {
        private ConfigRepository Repository { get; }
        private Configuration Config { get; }

        public TempUnverifyChecker(ConfigRepository repository, IOptions<Configuration> options)
        {
            Repository = repository;
            Config = options.Value;
        }

        public void Validate(SocketGuildUser user, SocketGuild guild, bool selfunverify, List<string> subjects, List<ulong> currentUnverifiedPersons)
        {
            if(selfunverify && subjects != null && subjects.Count > 0)
            {
                var config = Repository.FindConfig(guild.Id, "selfunverify", "").GetData<SelfUnverifyConfig>();

                if (subjects.Count > config.MaxSubjectsCount)
                    throw new ArgumentException(string.Format(UnverifyMessages.SubjectsOverMaximum, config.MaxSubjectsCount));

                foreach(var subject in subjects.Select(o => o.Trim().ToLower()))
                {
                    if (!config.Subjects.Contains(subject))
                        throw new ArgumentException(string.Format(UnverifyMessages.InvalidSubjectRole, subject));
                }
            }

            if (user.Id == guild.OwnerId)
                throw new ArgumentException(UnverifyMessages.ServerOwner);

            if (currentUnverifiedPersons.Any(o => o == user.Id))
                throw new ArgumentException(string.Format(UnverifyMessages.UserHaveUnverify, user.GetFullName()));

            var botRolePosition = guild.CurrentUser.Roles.Max(o => o.Position);
            var userMaxRolePosition = user.Roles.Max(o => o.Position);

            if(userMaxRolePosition > botRolePosition && !selfunverify)
            {
                var higherRoles = user.Roles.Where(o => o.Position > botRolePosition);
                var higherRoleNames = string.Join(", ", higherRoles.Select(o => o.Name));

                throw new ArgumentException(string.Format(UnverifyMessages.UserHaveHigherRoles, user.GetFullName(), higherRoleNames));
            }

            if (Config.IsUserBotAdmin(user.Id) && !selfunverify)
                throw new ArgumentException(string.Join(UnverifyMessages.BotAdmin, user.GetFullName()));
        }
    }
}
