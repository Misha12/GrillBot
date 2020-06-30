using Discord;
using Discord.WebSocket;
using Grillbot.Database.Entity.MethodConfig;
using Grillbot.Database.Enums;
using Grillbot.Exceptions;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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
            CorrectValue(ref group);
            CorrectValue(ref command);

            var query = GetBaseQuery(true);
            return query.FirstOrDefault(o => o.GuildID == guildID.ToString() && o.Group == group && o.Command == command);
        }

        public MethodsConfig AddConfig(SocketGuild guild, string group, string command, bool onlyAdmins, JObject json)
        {
            CorrectValue(ref group);
            CorrectValue(ref command);

            var entity = MethodsConfig.Create(guild, group, command, onlyAdmins, json);

            Context.MethodsConfig.Add(entity);
            Context.SaveChanges();

            return entity;
        }

        public List<MethodsConfig> GetAllMethods(SocketGuild guild)
        {
            var query = GetBaseQuery(false);
            return query.Where(o => o.GuildID == guild.Id.ToString()).ToList();
        }

        public MethodsConfig UpdateMethod(SocketGuild guild, int methodID, bool? onlyAdmins = null, JObject jsonConfig = null)
        {
            var item = GetBaseQuery(false)
                .FirstOrDefault(o => o.GuildID == guild.Id.ToString() && o.ID == methodID);

            if (item == null)
                throw new ArgumentException("Požadovaná metoda neexistuje");

            if (onlyAdmins != null)
                item.OnlyAdmins = onlyAdmins.Value;

            if (jsonConfig != null)
                item.Config = jsonConfig;

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
            var item = GetBaseQuery(true)
                .FirstOrDefault(o => o.GuildID == guild.Id.ToString() && o.ID == methodID);

            if (item == null)
                throw new ArgumentException("Požadovaná metoda neexistuje");

            return item;
        }

        public void RemovePermission(SocketGuild guild, int methodID, int permID)
        {
            var item = GetBaseQuery(true)
                .FirstOrDefault(o => o.GuildID == guild.Id.ToString() && o.ID == methodID);

            if (item == null)
                throw new ArgumentException("Požadovaná metoda neexistuje");

            var permission = item.Permissions.FirstOrDefault(o => o.PermID == permID);

            if (permission == null)
                throw new ArgumentException("Takové oprávnění neexistuje");

            item.Permissions.Remove(permission);
            Context.SaveChanges();
        }

        public void IncrementUsageCounter(IGuild guild, string group, string command)
        {
            CorrectValue(ref group);
            CorrectValue(ref command);

            var guildID = guild?.Id.ToString();

            var entity = GetBaseQuery(false)
                .FirstOrDefault(o => o.GuildID == guildID && o.Group == group && o.Command == command);

            if (entity == null)
            {
                entity = MethodsConfig.Create(guild, group, command, false, null);
                Context.MethodsConfig.Add(entity);
            }

            entity.UsedCount++;
            Context.SaveChanges();
        }

        public List<MethodsConfig> GetAllConfigurations()
        {
            return GetBaseQuery(true)
                .OrderByDescending(o => o.UsedCount)
                .ToList();
        }

        private void CorrectValue(ref string value)
        {
            if (value == null)
                value = "";
        }

        public void RemoveMethod(ulong guildID, int methodID)
        {
            var method = GetBaseQuery(true)
                .SingleOrDefault(o => o.GuildID == guildID.ToString() && o.ID == methodID);

            if (method == null)
                throw new NotFoundException($"Konfigurace pro metodu s ID `{methodID}` nebyla nalezena.");

            Context.MethodsConfig.Remove(method);
            Context.SaveChanges();
        }
    }
}
