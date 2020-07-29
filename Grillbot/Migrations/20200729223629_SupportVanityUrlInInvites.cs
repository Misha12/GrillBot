using Microsoft.EntityFrameworkCore.Migrations;

namespace Grillbot.Migrations
{
    public partial class SupportVanityUrlInInvites : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Invites_DiscordUsers_CreatorId",
                table: "Invites");

            migrationBuilder.AlterColumn<long>(
                name: "CreatorId",
                table: "Invites",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint");

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
                name: "FK_Invites_DiscordUsers_CreatorId",
                table: "Invites");

            migrationBuilder.AlterColumn<long>(
                name: "CreatorId",
                table: "Invites",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(long),
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Invites_DiscordUsers_CreatorId",
                table: "Invites",
                column: "CreatorId",
                principalTable: "DiscordUsers",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
