using Microsoft.EntityFrameworkCore.Migrations;

namespace Grillbot.Migrations
{
    public partial class RemoveMathSessions : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DELETE FROM [MethodPerms] WHERE [MethodID]=(SELECT ID FROM [MethodsConfig] WHERE [Group]='math' AND [Command]='session')");
            migrationBuilder.Sql("DELETE FROM [MethodPerms] WHERE [MethodID]=(SELECT ID FROM [MethodsConfig] WHERE [Group]='math' AND [Command]='sessions')");
            migrationBuilder.Sql("DELETE FROM [MethodsConfig] WHERE [Group]='math' AND [Command] IN ('session', 'sessions')");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
