using Microsoft.EntityFrameworkCore.Migrations;

namespace Grillbot.Migrations
{
    public partial class RemoveSetBotAdmin : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DELETE FROM [MethodPerms] WHERE [MethodID]=(SELECT ID FROM [MethodsConfig] WHERE [Group]='user' AND [Command]='setBotAdmin')");
            migrationBuilder.Sql("DELETE FROM [MethodsConfig] WHERE [Group]='user' AND [Command] IN ('setBotAdmin')");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
