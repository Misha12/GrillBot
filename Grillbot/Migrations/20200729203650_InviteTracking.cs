using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Grillbot.Migrations
{
    public partial class InviteTracking : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "UsedInviteCode",
                table: "DiscordUsers",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Invites",
                columns: table => new
                {
                    Code = table.Column<string>(maxLength: 20, nullable: false),
                    ChannelId = table.Column<string>(maxLength: 30, nullable: false),
                    CreatedAt = table.Column<DateTime>(nullable: true),
                    CreatorId = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Invites", x => x.Code);
                    table.ForeignKey(
                        name: "FK_Invites_DiscordUsers_CreatorId",
                        column: x => x.CreatorId,
                        principalTable: "DiscordUsers",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DiscordUsers_UsedInviteCode",
                table: "DiscordUsers",
                column: "UsedInviteCode");

            migrationBuilder.CreateIndex(
                name: "IX_Invites_CreatorId",
                table: "Invites",
                column: "CreatorId");

            migrationBuilder.AddForeignKey(
                name: "FK_DiscordUsers_Invites_UsedInviteCode",
                table: "DiscordUsers",
                column: "UsedInviteCode",
                principalTable: "Invites",
                principalColumn: "Code",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DiscordUsers_Invites_UsedInviteCode",
                table: "DiscordUsers");

            migrationBuilder.DropTable(
                name: "Invites");

            migrationBuilder.DropIndex(
                name: "IX_DiscordUsers_UsedInviteCode",
                table: "DiscordUsers");

            migrationBuilder.DropColumn(
                name: "UsedInviteCode",
                table: "DiscordUsers");
        }
    }
}
