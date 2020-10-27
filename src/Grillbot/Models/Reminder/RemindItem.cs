using Discord.WebSocket;
using System;
using RemindEntity = Grillbot.Database.Entity.Users.Reminder;

namespace Grillbot.Models.Reminder
{
    public class RemindItem
    {
        public long ID { get; set; }
        public SocketGuildUser FromUser { get; set; }
        public DateTime At { get; set; }
        public string Message { get; set; }
        public int PostponeCounter { get; set; }
        public bool WasNotified { get; set; }

        public RemindItem(RemindEntity entity, SocketGuildUser fromUser)
        {
            ID = entity.RemindID;
            FromUser = fromUser;
            At = entity.At;
            Message = entity.Message;
            PostponeCounter = entity.PostponeCounter;
            WasNotified = !string.IsNullOrEmpty(entity.RemindMessageID);
        }
    }
}
