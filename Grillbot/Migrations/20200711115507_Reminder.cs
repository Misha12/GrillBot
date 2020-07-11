using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Grillbot.Migrations
{
    public partial class Reminder : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Reminders",
                columns: table => new
                {
                    RemindID = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserID = table.Column<long>(nullable: false),
                    FromUserID = table.Column<long>(nullable: true),
                    At = table.Column<DateTime>(nullable: false),
                    Message = table.Column<string>(nullable: true)
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

            migrationBuilder.CreateIndex(
                name: "IX_Reminders_FromUserID",
                table: "Reminders",
                column: "FromUserID");

            migrationBuilder.CreateIndex(
                name: "IX_Reminders_UserID",
                table: "Reminders",
                column: "UserID");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Reminders");
        }
    }
}
