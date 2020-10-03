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
                    ID = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MustContains = table.Column<string>(nullable: false),
                    ReplyMessage = table.Column<string>(nullable: false),
                    IsDisabled = table.Column<bool>(nullable: false),
                    CompareType = table.Column<int>(nullable: false),
                    CaseSensitive = table.Column<bool>(nullable: false),
                    GuildID = table.Column<string>(nullable: false),
                    ChannelID = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AutoReply", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "DiscordUsers",
                columns: table => new
                {
                    ID = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserID = table.Column<string>(maxLength: 30, nullable: true),
                    GuildID = table.Column<string>(maxLength: 30, nullable: true),
                    Points = table.Column<long>(nullable: false),
                    GivenReactionsCount = table.Column<long>(nullable: false),
                    ObtainedReactionsCount = table.Column<long>(nullable: false),
                    WebAdminPassword = table.Column<string>(nullable: true),
                    ApiToken = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DiscordUsers", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "EmoteStatistics",
                columns: table => new
                {
                    EmoteID = table.Column<string>(maxLength: 255, nullable: false),
                    GuildID = table.Column<string>(nullable: false),
                    Count = table.Column<long>(nullable: false),
                    LastOccuredAt = table.Column<DateTime>(nullable: false),
                    IsUnicode = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmoteStatistics", x => new { x.GuildID, x.EmoteID });
                });

            migrationBuilder.CreateTable(
                name: "MethodsConfig",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GuildID = table.Column<string>(maxLength: 30, nullable: false),
                    Group = table.Column<string>(maxLength: 100, nullable: false),
                    Command = table.Column<string>(maxLength: 100, nullable: false),
                    ConfigData = table.Column<string>(nullable: false),
                    OnlyAdmins = table.Column<bool>(nullable: false),
                    UsedCount = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MethodsConfig", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "TeamSearch",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(maxLength: 30, nullable: true),
                    ChannelId = table.Column<string>(maxLength: 30, nullable: true),
                    MessageId = table.Column<string>(maxLength: 30, nullable: true),
                    GuildId = table.Column<string>(maxLength: 30, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TeamSearch", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TempUnverify",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GuildID = table.Column<string>(maxLength: 30, nullable: false),
                    UserID = table.Column<string>(maxLength: 30, nullable: false),
                    TimeFor = table.Column<int>(nullable: false),
                    StartAt = table.Column<DateTime>(nullable: false),
                    RolesToReturn = table.Column<string>(nullable: false),
                    ChannelOverrides = table.Column<string>(nullable: false),
                    Reason = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TempUnverify", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "UnverifyLog",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Operation = table.Column<int>(nullable: false),
                    FromUserID = table.Column<string>(maxLength: 30, nullable: true),
                    GuildID = table.Column<string>(maxLength: 30, nullable: true),
                    DateTime = table.Column<DateTime>(nullable: false),
                    Data = table.Column<string>(nullable: true),
                    DestUserID = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UnverifyLog", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "BirthdayDates",
                columns: table => new
                {
                    UserID = table.Column<long>(nullable: false),
                    Date = table.Column<DateTime>(nullable: false),
                    AcceptAge = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BirthdayDates", x => x.UserID);
                    table.ForeignKey(
                        name: "FK_BirthdayDates_DiscordUsers_UserID",
                        column: x => x.UserID,
                        principalTable: "DiscordUsers",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MathAuditLog",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Expression = table.Column<string>(nullable: true),
                    UserID = table.Column<long>(nullable: false),
                    ChannelID = table.Column<string>(maxLength: 30, nullable: true),
                    UnitInfo = table.Column<string>(nullable: true),
                    Result = table.Column<string>(nullable: true),
                    DateTime = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MathAuditLog", x => x.ID);
                    table.ForeignKey(
                        name: "FK_MathAuditLog_DiscordUsers_UserID",
                        column: x => x.UserID,
                        principalTable: "DiscordUsers",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserChannels",
                columns: table => new
                {
                    ID = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ChannelID = table.Column<string>(maxLength: 30, nullable: true),
                    DiscordUserID = table.Column<string>(maxLength: 30, nullable: true),
                    UserID = table.Column<long>(nullable: false),
                    GuildID = table.Column<string>(nullable: true),
                    Count = table.Column<long>(nullable: false),
                    LastMessageAt = table.Column<DateTime>(nullable: false)
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
                name: "MethodPerms",
                columns: table => new
                {
                    PermID = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MethodID = table.Column<int>(nullable: false),
                    DiscordID = table.Column<string>(maxLength: 30, nullable: false),
                    PermType = table.Column<byte>(nullable: false),
                    AllowType = table.Column<byte>(nullable: false)
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

            migrationBuilder.CreateIndex(
                name: "IX_DiscordUsers_UserID",
                table: "DiscordUsers",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_MathAuditLog_UserID",
                table: "MathAuditLog",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_MethodPerms_MethodID",
                table: "MethodPerms",
                column: "MethodID");

            migrationBuilder.CreateIndex(
                name: "IX_UserChannels_DiscordUserID",
                table: "UserChannels",
                column: "DiscordUserID");

            migrationBuilder.CreateIndex(
                name: "IX_UserChannels_UserID",
                table: "UserChannels",
                column: "UserID");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AutoReply");

            migrationBuilder.DropTable(
                name: "BirthdayDates");

            migrationBuilder.DropTable(
                name: "EmoteStatistics");

            migrationBuilder.DropTable(
                name: "MathAuditLog");

            migrationBuilder.DropTable(
                name: "MethodPerms");

            migrationBuilder.DropTable(
                name: "TeamSearch");

            migrationBuilder.DropTable(
                name: "TempUnverify");

            migrationBuilder.DropTable(
                name: "UnverifyLog");

            migrationBuilder.DropTable(
                name: "UserChannels");

            migrationBuilder.DropTable(
                name: "MethodsConfig");

            migrationBuilder.DropTable(
                name: "DiscordUsers");
        }
    }
}
