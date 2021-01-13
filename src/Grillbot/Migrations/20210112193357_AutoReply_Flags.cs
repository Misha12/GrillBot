using Microsoft.EntityFrameworkCore.Migrations;

namespace Grillbot.Migrations
{
    public partial class AutoReply_Flags : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CaseSensitive",
                table: "AutoReply");

            migrationBuilder.DropColumn(
                name: "IsDisabled",
                table: "AutoReply");

            migrationBuilder.AddColumn<int>(
                name: "Flags",
                table: "AutoReply",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Flags",
                table: "AutoReply");

            migrationBuilder.AddColumn<bool>(
                name: "CaseSensitive",
                table: "AutoReply",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDisabled",
                table: "AutoReply",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}
