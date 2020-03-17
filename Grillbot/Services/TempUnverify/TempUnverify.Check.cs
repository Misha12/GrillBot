using Discord.WebSocket;
using Grillbot.Extensions.Discord;
using Grillbot.Services.Config.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Grillbot.Services.TempUnverify
{
    public partial class TempUnverifyService
    {
        public void CheckIfCanStartUnverify(List<SocketGuildUser> users, SocketGuild guild, bool self, string[] subjects)
        {
            if (self && subjects != null && subjects.Length > 0)
            {
                var config = ConfigRepository.FindConfig(guild.Id, "selfunverify", "").GetData<SelfUnverifyConfig>();

                if (subjects.Length > config.MaxSubjectsCount)
                    throw new ArgumentException($"Je možné si ponechat maximálně {config.MaxSubjectsCount} rolí.");

                foreach(var subject in subjects)
                {
                    if (!config.Subjects.Contains(subject.ToLower()))
                        throw new ArgumentException($"**{subject}** není předmět.");
                }
            }

            var owner = users.Find(o => o.Id == guild.OwnerId);

            if (owner != null)
                throw new ArgumentException("Nelze provést odebrání přístupu, protože se mezi uživateli nachází vlastník serveru.");

            var botMaxRolePosition = guild.CurrentUser.Roles.Max(o => o.Position);
            foreach (var user in users)
            {
                if (Data.Exists(o => o.UserID == user.Id.ToString()))
                    throw new ArgumentException($"Nelze provést odebrání rolí, protože uživatel **{user.GetFullName()}** již má odebraný přístup.");

                if (user.Id == guild.CurrentUser.Id)
                    throw new ArgumentException("Nelze provést odebrání přístupu, protože tagnutý uživatel jsem já.");

                var usersMaxRolePosition = user.Roles.Max(o => o.Position);

                if (usersMaxRolePosition > botMaxRolePosition && !self)
                {
                    var higherRoles = user.Roles.Where(o => o.Position > botMaxRolePosition).Select(o => o.Name);

                    throw new ArgumentException($"Nelze provést odebírání přístupu, protože uživatel **{user.GetFullName()}** má vyšší role " +
                        $"**({string.Join(", ", higherRoles)})**.");
                }

                if (Config.IsUserBotAdmin(user.Id) && !self)
                    throw new ArgumentException($"Nelze provést odebrání přístupu, protože uživatel **{user.GetFullName()}** je administrátor bota.");
            }
        }
    }
}
