using Microsoft.EntityFrameworkCore.Migrations;

namespace Grillbot.Migrations
{
    public partial class AuditLogFiles : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "AuditLogItemId",
                table: "Files",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Files_AuditLogItemId",
                table: "Files",
                column: "AuditLogItemId");

            migrationBuilder.AddForeignKey(
                name: "FK_Files_AuditLogs_AuditLogItemId",
                table: "Files",
                column: "AuditLogItemId",
                principalTable: "AuditLogs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Files_AuditLogs_AuditLogItemId",
                table: "Files");

            migrationBuilder.DropIndex(
                name: "IX_Files_AuditLogItemId",
                table: "Files");

            migrationBuilder.DropColumn(
                name: "AuditLogItemId",
                table: "Files");
        }
    }
}
