using Microsoft.EntityFrameworkCore.Migrations;

namespace Grillbot.Migrations
{
    public partial class MergeAutoreplyCommands : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("UPDATE [MethodsConfig] SET [Command]='toggle' WHERE [Group]='autoreply' AND [Command]='enable'");
            migrationBuilder.Sql("DELETE FROM [MethodPerms] WHERE [MethodID]=(SELECT [ID] FROM [MethodsConfig] WHERE [Group]='autoreply' AND [Command]='disable');");
            migrationBuilder.Sql("DELETE FROM [MethodsConfig] WHERE [Group]='autoreply' AND [Command]='disable'");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
