using Microsoft.EntityFrameworkCore.Migrations;

namespace Grillbot.Migrations
{
    public partial class RemoveUnverifyImmunity : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UnverifyImunityGroup",
                table: "DiscordUsers");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "UnverifyImunityGroup",
                table: "DiscordUsers",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: true);
        }
    }
}
