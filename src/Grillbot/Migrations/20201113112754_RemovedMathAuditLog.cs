using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Grillbot.Migrations
{
    public partial class RemovedMathAuditLog : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MathAuditLog");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MathAuditLog",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ChannelID = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    DateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Expression = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Result = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UnitInfo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserID = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MathAuditLog", x => x.ID);
                    table.ForeignKey(
                        name: "FK_MathAuditLog_DiscordUsers_UserID",
                        column: x => x.UserID,
                        principalTable: "DiscordUsers",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MathAuditLog_UserID",
                table: "MathAuditLog",
                column: "UserID");
        }
    }
}
