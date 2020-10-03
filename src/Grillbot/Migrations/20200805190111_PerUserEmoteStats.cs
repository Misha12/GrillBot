using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Grillbot.Migrations
{
    public partial class PerUserEmoteStats : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EmoteStats",
                columns: table => new
                {
                    EmoteID = table.Column<string>(maxLength: 150, nullable: false),
                    UserID = table.Column<long>(nullable: false),
                    UseCount = table.Column<long>(nullable: false),
                    LastOccuredAt = table.Column<DateTime>(nullable: false),
                    FirstOccuredAt = table.Column<DateTime>(nullable: false),
                    IsUnicode = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmoteStats", x => x.EmoteID);
                    table.ForeignKey(
                        name: "FK_EmoteStats_DiscordUsers_UserID",
                        column: x => x.UserID,
                        principalTable: "DiscordUsers",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EmoteStats_UserID",
                table: "EmoteStats",
                column: "UserID");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EmoteStats");
        }
    }
}
