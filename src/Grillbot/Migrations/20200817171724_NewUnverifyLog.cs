using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Grillbot.Migrations
{
    public partial class NewUnverifyLog : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UnverifyLogs",
                columns: table => new
                {
                    ID = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Operation = table.Column<int>(nullable: false),
                    FromUserID = table.Column<long>(nullable: false),
                    ToUserID = table.Column<long>(nullable: false),
                    CreatedAt = table.Column<DateTime>(nullable: false),
                    JsonData = table.Column<string>(nullable: true)
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

            migrationBuilder.CreateIndex(
                name: "IX_DiscordUsers_GuildID",
                table: "DiscordUsers",
                column: "GuildID");

            migrationBuilder.CreateIndex(
                name: "IX_UnverifyLogs_FromUserID",
                table: "UnverifyLogs",
                column: "FromUserID");

            migrationBuilder.CreateIndex(
                name: "IX_UnverifyLogs_ToUserID",
                table: "UnverifyLogs",
                column: "ToUserID");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UnverifyLogs");

            migrationBuilder.DropIndex(
                name: "IX_DiscordUsers_GuildID",
                table: "DiscordUsers");
        }
    }
}
