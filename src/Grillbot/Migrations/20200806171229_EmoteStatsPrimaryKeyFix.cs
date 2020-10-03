using Microsoft.EntityFrameworkCore.Migrations;

namespace Grillbot.Migrations
{
    public partial class EmoteStatsPrimaryKeyFix : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_EmoteStats",
                table: "EmoteStats");

            migrationBuilder.AddPrimaryKey(
                name: "PK_EmoteStats",
                table: "EmoteStats",
                columns: new[] { "EmoteID", "UserID" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_EmoteStats",
                table: "EmoteStats");

            migrationBuilder.AddPrimaryKey(
                name: "PK_EmoteStats",
                table: "EmoteStats",
                column: "EmoteID");
        }
    }
}
