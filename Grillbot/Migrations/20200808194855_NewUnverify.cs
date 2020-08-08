using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Grillbot.Migrations
{
    public partial class NewUnverify : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Unverifies",
                columns: table => new
                {
                    UserID = table.Column<long>(nullable: false),
                    StartDateTime = table.Column<DateTime>(nullable: false),
                    EndDateTime = table.Column<DateTime>(nullable: false),
                    Reason = table.Column<string>(nullable: true),
                    Roles = table.Column<string>(nullable: true),
                    Channels = table.Column<string>(nullable: true)
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
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Unverifies");
        }
    }
}
