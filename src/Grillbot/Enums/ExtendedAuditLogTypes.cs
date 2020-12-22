using System;
using System.Collections.Generic;
using System.Linq;

namespace Grillbot.Enums
{
    public class ExtendedAuditLogTypes
    {
        private static ExtendedAuditLogTypes _instance;
        public static ExtendedAuditLogTypes Instance => _instance ??= new ExtendedAuditLogTypes();

        public Dictionary<string, Tuple<string, string>> Types { get; }

        public ExtendedAuditLogTypes()
        {
            Types = new Dictionary<string, Tuple<string, string>>();

            Add("Připojení a odpojení", null, AuditLogType.UserJoined, AuditLogType.UserLeft);
            Add("Správa kanálů", "Vytvoření, Aktualizace, Smazání", AuditLogType.ChannelCreated, AuditLogType.ChannelDeleted, AuditLogType.ChannelUpdated);
            Add("Emoty", "Vytvoření, Aktualizace, Smazání", AuditLogType.EmojiCreated, AuditLogType.EmojiDeleted, AuditLogType.EmojiUpdated);
            Add("Oprávnění v kanálech", "Vytvoření, Aktualizace, Smazání", AuditLogType.OverwriteCreated, AuditLogType.OverwriteDeleted, AuditLogType.OverwriteUpdated);
            Add("Role", "Vytvoření, Aktualizace, Smazání", AuditLogType.RoleUpdated, AuditLogType.RoleDeleted, AuditLogType.RoleCreated);
            Add("Webhook", "Vytvoření, Aktualizace, Smazání", AuditLogType.WebhookCreated, AuditLogType.WebhookDeleted, AuditLogType.WebhookUpdated);
        }

        public void Add(string description, string title, params AuditLogType[] types)
        {
            var key = string.Join("|", types.Select(o => (int)o));
            var value = Tuple.Create(description, title);

            if (Types.ContainsKey(key))
                Types[key] = value;

            Types.Add(key, value);
        }
    }
}
