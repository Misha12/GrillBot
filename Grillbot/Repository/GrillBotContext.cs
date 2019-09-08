using Grillbot.Repository.Entity;
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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }
        public virtual DbSet<TeamSearch> TeamSearch { get; set; }
        public virtual DbSet<ChannelStat> ChannelStats { get; set; }
        public virtual DbSet<EmoteStat> EmoteStats { get; set; }
    }
}
