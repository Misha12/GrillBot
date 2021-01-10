using Microsoft.EntityFrameworkCore.Migrations;

namespace Grillbot.Migrations
{
    public partial class AuditLogChannelId : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ChannelId",
                table: "AuditLogs",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ChannelId",
                table: "AuditLogs");
        }
    }
}
