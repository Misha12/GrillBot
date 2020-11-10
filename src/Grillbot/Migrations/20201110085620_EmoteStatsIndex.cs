using Microsoft.EntityFrameworkCore.Migrations;

namespace Grillbot.Migrations
{
    public partial class EmoteStatsIndex : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_EmoteStats_UserID",
                table: "EmoteStats");

            migrationBuilder.CreateIndex(
                name: "IX_EmoteStats_UserID_UseCount",
                table: "EmoteStats",
                columns: new[] { "UserID", "UseCount" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_EmoteStats_UserID_UseCount",
                table: "EmoteStats");

            migrationBuilder.CreateIndex(
                name: "IX_EmoteStats_UserID",
                table: "EmoteStats",
                column: "UserID");
        }
    }
}
