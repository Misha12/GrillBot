using System.ComponentModel.DataAnnotations;

namespace Grillbot.Enums
{
    public enum AuditLogType
    {
        [Display(Name = "Příkaz")]
        Command,

        [Display(Name = "Uživatel opustil server", GroupName = "Připojení a odpojení")]
        UserLeft,

        [Display(Name = "Uživatel se připojil na server", GroupName = "Připojení a odpojení")]
        UserJoined,

        [Display(Name = "Zpráva editována", GroupName = "Zprávy")]
        MessageEdited,

        [Display(Name = "Zpráva smazána", GroupName = "Zprávy")]
        MessageDeleted,

        [Display(Name = "Přidán bot", GroupName = "Uživatelé")]
        BotAdded,

        [Display(Name = "Vytvořen kanál", GroupName = "Kanály")]
        ChannelCreated,

        [Display(Name = "Upraven server")]
        GuildUpdated,

        [Display(Name = "Smazán kanál", GroupName = "Kanály")]
        ChannelDeleted,

        [Display(Name = "Upraven kanál", GroupName = "Kanály")]
        ChannelUpdated,

        [Display(Name = "Vytvořen emote", GroupName = "Emoty")]
        EmojiCreated,

        [Display(Name = "Smazán emote", GroupName = "Emoty")]
        EmojiDeleted,

        [Display(Name = "Upraven emote", GroupName = "Emoty")]
        EmojiUpdated,

        [Display(Name = "Vytvořena výjimka do kanálu", GroupName = "Oprávnění v kanálech")]
        OverwriteCreated,

        [Display(Name = "Smazána výjimka do kanálu", GroupName = "Oprávnění v kanálech")]
        OverwriteDeleted,

        [Display(Name = "Upravena výjimka do kanálu", GroupName = "Oprávnění v kanálech")]
        OverwriteUpdated,

        [Display(Name = "Hromadné vyhození neaktivních", GroupName = "Uživatelé")]
        Prune,

        [Display(Name = "Unban", GroupName = "Uživatelé")]
        Unban,

        [Display(Name = "Upraven uživatel", GroupName = "Uživatelé")]
        MemberUpdated,

        [Display(Name = "Vytvořena role", GroupName = "Role")]
        RoleCreated,

        [Display(Name = "Smazána role", GroupName = "Role")]
        RoleDeleted,

        [Display(Name = "Aktualizována role", GroupName = "Role")]
        RoleUpdated,

        [Display(Name = "Vytvořen webhook", GroupName = "Webhook")]
        WebhookCreated,

        [Display(Name = "Smazán webhook", GroupName = "Webhook")]
        WebhookDeleted,

        [Display(Name = "Aktualizován webhook", GroupName = "Webhook")]
        WebhookUpdated,

        [Display(Name = "Zpráva připnuta", GroupName = "Zprávy")]
        MessagePinned,

        [Display(Name = "Zpráva odepnuta", GroupName = "Zprávy")]
        MessageUnpinned,

        [Display(Name = "Změněny role uživatele", GroupName = "Uživatelé")]
        MemberRoleUpdated
    }
}
