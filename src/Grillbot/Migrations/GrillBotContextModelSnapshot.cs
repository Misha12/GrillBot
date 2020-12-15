﻿// <auto-generated />
using System;
using Grillbot.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Grillbot.Migrations
{
    [DbContext(typeof(GrillBotContext))]
    partial class GrillBotContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .UseIdentityColumns()
                .HasAnnotation("Relational:MaxIdentifierLength", 128)
                .HasAnnotation("ProductVersion", "5.0.0");

            modelBuilder.Entity("Grillbot.Database.Entity.AuditLog.AuditLogItem", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint")
                        .UseIdentityColumn();

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("datetime2");

                    b.Property<string>("DcAuditLogId")
                        .HasMaxLength(30)
                        .HasColumnType("nvarchar(30)");

                    b.Property<string>("GuildId")
                        .IsRequired()
                        .HasMaxLength(30)
                        .HasColumnType("nvarchar(30)");

                    b.Property<string>("JsonData")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("Type")
                        .HasColumnType("int");

                    b.Property<long?>("UserId")
                        .HasColumnType("bigint");

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.HasIndex(new[] { "DcAuditLogId" }, "IX_AuditLogs_DcAuditLogId");

                    b.HasIndex(new[] { "GuildId" }, "IX_AuditLogs_GuildId");

                    b.ToTable("AuditLogs");
                });

            modelBuilder.Entity("Grillbot.Database.Entity.AutoReplyItem", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .UseIdentityColumn();

                    b.Property<bool>("CaseSensitive")
                        .HasColumnType("bit");

                    b.Property<string>("ChannelID")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("CompareType")
                        .HasColumnType("int");

                    b.Property<string>("GuildID")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("IsDisabled")
                        .HasColumnType("bit");

                    b.Property<string>("MustContains")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("ReplyMessage")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("ID");

                    b.ToTable("AutoReply");
                });

            modelBuilder.Entity("Grillbot.Database.Entity.Config.GlobalConfigItem", b =>
                {
                    b.Property<string>("Key")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("Value")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Key");

                    b.ToTable("GlobalConfig");
                });

            modelBuilder.Entity("Grillbot.Database.Entity.ErrorLogItem", b =>
                {
                    b.Property<long>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint")
                        .UseIdentityColumn();

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("datetime2");

                    b.Property<string>("Data")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("ID");

                    b.ToTable("Errors");
                });

            modelBuilder.Entity("Grillbot.Database.Entity.File", b =>
                {
                    b.Property<string>("Filename")
                        .HasColumnType("nvarchar(450)");

                    b.Property<long?>("AuditLogItemId")
                        .HasColumnType("bigint");

                    b.Property<byte[]>("Content")
                        .HasColumnType("varbinary(max)");

                    b.HasKey("Filename");

                    b.HasIndex("AuditLogItemId");

                    b.ToTable("Files");
                });

            modelBuilder.Entity("Grillbot.Database.Entity.MethodConfig.MethodPerm", b =>
                {
                    b.Property<int>("PermID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .UseIdentityColumn();

                    b.Property<byte>("AllowType")
                        .HasColumnType("tinyint");

                    b.Property<string>("DiscordID")
                        .IsRequired()
                        .HasMaxLength(30)
                        .HasColumnType("nvarchar(30)");

                    b.Property<int>("MethodID")
                        .HasColumnType("int");

                    b.Property<byte>("PermType")
                        .HasColumnType("tinyint");

                    b.HasKey("PermID");

                    b.HasIndex("MethodID");

                    b.ToTable("MethodPerms");
                });

            modelBuilder.Entity("Grillbot.Database.Entity.MethodConfig.MethodsConfig", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .UseIdentityColumn();

                    b.Property<string>("Command")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)");

                    b.Property<string>("ConfigData")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Group")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)");

                    b.Property<string>("GuildID")
                        .IsRequired()
                        .HasMaxLength(30)
                        .HasColumnType("nvarchar(30)");

                    b.Property<bool>("OnlyAdmins")
                        .HasColumnType("bit");

                    b.Property<long>("UsedCount")
                        .HasColumnType("bigint");

                    b.HasKey("ID");

                    b.ToTable("MethodsConfig");
                });

            modelBuilder.Entity("Grillbot.Database.Entity.TeamSearch", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .UseIdentityColumn();

                    b.Property<string>("ChannelId")
                        .HasMaxLength(30)
                        .HasColumnType("nvarchar(30)");

                    b.Property<string>("GuildId")
                        .HasMaxLength(30)
                        .HasColumnType("nvarchar(30)");

                    b.Property<string>("MessageId")
                        .HasMaxLength(30)
                        .HasColumnType("nvarchar(30)");

                    b.Property<string>("UserId")
                        .HasMaxLength(30)
                        .HasColumnType("nvarchar(30)");

                    b.HasKey("Id");

                    b.ToTable("TeamSearch");
                });

            modelBuilder.Entity("Grillbot.Database.Entity.Unverify.Unverify", b =>
                {
                    b.Property<long>("UserID")
                        .HasColumnType("bigint");

                    b.Property<string>("Channels")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("EndDateTime")
                        .HasColumnType("datetime2");

                    b.Property<string>("Reason")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Roles")
                        .HasColumnType("nvarchar(max)");

                    b.Property<long?>("SetLogOperationID")
                        .HasColumnType("bigint");

                    b.Property<DateTime>("StartDateTime")
                        .HasColumnType("datetime2");

                    b.HasKey("UserID");

                    b.HasIndex("SetLogOperationID")
                        .IsUnique()
                        .HasFilter("[SetLogOperationID] IS NOT NULL");

                    b.ToTable("Unverifies");
                });

            modelBuilder.Entity("Grillbot.Database.Entity.Unverify.UnverifyLog", b =>
                {
                    b.Property<long>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint")
                        .UseIdentityColumn();

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("datetime2");

                    b.Property<long>("FromUserID")
                        .HasColumnType("bigint");

                    b.Property<string>("JsonData")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("Operation")
                        .HasColumnType("int");

                    b.Property<long>("ToUserID")
                        .HasColumnType("bigint");

                    b.HasKey("ID");

                    b.HasIndex("FromUserID");

                    b.HasIndex("ToUserID");

                    b.ToTable("UnverifyLogs");
                });

            modelBuilder.Entity("Grillbot.Database.Entity.Users.DiscordUser", b =>
                {
                    b.Property<long>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint")
                        .UseIdentityColumn();

                    b.Property<int?>("ApiAccessCount")
                        .HasColumnType("int");

                    b.Property<string>("ApiToken")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime?>("Birthday")
                        .HasColumnType("datetime2");

                    b.Property<long>("Flags")
                        .HasColumnType("bigint");

                    b.Property<long>("GivenReactionsCount")
                        .HasColumnType("bigint");

                    b.Property<string>("GuildID")
                        .HasMaxLength(30)
                        .HasColumnType("nvarchar(30)");

                    b.Property<long>("ObtainedReactionsCount")
                        .HasColumnType("bigint");

                    b.Property<long>("Points")
                        .HasColumnType("bigint");

                    b.Property<string>("UnverifyImunityGroup")
                        .HasMaxLength(64)
                        .HasColumnType("nvarchar(64)");

                    b.Property<string>("UsedInviteCode")
                        .HasColumnType("nvarchar(20)");

                    b.Property<string>("UserID")
                        .HasMaxLength(30)
                        .HasColumnType("nvarchar(30)");

                    b.Property<int?>("WebAdminLoginCount")
                        .HasColumnType("int");

                    b.Property<string>("WebAdminPassword")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("ID");

                    b.HasIndex("GuildID");

                    b.HasIndex("UsedInviteCode");

                    b.HasIndex("UserID");

                    b.ToTable("DiscordUsers");
                });

            modelBuilder.Entity("Grillbot.Database.Entity.Users.EmoteStatItem", b =>
                {
                    b.Property<string>("EmoteID")
                        .HasMaxLength(255)
                        .HasColumnType("nvarchar(255)");

                    b.Property<long>("UserID")
                        .HasColumnType("bigint");

                    b.Property<DateTime>("FirstOccuredAt")
                        .HasColumnType("datetime2");

                    b.Property<bool>("IsUnicode")
                        .HasColumnType("bit");

                    b.Property<DateTime>("LastOccuredAt")
                        .HasColumnType("datetime2");

                    b.Property<long>("UseCount")
                        .HasColumnType("bigint");

                    b.HasKey("EmoteID", "UserID");

                    b.HasIndex("UserID", "UseCount");

                    b.ToTable("EmoteStats");
                });

            modelBuilder.Entity("Grillbot.Database.Entity.Users.Invite", b =>
                {
                    b.Property<string>("Code")
                        .HasMaxLength(20)
                        .HasColumnType("nvarchar(20)");

                    b.Property<string>("ChannelId")
                        .IsRequired()
                        .HasMaxLength(30)
                        .HasColumnType("nvarchar(30)");

                    b.Property<DateTime?>("CreatedAt")
                        .HasColumnType("datetime2");

                    b.Property<long?>("CreatorId")
                        .HasColumnType("bigint");

                    b.HasKey("Code");

                    b.HasIndex("CreatorId");

                    b.ToTable("Invites");
                });

            modelBuilder.Entity("Grillbot.Database.Entity.Users.Reminder", b =>
                {
                    b.Property<long>("RemindID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint")
                        .UseIdentityColumn();

                    b.Property<DateTime>("At")
                        .HasColumnType("datetime2");

                    b.Property<long?>("FromUserID")
                        .HasColumnType("bigint");

                    b.Property<string>("Message")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("OriginalMessageID")
                        .IsRequired()
                        .HasMaxLength(30)
                        .HasColumnType("nvarchar(30)");

                    b.Property<int>("PostponeCounter")
                        .HasColumnType("int");

                    b.Property<string>("RemindMessageID")
                        .HasMaxLength(30)
                        .HasColumnType("nvarchar(30)");

                    b.Property<long>("UserID")
                        .HasColumnType("bigint");

                    b.HasKey("RemindID");

                    b.HasIndex("FromUserID");

                    b.HasIndex("UserID");

                    b.ToTable("Reminders");
                });

            modelBuilder.Entity("Grillbot.Database.Entity.Users.UserChannel", b =>
                {
                    b.Property<long>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint")
                        .UseIdentityColumn();

                    b.Property<string>("ChannelID")
                        .HasMaxLength(30)
                        .HasColumnType("nvarchar(30)");

                    b.Property<long>("Count")
                        .HasColumnType("bigint");

                    b.Property<DateTime>("LastMessageAt")
                        .HasColumnType("datetime2");

                    b.Property<long>("UserID")
                        .HasColumnType("bigint");

                    b.HasKey("ID");

                    b.HasIndex("UserID");

                    b.ToTable("UserChannels");
                });

            modelBuilder.Entity("Grillbot.Database.Entity.AuditLog.AuditLogItem", b =>
                {
                    b.HasOne("Grillbot.Database.Entity.Users.DiscordUser", "User")
                        .WithMany("AuditLogs")
                        .HasForeignKey("UserId");

                    b.Navigation("User");
                });

            modelBuilder.Entity("Grillbot.Database.Entity.File", b =>
                {
                    b.HasOne("Grillbot.Database.Entity.AuditLog.AuditLogItem", "AuditLogItem")
                        .WithMany("Files")
                        .HasForeignKey("AuditLogItemId");

                    b.Navigation("AuditLogItem");
                });

            modelBuilder.Entity("Grillbot.Database.Entity.MethodConfig.MethodPerm", b =>
                {
                    b.HasOne("Grillbot.Database.Entity.MethodConfig.MethodsConfig", "Method")
                        .WithMany("Permissions")
                        .HasForeignKey("MethodID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Method");
                });

            modelBuilder.Entity("Grillbot.Database.Entity.Unverify.Unverify", b =>
                {
                    b.HasOne("Grillbot.Database.Entity.Unverify.UnverifyLog", "SetLogOperation")
                        .WithOne("Unverify")
                        .HasForeignKey("Grillbot.Database.Entity.Unverify.Unverify", "SetLogOperationID");

                    b.HasOne("Grillbot.Database.Entity.Users.DiscordUser", "User")
                        .WithOne("Unverify")
                        .HasForeignKey("Grillbot.Database.Entity.Unverify.Unverify", "UserID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("SetLogOperation");

                    b.Navigation("User");
                });

            modelBuilder.Entity("Grillbot.Database.Entity.Unverify.UnverifyLog", b =>
                {
                    b.HasOne("Grillbot.Database.Entity.Users.DiscordUser", "FromUser")
                        .WithMany("OutgoingUnverifyOperations")
                        .HasForeignKey("FromUserID")
                        .OnDelete(DeleteBehavior.NoAction)
                        .IsRequired();

                    b.HasOne("Grillbot.Database.Entity.Users.DiscordUser", "ToUser")
                        .WithMany("IncomingUnverifyOperations")
                        .HasForeignKey("ToUserID")
                        .OnDelete(DeleteBehavior.NoAction)
                        .IsRequired();

                    b.Navigation("FromUser");

                    b.Navigation("ToUser");
                });

            modelBuilder.Entity("Grillbot.Database.Entity.Users.DiscordUser", b =>
                {
                    b.HasOne("Grillbot.Database.Entity.Users.Invite", "UsedInvite")
                        .WithMany("UsedUsers")
                        .HasForeignKey("UsedInviteCode");

                    b.Navigation("UsedInvite");
                });

            modelBuilder.Entity("Grillbot.Database.Entity.Users.EmoteStatItem", b =>
                {
                    b.HasOne("Grillbot.Database.Entity.Users.DiscordUser", "User")
                        .WithMany("UsedEmotes")
                        .HasForeignKey("UserID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("User");
                });

            modelBuilder.Entity("Grillbot.Database.Entity.Users.Invite", b =>
                {
                    b.HasOne("Grillbot.Database.Entity.Users.DiscordUser", "Creator")
                        .WithMany("CreatedInvites")
                        .HasForeignKey("CreatorId");

                    b.Navigation("Creator");
                });

            modelBuilder.Entity("Grillbot.Database.Entity.Users.Reminder", b =>
                {
                    b.HasOne("Grillbot.Database.Entity.Users.DiscordUser", "FromUser")
                        .WithMany()
                        .HasForeignKey("FromUserID");

                    b.HasOne("Grillbot.Database.Entity.Users.DiscordUser", "User")
                        .WithMany("Reminders")
                        .HasForeignKey("UserID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("FromUser");

                    b.Navigation("User");
                });

            modelBuilder.Entity("Grillbot.Database.Entity.Users.UserChannel", b =>
                {
                    b.HasOne("Grillbot.Database.Entity.Users.DiscordUser", "User")
                        .WithMany("Channels")
                        .HasForeignKey("UserID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("User");
                });

            modelBuilder.Entity("Grillbot.Database.Entity.AuditLog.AuditLogItem", b =>
                {
                    b.Navigation("Files");
                });

            modelBuilder.Entity("Grillbot.Database.Entity.MethodConfig.MethodsConfig", b =>
                {
                    b.Navigation("Permissions");
                });

            modelBuilder.Entity("Grillbot.Database.Entity.Unverify.UnverifyLog", b =>
                {
                    b.Navigation("Unverify");
                });

            modelBuilder.Entity("Grillbot.Database.Entity.Users.DiscordUser", b =>
                {
                    b.Navigation("AuditLogs");

                    b.Navigation("CreatedInvites");

                    b.Navigation("Channels");

                    b.Navigation("IncomingUnverifyOperations");

                    b.Navigation("OutgoingUnverifyOperations");

                    b.Navigation("Reminders");

                    b.Navigation("Unverify");

                    b.Navigation("UsedEmotes");
                });

            modelBuilder.Entity("Grillbot.Database.Entity.Users.Invite", b =>
                {
                    b.Navigation("UsedUsers");
                });
#pragma warning restore 612, 618
        }
    }
}
