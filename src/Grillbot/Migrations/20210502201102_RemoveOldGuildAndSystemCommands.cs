using Microsoft.EntityFrameworkCore.Migrations;

namespace Grillbot.Migrations
{
    public partial class RemoveOldGuildAndSystemCommands : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var guildCommands = new[] { "calc_perms", "clear_perms", "clear_react", "createEmotesList", "backupEmotes" };
            var systemCommands = new[] { "shutdown_force", "shutdown", "migrateLogs" };

            foreach (var command in guildCommands)
            {
                migrationBuilder.Sql($"DELETE FROM [MethodPerms] WHERE [MethodID]=(SELECT ID FROM [MethodsConfig] WHERE [Group]='guild' AND [Command]='{command}')");
                migrationBuilder.Sql($"DELETE FROM [MethodsConfig] WHERE [Group]='guild' AND [Command]='{command}'");
            }

            foreach (var command in systemCommands)
            {
                migrationBuilder.Sql($"DELETE FROM [MethodPerms] WHERE [MethodID]=(SELECT ID FROM [MethodsConfig] WHERE [Group]='system' AND [Command]='{command}')");
                migrationBuilder.Sql($"DELETE FROM [MethodsConfig] WHERE [Group]='system' AND [Command]='{command}'");
            }
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
