using Microsoft.EntityFrameworkCore.Migrations;

namespace Grillbot.Migrations
{
    public partial class RemoveUnverifyStats : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DELETE FROM [MethodPerms] WHERE [MethodID]=(SELECT ID FROM [MethodsConfig] WHERE [Group]='unverify' AND [Command]='stats')");
            migrationBuilder.Sql("DELETE FROM [MethodsConfig] WHERE [Group]='unverify' AND [Command] IN ('stats')");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
