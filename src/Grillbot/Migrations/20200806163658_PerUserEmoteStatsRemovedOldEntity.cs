using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Grillbot.Migrations
{
    public partial class PerUserEmoteStatsRemovedOldEntity : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EmoteStatistics");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EmoteStatistics",
                columns: table => new
                {
                    GuildID = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    EmoteID = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Count = table.Column<long>(type: "bigint", nullable: false),
                    IsUnicode = table.Column<bool>(type: "bit", nullable: false),
                    LastOccuredAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmoteStatistics", x => new { x.GuildID, x.EmoteID });
                });
        }
    }
}
