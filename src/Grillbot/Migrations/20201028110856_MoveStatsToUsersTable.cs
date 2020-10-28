using Microsoft.EntityFrameworkCore.Migrations;

namespace Grillbot.Migrations
{
    public partial class MoveStatsToUsersTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Statistics");

            migrationBuilder.AddColumn<int>(
                name: "ApiAccessCount",
                table: "DiscordUsers",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "WebAdminLoginCount",
                table: "DiscordUsers",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ApiAccessCount",
                table: "DiscordUsers");

            migrationBuilder.DropColumn(
                name: "WebAdminLoginCount",
                table: "DiscordUsers");

            migrationBuilder.CreateTable(
                name: "Statistics",
                columns: table => new
                {
                    UserID = table.Column<long>(type: "bigint", nullable: false),
                    ApiCallCount = table.Column<int>(type: "int", nullable: false),
                    WebAdminLoginCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Statistics", x => x.UserID);
                    table.ForeignKey(
                        name: "FK_Statistics_DiscordUsers_UserID",
                        column: x => x.UserID,
                        principalTable: "DiscordUsers",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });
        }
    }
}
