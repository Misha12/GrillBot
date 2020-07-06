# GrillBot

[![Build Status](https://dev.azure.com/mhalabica/GrillBot/_apis/build/status/Misha12.GrillBot?branchName=master)](https://dev.azure.com/mhalabica/GrillBot/_build/latest?definitionId=8&branchName=master)
[![GitHub license](https://img.shields.io/github/license/Naereen/StrapDown.js.svg)](https://github.com/Naereen/StrapDown.js/blob/master/LICENSE)
[![Maintenance](https://img.shields.io/badge/Maintained%3F-yes-green.svg)](https://GitHub.com/Misha12/grillbot/graphs/commit-activity)

## Requirements
- MSSQL server 
  - Instalation: https://www.microsoft.com/en-us/sql-server/sql-server-downloads 
- Microsoft Visual Studio 2019 (or another IDE supports .NET)
  - Visual studio instalation: https://docs.microsoft.com/cs-cz/visualstudio/install/install-visual-studio?view=vs-2019
- .NET Core 3.1 (with ASP\.NET Core 3.1)
  - https://dotnet.microsoft.com/download/dotnet-core/3.1
- dotnet-ef (For code first migrations)
  - https://docs.microsoft.com/cs-cz/ef/core/miscellaneous/cli/dotnet

## Used NuGet packages

### GrillBot
- [Discord.NET](https://www.nuget.org/packages/Discord.Net/)
- [Microsoft.AspNetCore.Mvc.Razor.RuntimeCompilation](https://www.nuget.org/packages/Microsoft.AspNetCore.Mvc.Razor.RuntimeCompilation/3.1.3)
- [Microsoft.EntityFrameworkCore.SqlServer](https://www.nuget.org/packages/Microsoft.EntityFrameworkCore.SqlServer/3.1.3)
- [Microsoft.VisualStudio.CodeGeneration.Design](https://www.nuget.org/packages/Microsoft.VisualStudio.Web.CodeGeneration.Design/5.0.0-preview.3.20207.1)
- [UnicodeEmoji.NET](https://www.nuget.org/packages/UnicodeEmoji.net/)
- [BCrypt.Net-Next](https://www.nuget.org/packages/BCrypt.Net-Next/)
- [Microsoft.EntityFrameworkCore.Tools](https://www.nuget.org/packages/Microsoft.EntityFrameworkCore.Tools/)

### GrillBotMath
- [Newtonsoft.JSON](https://www.nuget.org/packages/Newtonsoft.Json/)
- [MathParser.org-mXParser](https://www.nuget.org/packages/MathParser.org-mXparser/)

## Database
This project using Code first database migrations.

To create database:
- Configure database connection string in appsettings.development.json. If you are using MSSQL LocalDB you can use connection string in appsettings.json.
- `dotnet tool restore`
- `dotnet ef database update`

## Config (appsettings.json/appsettings.development.json)
**Keys in bold must be setup to develop GrillBot locally**
- Format: **JSON**
- Filename: **appsettings(.development).json**

Use appsettings.development.json for development purposes.
If you edit `appsettings.json` file, write it to pull request.

### Models
#### Config

| Key                       | Type                              | Description                                                                       |
| ------------------------- | --------------------------------- | --------------------------------------------------------------------------------- |
| AllowedHosts              | string                            | Semicollon delimited list of allowed hostnames without port numbers.              |
| CommandPrefix             | string                            | Message content, that must starts to invoke command.                              |
| Administrators            | string[]                          | List of bot administrators. Can use bot independently of roles. Value is user ID. |
| EmoteChain_CheckLastCount | int                               | Count of same emotes before bot send emote.                                       |
| Discord                   | [Config.Discord](#Config.Discord) | Service configuration                                                             |
| ConnectionStrings         | KeyValuePair<string, string>      | Database connection strings                                                       |

#### Config.Discord
For properties **Token**, **ClientId**, **ClientSecret** you will need to create your own Discord Application to get a Token for local development.

| Key                 | Type   | Description                                                              |
| ------------------- | ------ | ------------------------------------------------------------------------ |
| Activity            | string | Now playing game info.                                                   |
| Token               | string | Login token                                                              |
| UserJoinedMessage   | string | Message, that will be sent, when user joined to guild.                   |
| LoggerRoomID        | string | ID of channel to send logging data (MessageEdited, MessageDeleted, ...). |
| ClientId            | string | ID of application in discord OAuth Service.                              |
| ClientSecret        | string | Secret key for authentication in Discord OAuth service.                  |
| ServerBoosterRoleId | string | ID of role with Nitro Server Booster role.                               |
| AdminChannelID      | string | ID of channel for administration purposes.                               |
| ErrorLogChannelID   | string | ID of channel for logging errors.                                        |

## GrillBotMath
To run the math module in bot, you have to build GrillBotMath project and set path to GrillBotMath.dll file into database config `$config addMethod /solve {"ProcessPath": "<HereYourPath>"}`

## Features

- [Permissions](docs/permissions.md)
