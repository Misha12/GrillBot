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

```sh
dotnet tool restore
dotnet ef database update -- DB_CONN="{YOUR_CONNECTION_STRING}"
```

## Config

- `appsettings.json` configuration was deprecated in version `1.8` and removed in `2.0`. Newly is used database config in table GlobalConfig and environment or command line parameters.

Choice between command line parameters or environment variables is your. GrillBot supports both.

### Configuration variables

- `APP_TOKEN`: **REQUIRED** to run bot. This token you can create in discord developer portal.
- `DB_CONN`: **REQUIRED** to run bot. Connection string to your existing database.

#### GlobalConfig

| Key                   | Description                                                                                                                      | Example value        |
| --------------------- | -------------------------------------------------------------------------------------------------------------------------------- | -------------------- |
| CommandPrefix         | Message content, that must starts to invoke command.                                                                             | `$`                  |
| EmoteChain_CheckCount | Count of same emotes before bot send emote.                                                                                      | `5`                  |
| ActivityMessage       | Now playing info. Includes git latest commit, current branch and lastest tag. If you do not want display any test, enter `None`. | `Some message`       |
| LoggerRoom            | ID of channel to send logging data (MessageEdited, MessageDeleted, ...).                                                         | `531058805233156096` |
| AdminChannel          | ID of channel for administration purposes. Such as booster notifications.                                                        | `531058805233156096` |
| ServerBoosterRoleId   | ID of role with Nitro Server Booster role.                                                                                       | `585529323960664074` |
| ErrorLogChannel       | ID of channel for logging errors.                                                                                                | `531058805233156096` |

##### Commands for global config control

- `$globalConfig keys` - Prints list of available configuration values.
- `$globalConfig get {key}` - Prints content of current configuration.
- `$globalConfig set {key} {value}` - Sets configuration and saves it.

#### Run with command line parameters

```sh
dotnet run GrillBot.dll -- APP_TOKEN="{YOUR_TOKEN}" DB_CONN="{YOUR_CONNECTION_STRING}"
```

or

- Linux:

```sh
./GrillBot APP_TOKEN="{YOUR_TOKEN}" DB_CONN="{YOUR_CONNECTION_STRING}"
```

- Windows:

```sh
GrillBot.exe APP_TOKEN="{YOUR_TOKEN}" DB_CONN="{YOUR_CONNECTION_STRING}"
```

## Features

- [Permissions](docs/permissions.md)
- [Unverify](docs/unverify.md)
- [Meme](docs/meme.md)
- [Reminder](docs/reminder.md)
- [Users management](docs/users-management.md)

## Docker

Latest tag is published on [DockerHub](https://hub.docker.com/repository/docker/misha12/grillbot). SQL script with latest DB schema is included in release.
