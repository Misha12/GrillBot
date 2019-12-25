# GrillBot

## Requirements
- MSSQL server.
- Microsoft Visual Studio (2017 and later) (or another IDE)
- .NET Core 2.2 (with ASP\.NET Core 2.2)

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
|Database|string|Connection string to MSSQL database. **If you don't want to setup DB locally ask [owner](http://github.com/Misha12) for remote connection string.**|
|Administrators|string[]|List of bot administrators. Can use bot independently of roles. Value is user ID.|
|EmoteChain_CheckLastCount|int|Count of same emotes before bot send emote.|
|Discord|[Config.Discord](#Config.Discord)|Service configuration|
|Log|[Config.Log](#Config.Log)|Logging configuration|
|MethodsConfig|[Config.MethodsConfig](#Config.MethodsConfig)|Features configuration|

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

#### Config.Log

|Key|Type|Description|
|---|---|---|
|LogRoomID|string|ID of channel to send logging data such as errors.

#### Config.MethodsConfig

|Key|Type
|---|---|
|Greeting|[Config.MethodsConfig.Greeting](#Config.MethodsConfig.Greeting)|
|GrillStatus|[Config.MethodsConfig.GrillStatus](#Config.MethodsConfig.GrillStatus)|
|Help|[Config.MethodsConfig.Help](#Config.MethodsConfig.Help)|
|Channelboard|[Config.MethodsConfig.Channelboard](#Config.MethodsConfig.Channelboard)|
|MemeImages|[Config.MethodsConfig.MemeImages](#Config.MethodsConfig.MemeImages)|
|RoleManager|[Config.MethodsConfig.RoleManager](#Config.MethodsConfig.RoleManager)|
|Math|[Config.MethodsConfig.Math](#Config.MethodsConfig.Math)|
|AutoReply|[Config.MethodsConfig.AutoReply](#Config.MethodsConfig.AutoReply)|
|TeamSearch|[Config.MethodsConfig.TeamSearch](#Config.MethodsConfig.TeamSearch)|
|EmoteManager|[Config.MethodsConfig.EmoteManager](#Config.MethodsConfig.EmoteManager)|
|TempUnverify|[Config.MethodsConfig.TempUnverify](#Config.MethodsConfig.TempUnverify)|
|Admin|[Config.MethodsConfig.Admin](#Config.MethodsConfig.Admin)|
|CReference|[Config.MethodsConfig.CReference](#Config.MethodsConfig.CReference)|
|SelfUnverify|[Config.MethodsConfig.SelfUnverify](#Config.MethodsConfig.SelfUnverify)|

#### Config.MethodsConfig.Permissions

|Key|Type|Description|
|---|---|---|
|RequireRoles|string[]|List of required roles. User must have at least one of these rolese. Value is role name.|
|AllowedUsers|string[]|List of users with allowed access to feature. Value is user ID.|
|BannedUsers|string[]|List of users with disabled access to feature. Value is user ID.|
|OnlyAdmins|bool|Feature is allowed only for users with Administration permission.|

#### Config.MethodsConfig.Greeting

|Key|Type|Description|
|---|---|---|
|Permissions|[Config.MethodsConfig.Permissions](#Config.MethodsConfig.Permissions)|
|MessageTemplate|string|Bots response.|
|OutputMode|string|Default output mode. Supported is 'bin', 'text', 'hexa'.|

#### Config.MethodsConfig.GrillStatus

|Key|Type|Description|
|---|---|---|
|Permissions|[Config.MethodsConfig.Permissions](#Config.MethodsConfig.Permissions)|

#### Config.MethodsConfig.Help

|Key|Type|Description|
|---|---|---|
|Permissions|[Config.MethodsConfig.Permissions](#Config.MethodsConfig.Permissions)|

#### Config.MethodsConfig.Channelboard

|Key|Type|Description|
|---|---|---|
|Permissions|[Config.MethodsConfig.Permissions](#Config.MethodsConfig.Permissions)|
|WebTokenValidMinutes|int|Time in minutes, to remove token from memory.
|WebUrl|string|URL to channelboard site.

#### Config.MethodsConfig.MemeImages

|Key|Type|Description|
|---|---|---|
|Permissions|[Config.MethodsConfig.Permissions](#Config.MethodsConfig.Permissions)|
|NudesDataPath|string|Path to directory of images. Bot take one of these images on nudes command.
|NotNudesDataPath|string|Path to directory of images. Bot take one of these images on notnudes command.
|AllowedImageTypes|string[]|List of supported image types.

#### Config.MethodsConfig.RoleManager

|Key|Type|Description|
|---|---|---|
|Permissions|[Config.MethodsConfig.Permissions](#Config.MethodsConfig.Permissions)|

#### Config.MethodsConfig.Math

|Key|Type|Description|
|---|---|---|
|Permissions|[Config.MethodsConfig.Permissions](#Config.MethodsConfig.Permissions)|
|ComputingTime|int|Time in miliseconds to compute. When time is up, computing will be killed. Boosters have double time for computing.|
|ProcessPath|string|Path to executable dll to computing engine.|

#### Config.MethodsConfig.AutoReply

|Key|Type|Description|
|---|---|---|
|Permissions|[Config.MethodsConfig.Permissions](#Config.MethodsConfig.Permissions)|

#### Config.MethodsConfig.TeamSearch

|Key|Type|Description|
|---|---|---|
|Permissions|[Config.MethodsConfig.Permissions](#Config.MethodsConfig.Permissions)|
|GeneralCategoryID|ulong|ID of category with grouped searches.

#### Config.MethodsConfig.EmoteManager

|Key|Type|Description|
|---|---|---|
|Permissions|[Config.MethodsConfig.Permissions](#Config.MethodsConfig.Permissions)|

#### Config.MethodsConfig.TempUnverify

|Key|Type|Description|
|---|---|---|
|Permissions|[Config.MethodsConfig.Permissions](#Config.MethodsConfig.Permissions)|
|MainAdminID|string|ID of main administrator. This user will receive all problems with temp unverify.|

#### Config.MethodsConfig.Admin

|Key|Type|Description|
|---|---|---|
|Permissions|[Config.MethodsConfig.Permissions](#Config.MethodsConfig.Permissions)|

#### Config.MethodsConfig.CReference

|Key|Type|Description|
|---|---|---|
|Permissions|[Config.MethodsConfig.Permissions](#Config.MethodsConfig.Permissions)|

#### Config.MethodsConfig.SelfUnverify

|Key|Type|Description|
|---|---|---|
|Permissions|[Config.MethodsConfig.Permissions](#Config.MethodsConfig.Permissions)|
    
## GrillBotMath
To run the math module in bot, you have to publish GrillBotMath project and set path to GrillBotMath.dll file into appsettings.json (Config.MethodsConfig.Math.ProcessPath)

## GrillBot-Web
Readme for GrillBot-Web is [Here](GrillBot-Web)
