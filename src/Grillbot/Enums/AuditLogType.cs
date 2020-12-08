using System.ComponentModel.DataAnnotations;

namespace Grillbot.Enums
{
    public enum AuditLogType
    {
        [Display(Name = "Příkaz")]
        Command,

        [Display(Name = "Uživatel opustil server")]
        UserLeft,

        [Display(Name = "Uživatel se připojil na server")]
        UserJoined,

        [Display(Name = "Zpráva editována")]
        MessageEdited,

        [Display(Name = "Zpráva smazána")]
        MessageDeleted,

        [Display(Name = "Přidán bot")]
        BotAdded,

        [Display(Name = "Vytvořen kanál")]
        ChannelCreated,

        [Display(Name = "Upraven server")]
        GuildUpdated,

        [Display(Name = "Smazán kanál")]
        ChannelDeleted,

        [Display(Name = "Upraven kanál")]
        ChannelUpdated,

        [Display(Name = "Vytvořen emote")]
        EmojiCreated,

        [Display(Name = "Smazán emote")]
        EmojiDeleted,

        [Display(Name = "Upraven emote")]
        EmojiUpdated,

        [Display(Name = "Vytvořena výjimka do kanálu")]
        OverwriteCreated,

        [Display(Name = "Smazána výjimka do kanálu")]
        OverwriteDeleted,

        [Display(Name = "Upravena výjimka do kanálu")]
        OverwriteUpdated,

        [Display(Name = "Hromadné vyhození neaktivních")]
        Prune,

        [Display(Name = "Unban")]
        Unban,

        [Display(Name = "Upraven uživatel")]
        MemberUpdated,

        [Display(Name = "Upraveny role uživatele")]
        MemberRoleUpdated,

        [Display(Name = "Vytvořena role")]
        RoleCreated,

        [Display(Name = "Smazána role")]
        RoleDeleted,

        [Display(Name = "Aktualizována role")]
        RoleUpdated,

        [Display(Name = "Vytvořen webhook")]
        WebhookCreated,

        [Display(Name = "Smazán webhook")]
        WebhookDeleted,

        [Display(Name = "Aktualizován webhook")]
        WebhookUpdated,

        [Display(Name = "Zpráva připnuta")]
        MessagePinned,

        [Display(Name = "Zpráva odepnuta")]
        MessageUnpinned
    }
}
