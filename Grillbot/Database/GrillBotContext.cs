using Grillbot.Database.Entity;
using Grillbot.Database.Entity.Config;
using Grillbot.Database.Entity.Math;
using Grillbot.Database.Entity.MethodConfig;
using Grillbot.Database.Entity.UnverifyLog;
using Grillbot.Database.Entity.Users;
using Grillbot.Models.Reminder;
using Microsoft.EntityFrameworkCore;

namespace Grillbot.Database
{
    public class GrillBotContext : DbContext
    {
        public GrillBotContext(DbContextOptions<GrillBotContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<MethodsConfig>().HasMany(o => o.Permissions).WithOne(o => o.Method);

            modelBuilder.Entity<DiscordUser>(builder =>
            {
                builder
                    .HasMany(o => o.Channels)
                    .WithOne(o => o.User);

                builder
                    .HasIndex(o => o.UserID);

                builder
                    .HasOne(o => o.Birthday)
                    .WithOne(o => o.User);

                builder
                    .HasMany(o => o.MathAudit)
                    .WithOne(o => o.User);

                builder
                    .HasOne(o => o.Statistics)
                    .WithOne(o => o.User);

                builder
                    .HasMany(o => o.Reminders)
                    .WithOne(o => o.User);
            });

            modelBuilder.Entity<UserChannel>(builder =>
            {
                builder
                    .HasIndex(o => o.UserID);

                builder
                    .HasIndex(o => o.DiscordUserID);
            });

            modelBuilder.Entity<EmoteStat>(builder =>
            {
                builder
                    .HasKey(o => new { o.GuildID, o.EmoteID });
            });

            modelBuilder.Entity<Reminder>(builder =>
            {
                builder
                    .HasOne(o => o.FromUser);
            });
        }

        public virtual DbSet<TeamSearch> TeamSearch { get; set; }
        public virtual DbSet<EmoteStat> EmoteStats { get; set; }
        public virtual DbSet<AutoReplyItem> AutoReply { get; set; }
        public virtual DbSet<TempUnverifyItem> TempUnverify { get; set; }
        public virtual DbSet<UnverifyLog> UnverifyLog { get; set; }
        public virtual DbSet<MethodsConfig> MethodsConfig { get; set; }
        public virtual DbSet<DiscordUser> Users { get; set; }
        public virtual DbSet<UserChannel> UserChannels { get; set; }
        public virtual DbSet<MathAuditLogItem> MathAuditLogs { get; set; }
        public virtual DbSet<GlobalConfigItem> GlobalConfig { get; set; }
        public virtual DbSet<Reminder> Reminders { get; set; }
    }
}
