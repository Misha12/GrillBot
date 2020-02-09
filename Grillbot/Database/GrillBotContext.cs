using Grillbot.Database.Entity;
using Grillbot.Database.Entity.UnverifyLog;
using Microsoft.EntityFrameworkCore;

namespace Grillbot.Database
{
    public class GrillBotContext : DbContext
    {
        private string ConnectionString { get; }

        public GrillBotContext(string connectionString)
        {
            ConnectionString = connectionString;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
            optionsBuilder.UseSqlServer(ConnectionString);
        }

        public virtual DbSet<TeamSearch> TeamSearch { get; set; }
        public virtual DbSet<ChannelStat> ChannelStats { get; set; }
        public virtual DbSet<EmoteStat> EmoteStats { get; set; }
        public virtual DbSet<AutoReplyItem> AutoReply { get; set; }
        public virtual DbSet<TempUnverifyItem> TempUnverify { get; set; }
        public virtual DbSet<CommandLog> CommandLog { get; set; }
        public virtual DbSet<UnverifyLog> UnverifyLog { get; set; }
        public virtual DbSet<Birthday> Birthdays { get; set; }
    }
}
