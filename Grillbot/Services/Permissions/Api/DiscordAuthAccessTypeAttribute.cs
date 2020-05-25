using System;

namespace Grillbot.Services.Permissions.Api
{
    public class DiscordAuthAccessTypeAttribute : Attribute
    {
        public AccessType AccessType { get; set; }
    }
}
