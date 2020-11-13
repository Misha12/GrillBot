# GrillBot

[![Build Status](https://github.com/misha12/GrillBot/workflows/.NET%20Core/badge.svg)](https://github.com/Misha12/GrillBot/actions)
[![GitHub license](https://img.shields.io/github/license/Naereen/StrapDown.js.svg)](https://github.com/Naereen/StrapDown.js/blob/master/LICENSE)
[![Maintenance](https://img.shields.io/badge/Maintained%3F-yes-green.svg)](https://GitHub.com/Misha12/grillbot/graphs/commit-activity)
[![Code size](https://img.shields.io/github/languages/code-size/misha12/grillbot?label=Code%20size)](https://github.com/misha12/grillbot)
[![Repo size](https://img.shields.io/github/repo-size/misha12/grillbot?label=Repo%20size)](https://github.com/misha12/grillbot)

## Requirements

- MSSQL server - [SQL Server Downloads](https://www.microsoft.com/en-us/sql-server/sql-server-downloads)
- .NET 5.0 (with ASP\.NET Core 5.0.0) - [Download .NET 5.0 (Linux, macOS, and Windows)](https://dotnet.microsoft.com/download/dotnet/5.0)

### Development requirements

- Microsoft Visual Studio 2019 (or another IDE supports .NET) - [Install Visual Studio](https://docs.microsoft.com/en-us/visualstudio/install/install-visual-studio?view=vs-2019)
- dotnet-ef (For code first migrations) - [EF Core tools reference (.NET CLI) - EF Core](https://docs.microsoft.com/en-us/ef/core/miscellaneous/cli/dotnet)

## Used NuGet packages

Most packages are distributed using the NuGet packaging system and will be installed at build.

Only Discord.NET package is distributed as pre-release from MyGet feed.

### GrillBot

- Discord.NET
  - [NuGet](https://www.nuget.org/packages/Discord.Net/)
  - [MyGet-PreRelease](https://www.myget.org/F/discord-net/api/v3/index.json) &lt;= Add as NuGet source (**Used in project**)
    - Command to add MyGet source from dotnet CLI: `dotnet nuget add source -n "MyGet_DiscordNET_Prerelease" https://www.myget.org/F/discord-net/api/v3/index.json`
- [Microsoft.AspNetCore.Mvc.Razor.RuntimeCompilation](https://www.nuget.org/packages/Microsoft.AspNetCore.Mvc.Razor.RuntimeCompilation/)
- [Microsoft.EntityFrameworkCore.SqlServer](https://www.nuget.org/packages/Microsoft.EntityFrameworkCore.SqlServer/)
- [Microsoft.VisualStudio.CodeGeneration.Design](https://www.nuget.org/packages/Microsoft.VisualStudio.Web.CodeGeneration.Design/)
- [UnicodeEmoji.NET](https://www.nuget.org/packages/UnicodeEmoji.net/)
- [BCrypt.Net-Next](https://www.nuget.org/packages/BCrypt.Net-Next/)
- [Microsoft.EntityFrameworkCore.Tools](https://www.nuget.org/packages/Microsoft.EntityFrameworkCore.Tools/)
- [GitInfo](https://www.nuget.org/packages/GitInfo/)
- [Swashbuckle.AspNetCore](https://www.nuget.org/packages/Swashbuckle.AspNetCore/)
- [System.Drawing.Common](https://www.nuget.org/packages/System.Drawing.Common/)
- [System.Drawing.Primitives](https://www.nuget.org/packages/System.Drawing.Primitives/)

### GrillBotMath

- [Newtonsoft.JSON](https://www.nuget.org/packages/Newtonsoft.Json/)
- [MathParser.org-mXParser](https://www.nuget.org/packages/MathParser.org-mXparser/)

## Database

GrillBot project using Code first database migrations.

To create database:

- Configure database connection string in appsettings.development.json. If you are using MSSQL LocalDB you can use connection string in appsettings.json.
- `dotnet tool restore`
- `dotnet ef database update`

## Config (appsettings.json/appsettings.development.json)

- **Keys in bold must be setup to develop GrillBot locally**
- Format: **JSON**
- Filename: **appsettings(.development).json**

Use appsettings.development.json for development purposes.
If you edit `appsettings.json` file, write it to pull request.

### Models

#### Config

| Key                       | Type                              | Description                                                                                       |
| ------------------------- | --------------------------------- | ------------------------------------------------------------------------------------------------- |
| AllowedHosts              | string                            | Semicollon delimited list of allowed hostnames without port numbers.                              |
| CommandPrefix             | string                            | Message content, that must starts to invoke command.                                              |
| EmoteChain_CheckLastCount | int                               | Count of same emotes before bot send emote.                                                       |
| BackupErrors              | string                            | Path to the directory where the error log files will be saved when saving to the database failed. |
| Discord                   | [Config.Discord](#Config.Discord) | Service configuration                                                                             |
| ConnectionStrings         | KeyValuePair<string, string>      | Database connection strings                                                                       |

#### Config.Discord

For properties **Token** you will need to create your own Discord Application to get a Token for local development.

| Key                 | Type   | Description                                                                                                                      |
| ------------------- | ------ | -------------------------------------------------------------------------------------------------------------------------------- |
| Activity            | string | Now playing info. Includes git latest commit, current branch and lastest tag. If you do not want display any test, enter `None`. |
| Token               | string | Login token                                                                                                                      |
| UserJoinedMessage   | string | Message, that will be sent, when user joined to guild.                                                                           |
| LoggerRoomID        | string | ID of channel to send logging data (MessageEdited, MessageDeleted, ...).                                                         |
| ServerBoosterRoleId | string | ID of role with Nitro Server Booster role.                                                                                       |
| AdminChannelID      | string | ID of channel for administration purposes.                                                                                       |
| ErrorLogChannelID   | string | ID of channel for logging errors.                                                                                                |

## GrillBotMath

To run the math module in bot, you have to build GrillBotMath project and set path to GrillBotMath.dll file into database config `$config addMethod math/solve {"ProcessPath": "<HereYourPath>"}`

## Features

- [Permissions](docs/permissions.md)
- [Unverify](docs/unverify.md)
- [Meme](docs/meme.md)
- [Reminder](docs/reminder.md)
- [Users management](docs/users-management.md)
