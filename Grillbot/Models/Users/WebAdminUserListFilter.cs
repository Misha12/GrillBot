﻿using Grillbot.Enums;

namespace Grillbot.Models.Users
{
    public class WebAdminUserListFilter
    {
        public ulong? GuildID { get; set; }
        public ulong? UserID { get; set; }
        public int Limit { get; set; } = 25;
        public WebAdminUserOrder Order { get; set; } = WebAdminUserOrder.Points;
        public bool SortDesc { get; set; } = true;
    }
}
