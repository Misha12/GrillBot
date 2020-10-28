using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Grillbot.Migrations
{
    public partial class BirthdayMerge : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BirthdayDates");

            migrationBuilder.AddColumn<DateTime>(
                name: "Birthday",
                table: "DiscordUsers",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Birthday",
                table: "DiscordUsers");

            migrationBuilder.CreateTable(
                name: "BirthdayDates",
                columns: table => new
                {
                    UserID = table.Column<long>(type: "bigint", nullable: false),
                    AcceptAge = table.Column<bool>(type: "bit", nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false)
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
        }
    }
}
