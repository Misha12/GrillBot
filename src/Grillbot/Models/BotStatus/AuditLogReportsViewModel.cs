using Discord;
using Grillbot.Enums;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Grillbot.Models.BotStatus
{
    public class AuditLogReportsViewModel
    {
        public Dictionary<AuditLogType, int> PerTypeStats { get; set; }

        public AuditLogReportsViewModel()
        {
            PerTypeStats = Enum.GetValues<AuditLogType>().ToDictionary(o => o, _ => 0);
        }
    }
}
