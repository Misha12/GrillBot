using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Grillbot.Migrations
{
    public partial class AuditLogs : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AuditLogs",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UserId = table.Column<long>(type: "bigint", nullable: true),
                    GuildId = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    DcAuditLogId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    JsonData = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AuditLogs_DiscordUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "DiscordUsers",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_DcAuditLogId",
                table: "AuditLogs",
                column: "DcAuditLogId");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_GuildId",
                table: "AuditLogs",
                column: "GuildId");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_UserId",
                table: "AuditLogs",
                column: "UserId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AuditLogs");
        }
    }
}
