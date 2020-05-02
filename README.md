# GrillBot

## Requirements
- MSSQL server 
  - Instalation: https://www.microsoft.com/en-us/sql-server/sql-server-downloads 
- Microsoft Visual Studio (2017 and later) (or another IDE supports .NET)
  - Visual studio instalation: https://docs.microsoft.com/cs-cz/visualstudio/install/install-visual-studio?view=vs-2019
- .NET Core 3.1 (with ASP\.NET Core 3.1)
  - https://dotnet.microsoft.com/download/dotnet-core/3.1

## Continuous integration
[![Build Status](https://dev.azure.com/mhalabica/GrillBot/_apis/build/status/Misha12.GrillBot?branchName=master)](https://dev.azure.com/mhalabica/GrillBot/_build/latest?definitionId=8&branchName=master)

## Used NuGet packages

### GrillBot
- [Discord.NET](https://www.nuget.org/packages/Discord.Net/)
- [Discord.Addons.Interactive](https://www.nuget.org/packages/Discord.Addons.Interactive/)
- [Microsoft.AspNetCore.Mvc.Razor.RuntimeCompilation](https://www.nuget.org/packages/Microsoft.AspNetCore.Mvc.Razor.RuntimeCompilation/3.1.3)
- [Microsoft.EntityFrameworkCore.SqlServer](https://www.nuget.org/packages/Microsoft.EntityFrameworkCore.SqlServer/3.1.3)
- [Microsoft.VisualStudio.CodeGeneration.Design](https://www.nuget.org/packages/Microsoft.VisualStudio.Web.CodeGeneration.Design/5.0.0-preview.3.20207.1)
- [UnicodeEmoji.NET](https://www.nuget.org/packages/UnicodeEmoji.net/)
- [BCrypt.Net-Next](https://www.nuget.org/packages/BCrypt.Net-Next/)

### GrillBotMath
- [Newtonsoft.JSON](https://www.nuget.org/packages/Newtonsoft.Json/)
- [MathParser.org-mXParser](https://www.nuget.org/packages/MathParser.org-mXparser/)

## Database
You can create database with scripts in `GrillBot-DB` project. If you're using Visual Studio on Windows, you can create migration script for your SQL Server instance.

## Config (appsettings.json)
**Keys in bold must be setup to develop GrillBot locally**
- Format: **JSON**
- Filename: **appsettings.json**

### Models
#### Config

| Key                       | Type                              | Description                                                                       |
| ------------------------- | --------------------------------- | --------------------------------------------------------------------------------- |
| AllowedHosts              | string                            | Semicollon delimited list of allowed hostnames without port numbers.              |
| CommandPrefix             | string                            | Message content, that must starts to invoke command.                              |
| Database                  | string                            | Connection string to MSSQL database.                                              |
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
To run the math module in bot, you have to publish GrillBotMath project and set path to GrillBotMath.dll file into database config `$config addMethod /solve {"ProcessPath": "<HereYourPath>"}`

## GrillBot-Web
Readme for GrillBot-Web is [Here](GrillBot-Web)

## Permission system
Permission is documented in file [permissions.md](docs/permissions.md).
