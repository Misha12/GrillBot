using Discord.WebSocket;
using Grillbot.Database.Repository;
using Grillbot.Exceptions;
using Grillbot.Extensions.Discord;
using Grillbot.Models.Config.AppSettings;
using Grillbot.Models.Config.Dynamic;
using Microsoft.Extensions.Options;
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
            if(selfunverify && subjects?.Count > 0)
            {
                var config = Repository.FindConfig(guild.Id, "selfunverify", "").GetData<SelfUnverifyConfig>();

                if (subjects.Count > config.MaxSubjectsCount)
                    throw new BotCommandInfoException($"Je možné si ponechat maximálně {config.MaxSubjectsCount} rolí.");

                foreach(var subject in subjects.Select(o => o.ToLower()))
                {
                    if (!config.Subjects.Contains(subject))
                        throw new BotCommandInfoException($"`{subject}` není předmětová role.");
                }
            }

            if (user.Id == guild.OwnerId)
                throw new BotCommandInfoException("Nelze provést odebrání přístupu, protože se mezi uživateli nachází vlastník serveru.");

            if (currentUnverifiedPersons.Any(o => o == user.Id))
                throw new BotCommandInfoException($"Nelze provést odebrání přístupu, protože uživatel **{user.GetFullName()}** již má odebraný přístup.");

            var botRolePosition = guild.CurrentUser.Roles.Max(o => o.Position);
            var userMaxRolePosition = user.Roles.Max(o => o.Position);

            if(userMaxRolePosition > botRolePosition && !selfunverify)
            {
                var higherRoles = user.Roles.Where(o => o.Position > botRolePosition);
                var higherRoleNames = string.Join(", ", higherRoles.Select(o => o.Name));

                throw new BotCommandInfoException($"Nelze provést odebírání přístupu, protože uživatel **{user.GetFullName()}** má vyšší role. **({higherRoleNames})**");
            }

            if (Config.IsUserBotAdmin(user.Id) && !selfunverify)
                throw new BotCommandInfoException($"Nelze provést odebrání přístupu, protože uživatel **{user.GetFullName()}** je administrátor bota.");
        }
    }
}
