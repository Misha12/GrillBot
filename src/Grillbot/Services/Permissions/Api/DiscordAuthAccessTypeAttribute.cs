using Grillbot.Enums;
using System;

namespace Grillbot.Services.Permissions.Api
{
    [AttributeUsage(AttributeTargets.All, AllowMultiple = false)]
    public class DiscordAuthAccessTypeAttribute : Attribute
    {
        public AccessType AccessType { get; set; }
    }
}
