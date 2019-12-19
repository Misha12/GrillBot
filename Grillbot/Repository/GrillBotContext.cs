using System.Data.Common;
using Grillbot.Repository.Entity;
using Grillbot.Repository.Entity.UnverifyLog;
using Microsoft.EntityFrameworkCore;

namespace Grillbot.Repository
{
    public class GrillBotContext : DbContext
    {
        private string ConnectionString { get; set; }

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
    }
}
