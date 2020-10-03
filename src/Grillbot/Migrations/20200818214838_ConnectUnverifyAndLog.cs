using Microsoft.EntityFrameworkCore.Migrations;

namespace Grillbot.Migrations
{
    public partial class ConnectUnverifyAndLog : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "SetLogOperationID",
                table: "Unverifies",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.CreateIndex(
                name: "IX_Unverifies_SetLogOperationID",
                table: "Unverifies",
                column: "SetLogOperationID",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Unverifies_UnverifyLogs_SetLogOperationID",
                table: "Unverifies",
                column: "SetLogOperationID",
                principalTable: "UnverifyLogs",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Unverifies_UnverifyLogs_SetLogOperationID",
                table: "Unverifies");

            migrationBuilder.DropIndex(
                name: "IX_Unverifies_SetLogOperationID",
                table: "Unverifies");

            migrationBuilder.DropColumn(
                name: "SetLogOperationID",
                table: "Unverifies");
        }
    }
}
