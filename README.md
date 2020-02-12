# GrillBot

## Requirements
- MSSQL server 
  - Instalation: https://www.microsoft.com/en-us/sql-server/sql-server-downloads 
- Microsoft Visual Studio (2017 and later) (or another IDE supports .NET)
  - Visual studio instalation: https://docs.microsoft.com/cs-cz/visualstudio/install/install-visual-studio?view=vs-2019
- .NET Core 2.2 (with ASP\.NET Core 2.2)
  - https://dotnet.microsoft.com/download/dotnet-core/2.2

## Continuous integration
[![Build Status](https://dev.azure.com/mhalabica/GrillBot/_apis/build/status/Misha12.GrillBot?branchName=master)](https://dev.azure.com/mhalabica/GrillBot/_build/latest?definitionId=8&branchName=master)

## Used NuGet packages

### GrillBot
- Discord.NET
- Discord.Addons.Interactive
- Microsoft.AspNetCore.App
- Microsoft.AspNetCore.Razor.Design
- Microsoft.EntityFrameworkCore.SqlServer
- Microsoft.VisualStudio.CodeGeneration.Design
- UnicodeEmoji.NET

### GrillBotMath
- Newtonsoft.JSON
- MathParser.org-mXParser

## Database
You can create database with scripts in `GrillBot-DB` project. If you're using Visual Studio on Windows, you can create migration script for your SQL Server instance.

## Config (appsettings.json)
**Keys in bold must be setup to develop GrillBot locally**
- Format: **JSON**
- Filename: **appsettings.json**

### Models
#### Config

|Key|Type|Description|
|---|---|---|
|AllowedHosts|string|Semicollon delimited list of allowed hostnames without port numbers.|
|CommandPrefix|string|Message content, that must starts to invoke command.|
|Database|string|Connection string to MSSQL database.|
|Administrators|string[]|List of bot administrators. Can use bot independently of roles. Value is user ID.|
|EmoteChain_CheckLastCount|int|Count of same emotes before bot send emote.|
|Discord|[Config.Discord](#Config.Discord)|Service configuration|
|Log|[Config.Log](#Config.Log)|Logging configuration|

#### Config.Discord
For properties **Token**, **ClientId**, **ClientSecret** you will need to create your own Discord Application to get a Token for local development.

|Key|Type|Description|
|---|---|---|
|Activity|string|Now playing game info.|
|Token|string|Login token|
|UserJoinedMessage|string|Message, that will be sent, when user joined to guild.|
|LoggerRoomID|string|ID of channel to send logging data (MessageEdited, MessageDeleted, ...).|
|ClientId|string|ID of application in discord OAuth Service.|
|ClientSecret|string|Secret key for authentication in Discord OAuth service.|
|ServerBoosterRoleId|string|ID of role with Nitro Server Booster role.|
|AdminChannelID|string|ID of channel for administration purposes.

#### Config.Log

|Key|Type|Description|
|---|---|---|
|LogRoomID|string|ID of channel to send logging data such as errors.

## GrillBotMath
To run the math module in bot, you have to publish GrillBotMath project and set path to GrillBotMath.dll file into database config `$config addMethod /solve {"ProcessPath": "<HereYourPath>"}`

## GrillBot-Web
Readme for GrillBot-Web is [Here](GrillBot-Web)

## Permission system
Permissions are stored in database in tables `MethodsConfig` and `MethodPerms`. For correct functionality you have to define at lease one administrator in `Config.Administrators`. Then you can use methods `config addMethod` and `config addPermission` to add other methods and permissions. 