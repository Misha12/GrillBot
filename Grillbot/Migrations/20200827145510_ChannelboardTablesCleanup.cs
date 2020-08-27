using Microsoft.EntityFrameworkCore.Migrations;

namespace Grillbot.Migrations
{
    public partial class ChannelboardTablesCleanup : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_UserChannels_DiscordUserID",
                table: "UserChannels");

            migrationBuilder.DropColumn(
                name: "DiscordUserID",
                table: "UserChannels");

            migrationBuilder.DropColumn(
                name: "GuildID",
                table: "UserChannels");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DiscordUserID",
                table: "UserChannels",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GuildID",
                table: "UserChannels",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserChannels_DiscordUserID",
                table: "UserChannels",
                column: "DiscordUserID");
        }
    }
}
