using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Grillbot.Migrations
{
    public partial class InitialMigration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AutoReply",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MustContains = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ReplyMessage = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsDisabled = table.Column<bool>(type: "bit", nullable: false),
                    CompareType = table.Column<int>(type: "int", nullable: false),
                    CaseSensitive = table.Column<bool>(type: "bit", nullable: false),
                    GuildID = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ChannelID = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AutoReply", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "Errors",
                columns: table => new
                {
                    ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Data = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Errors", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "GlobalConfig",
                columns: table => new
                {
                    Key = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GlobalConfig", x => x.Key);
                });

            migrationBuilder.CreateTable(
                name: "MethodsConfig",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GuildID = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Group = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Command = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ConfigData = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OnlyAdmins = table.Column<bool>(type: "bit", nullable: false),
                    UsedCount = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MethodsConfig", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "TeamSearch",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    ChannelId = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    MessageId = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    GuildId = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TeamSearch", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MethodPerms",
                columns: table => new
                {
                    PermID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MethodID = table.Column<int>(type: "int", nullable: false),
                    DiscordID = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    PermType = table.Column<byte>(type: "tinyint", nullable: false),
                    AllowType = table.Column<byte>(type: "tinyint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MethodPerms", x => x.PermID);
                    table.ForeignKey(
                        name: "FK_MethodPerms_MethodsConfig_MethodID",
                        column: x => x.MethodID,
                        principalTable: "MethodsConfig",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EmoteStats",
                columns: table => new
                {
                    EmoteID = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    UserID = table.Column<long>(type: "bigint", nullable: false),
                    UseCount = table.Column<long>(type: "bigint", nullable: false),
                    LastOccuredAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FirstOccuredAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsUnicode = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmoteStats", x => new { x.EmoteID, x.UserID });
                });

            migrationBuilder.CreateTable(
                name: "Invites",
                columns: table => new
                {
                    Code = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ChannelId = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatorId = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Invites", x => x.Code);
                });

            migrationBuilder.CreateTable(
                name: "DiscordUsers",
                columns: table => new
                {
                    ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserID = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    GuildID = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    Points = table.Column<long>(type: "bigint", nullable: false),
                    GivenReactionsCount = table.Column<long>(type: "bigint", nullable: false),
                    ObtainedReactionsCount = table.Column<long>(type: "bigint", nullable: false),
                    WebAdminPassword = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ApiToken = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UsedInviteCode = table.Column<string>(type: "nvarchar(20)", nullable: true),
                    Flags = table.Column<long>(type: "bigint", nullable: false),
                    WebAdminLoginCount = table.Column<int>(type: "int", nullable: true),
                    ApiAccessCount = table.Column<int>(type: "int", nullable: true),
                    Birthday = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UnverifyImunityGroup = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DiscordUsers", x => x.ID);
                    table.ForeignKey(
                        name: "FK_DiscordUsers_Invites_UsedInviteCode",
                        column: x => x.UsedInviteCode,
                        principalTable: "Invites",
                        principalColumn: "Code",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Reminders",
                columns: table => new
                {
                    RemindID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserID = table.Column<long>(type: "bigint", nullable: false),
                    FromUserID = table.Column<long>(type: "bigint", nullable: true),
                    At = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Message = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PostponeCounter = table.Column<int>(type: "int", nullable: false),
                    RemindMessageID = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    OriginalMessageID = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reminders", x => x.RemindID);
                    table.ForeignKey(
                        name: "FK_Reminders_DiscordUsers_FromUserID",
                        column: x => x.FromUserID,
                        principalTable: "DiscordUsers",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Reminders_DiscordUsers_UserID",
                        column: x => x.UserID,
                        principalTable: "DiscordUsers",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UnverifyLogs",
                columns: table => new
                {
                    ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Operation = table.Column<int>(type: "int", nullable: false),
                    FromUserID = table.Column<long>(type: "bigint", nullable: false),
                    ToUserID = table.Column<long>(type: "bigint", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    JsonData = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UnverifyLogs", x => x.ID);
                    table.ForeignKey(
                        name: "FK_UnverifyLogs_DiscordUsers_FromUserID",
                        column: x => x.FromUserID,
                        principalTable: "DiscordUsers",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_UnverifyLogs_DiscordUsers_ToUserID",
                        column: x => x.ToUserID,
                        principalTable: "DiscordUsers",
                        principalColumn: "ID");
                });

            migrationBuilder.CreateTable(
                name: "UserChannels",
                columns: table => new
                {
                    ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ChannelID = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    UserID = table.Column<long>(type: "bigint", nullable: false),
                    Count = table.Column<long>(type: "bigint", nullable: false),
                    LastMessageAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserChannels", x => x.ID);
                    table.ForeignKey(
                        name: "FK_UserChannels_DiscordUsers_UserID",
                        column: x => x.UserID,
                        principalTable: "DiscordUsers",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Unverifies",
                columns: table => new
                {
                    UserID = table.Column<long>(type: "bigint", nullable: false),
                    StartDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Roles = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SetLogOperationID = table.Column<long>(type: "bigint", nullable: true),
                    Channels = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Unverifies", x => x.UserID);
                    table.ForeignKey(
                        name: "FK_Unverifies_DiscordUsers_UserID",
                        column: x => x.UserID,
                        principalTable: "DiscordUsers",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Unverifies_UnverifyLogs_SetLogOperationID",
                        column: x => x.SetLogOperationID,
                        principalTable: "UnverifyLogs",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DiscordUsers_GuildID",
                table: "DiscordUsers",
                column: "GuildID");

            migrationBuilder.CreateIndex(
                name: "IX_DiscordUsers_UsedInviteCode",
                table: "DiscordUsers",
                column: "UsedInviteCode");

            migrationBuilder.CreateIndex(
                name: "IX_DiscordUsers_UserID",
                table: "DiscordUsers",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_EmoteStats_UserID_UseCount",
                table: "EmoteStats",
                columns: new[] { "UserID", "UseCount" });

            migrationBuilder.CreateIndex(
                name: "IX_Invites_CreatorId",
                table: "Invites",
                column: "CreatorId");

            migrationBuilder.CreateIndex(
                name: "IX_MethodPerms_MethodID",
                table: "MethodPerms",
                column: "MethodID");

            migrationBuilder.CreateIndex(
                name: "IX_Reminders_FromUserID",
                table: "Reminders",
                column: "FromUserID");

            migrationBuilder.CreateIndex(
                name: "IX_Reminders_UserID",
                table: "Reminders",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_Unverifies_SetLogOperationID",
                table: "Unverifies",
                column: "SetLogOperationID",
                unique: true,
                filter: "[SetLogOperationID] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_UnverifyLogs_FromUserID",
                table: "UnverifyLogs",
                column: "FromUserID");

            migrationBuilder.CreateIndex(
                name: "IX_UnverifyLogs_ToUserID",
                table: "UnverifyLogs",
                column: "ToUserID");

            migrationBuilder.CreateIndex(
                name: "IX_UserChannels_UserID",
                table: "UserChannels",
                column: "UserID");

            migrationBuilder.AddForeignKey(
                name: "FK_EmoteStats_DiscordUsers_UserID",
                table: "EmoteStats",
                column: "UserID",
                principalTable: "DiscordUsers",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Invites_DiscordUsers_CreatorId",
                table: "Invites",
                column: "CreatorId",
                principalTable: "DiscordUsers",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DiscordUsers_Invites_UsedInviteCode",
                table: "DiscordUsers");

            migrationBuilder.DropTable(
                name: "AutoReply");

            migrationBuilder.DropTable(
                name: "EmoteStats");

            migrationBuilder.DropTable(
                name: "Errors");

            migrationBuilder.DropTable(
                name: "GlobalConfig");

            migrationBuilder.DropTable(
                name: "MethodPerms");

            migrationBuilder.DropTable(
                name: "Reminders");

            migrationBuilder.DropTable(
                name: "TeamSearch");

            migrationBuilder.DropTable(
                name: "Unverifies");

            migrationBuilder.DropTable(
                name: "UserChannels");

            migrationBuilder.DropTable(
                name: "MethodsConfig");

            migrationBuilder.DropTable(
                name: "UnverifyLogs");

            migrationBuilder.DropTable(
                name: "Invites");

            migrationBuilder.DropTable(
                name: "DiscordUsers");
        }
    }
}
