# GrillBot

## Requirements
- MSSQL server.
- Microsoft Visual Studio (2017 and later) (or another IDE)
- .NET Core 2.2 (with ASP.NET Core 2.2)

## Continuous integration
[![Build Status](https://dev.azure.com/mhalabica/GrillBot/_apis/build/status/GrillBot-CI?branchName=master)](https://dev.azure.com/mhalabica/GrillBot/_build/latest?definitionId=4&branchName=master)

## Used NuGet packages

### GrillBot
- Discord .NET
- Microsoft.AspNetCore.App
- Microsoft.AspNetCore.Razor.Design
- Microsoft.EntityFrameworkCore.SqlServer
- Microsoft.VisualStudio.CodeGeneration.Design

### GrillBotMath
- Newtonsoft.JSON
- MathParser.org-mXParser

## Config (appsettings.json)
**Keys in bold must be setup to develop GrillBot locally**
- Format: **JSON**
- Config parts
  - AllowedHosts (string): Semicollon delimited list of allowed hostnames without port numbers.
  - CommandPrefix (string): Message content, that must contain to invoke command.
  - **Database** (string): Connection string to MSSQL database. **If you don't want to setup Db locally ask Misha for remote connection string**
  - Discord: Bot configuration
    - Activity (string): Activity message
    - **Token** (string): Bot login token **You will need to create your own Discord Application to get a Token for local development**
    - UserJoinedMessage (string): Message, that will be sent, when user joined to guild.
    - Administrators (string[]): List of bot administrators. Can use bot independently of roles.
    - LoggerRoomID (string): ID of channel to send logging data.
  - Log: Bot logging configuration
    - LogToDiscord: Sending errors to discord room.
      - Enabled (bool)
      - Room (ulong): Channel ID
  - MethodsConfig: Bot features configuration.
    - *In common*:
      - RequireRoles (string[]): List of required roles. User must have at least one of these roles.
      - IsDisabled (bool): Deactivated command.
    - Greeting (grillhi command):
      - Message (string): Bots response.
      - AppendEmoji (string): Local server emoji, that will be appended to Message.
      - OutputMode (string): Default output mode. Supported is 'bin', 'text', 'hexa'
    - GrillStatus: Grillbot diagnostics.
    - Help
    - Channelboard: Channelboard commands configuration.
      - Web: REST API and client configuration.
        - TokenValidMins (int): Time in minutes, to remove token from memory.
        - Url (string): Url to client.
    - Images
      - NudesDataPath (string): Path to directory of images. Bot take one of these images on nudes command.
      - NotNudesDataPath (string): Path to directory of images. Bot take one of these images on notnudes command.
      - AllowedDataPath (string[]): List of image extensions.
    - RoleManager
    - Math:
      - ComputingTime (int): Time in miliseconds to compute. When time is up, computing will be killed.
      - ProcessPath (string): Path to executable dll to computing engine.
    - AutoReply
  - EmoteChain:
    - CheckLastN (int): Count of same emotes before bot send emote.
    
## GrillBotMath
To run the math module in bot, you have to publish GrillBotMath project and set path to GrillBotMath.dll file into appsettings.json (MethodsConfig.Math.ProcessPath)

## GrillBot-Web
Readme for GrillBot-Web is [Here](GrillBot-Web)
