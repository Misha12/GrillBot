using Microsoft.EntityFrameworkCore.Migrations;

namespace Grillbot.Migrations
{
    public partial class FixNullableInUnverify : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Unverifies_UnverifyLogs_SetLogOperationID",
                table: "Unverifies");

            migrationBuilder.DropIndex(
                name: "IX_Unverifies_SetLogOperationID",
                table: "Unverifies");

            migrationBuilder.AlterColumn<long>(
                name: "SetLogOperationID",
                table: "Unverifies",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.CreateIndex(
                name: "IX_Unverifies_SetLogOperationID",
                table: "Unverifies",
                column: "SetLogOperationID",
                unique: true,
                filter: "[SetLogOperationID] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_Unverifies_UnverifyLogs_SetLogOperationID",
                table: "Unverifies",
                column: "SetLogOperationID",
                principalTable: "UnverifyLogs",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Unverifies_UnverifyLogs_SetLogOperationID",
                table: "Unverifies");

            migrationBuilder.DropIndex(
                name: "IX_Unverifies_SetLogOperationID",
                table: "Unverifies");

            migrationBuilder.AlterColumn<long>(
                name: "SetLogOperationID",
                table: "Unverifies",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(long),
                oldNullable: true);

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
    }
}
