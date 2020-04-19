using Discord.WebSocket;
using Grillbot.Database.Entity.MethodConfig;
using Grillbot.Database.Enums;
using Grillbot.Exceptions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Grillbot.Database.Repository
{
    public class ConfigRepository : RepositoryBase
    {
        public ConfigRepository(GrillBotContext context) : base(context)
        {
        }

        private IQueryable<MethodsConfig> GetBaseQuery(bool includePermissions)
        {
            var query = Context.MethodsConfig.AsQueryable();

            if (includePermissions)
                query = query.Include(o => o.Permissions);

            return query;
        }

        public MethodsConfig FindConfig(ulong guildID, string group, string command)
        {
            if (group == null) group = "";
            if (command == null) command = "";

            var query = GetBaseQuery(true);
            return query.FirstOrDefault(o => o.GuildID == guildID.ToString() && o.Group == group && o.Command == command);
        }

        public MethodsConfig AddConfig(SocketGuild guild, string group, string command, bool onlyAdmins, string jsonConfig)
        {
            var entity = new MethodsConfig()
            {
                Command = command,
                ConfigData = jsonConfig,
                Group = group,
                GuildID = guild.Id.ToString(),
                OnlyAdmins = onlyAdmins,
            };

            Context.Set<MethodsConfig>().Add(entity);
            Context.SaveChanges();

            return entity;
        }

        public List<MethodsConfig> GetAllMethods(SocketGuild guild)
        {
            var query = GetBaseQuery(false);
            return query.Where(o => o.GuildID == guild.Id.ToString()).ToList();
        }

        public MethodsConfig UpdateMethod(SocketGuild guild, int methodID, bool? onlyAdmins = null, string jsonConfig = null)
        {
            var item = GetBaseQuery(false).FirstOrDefault(o => o.GuildID == guild.Id.ToString() && o.ID == methodID);

            if (item == null)
                throw new ArgumentException("Požadovaná metoda neexistuje");

            if (onlyAdmins != null)
                item.OnlyAdmins = onlyAdmins.Value;

            if (!string.IsNullOrEmpty(jsonConfig))
                item.ConfigData = jsonConfig;

            Context.SaveChanges();
            return item;
        }

        public MethodsConfig AddPermission(SocketGuild guild, int methodID, string targetID, PermType permType, AllowType allowType)
        {
            var item = GetBaseQuery(true).FirstOrDefault(o => o.GuildID == guild.Id.ToString() && o.ID == methodID);

            if (item == null)
                throw new ArgumentException("Požadovaná metoda neexistuje.");

            item.Permissions.Add(new MethodPerm()
            {
                AllowType = allowType,
                DiscordID = targetID,
                PermType = permType
            });

            Context.SaveChanges();
            return item;
        }

        public MethodsConfig GetMethod(SocketGuild guild, int methodID)
        {
            var item = GetBaseQuery(true).FirstOrDefault(o => o.GuildID == guild.Id.ToString() && o.ID == methodID);

            if (item == null)
                throw new ArgumentException("Požadovaná metoda neexistuje");

            return item;
        }

        public void RemovePermission(SocketGuild guild, int methodID, int permID)
        {
            var item = GetBaseQuery(true).FirstOrDefault(o => o.GuildID == guild.Id.ToString() && o.ID == methodID);

            if (item == null)
                throw new ArgumentException("Požadovaná metoda neexistuje");

            var permission = item.Permissions.FirstOrDefault(o => o.PermID == permID);

            if (permission == null)
                throw new ArgumentException("Takové oprávnění neexistuje");

            item.Permissions.Remove(permission);
            Context.SaveChanges();
        }

        public void IncrementUsageCounter(SocketGuild guild, string group, string command)
        {
            if (group == null) group = "";
            if (command == null) command = "";
            var guildID = guild.Id.ToString();

            var entity = GetBaseQuery(false).FirstOrDefault(o => o.GuildID == guildID && o.Group == group && o.Command == command);

            if (entity == null)
                throw new NotFoundException($"Metoda {guildID}/{group}/{command} nebyla nalezena v konfiguraci.");

            entity.UsedCount++;
            Context.SaveChanges();
        }

        public List<MethodsConfig> GetAllConfigurations()
        {
            return GetBaseQuery(true).OrderByDescending(o => o.UsedCount).ToList();
        }
    }
}
