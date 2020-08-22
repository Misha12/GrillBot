using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Grillbot.Migrations
{
    public partial class RemovedOldEntities : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TempUnverify");

            migrationBuilder.DropTable(
                name: "UnverifyLog");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TempUnverify",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ChannelOverrides = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    GuildID = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RolesToReturn = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StartAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TimeFor = table.Column<int>(type: "int", nullable: false),
                    UserID = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TempUnverify", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "UnverifyLog",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Data = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DestUserID = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FromUserID = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    GuildID = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    Operation = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UnverifyLog", x => x.ID);
                });
        }
    }
}
