# Features: Users management

Users management is a feature for managing users on guilds.

## User statistics

Each user on the server is monitored for his activity and statistics are generated accordingly.

Statistics include the following:

- Points can be awarded for:
  - For each message the user writes, they can get a random amount of 15-25 points. This gain is limited to happen once every 1 minute.
  - For each reaction the user clicks he can get 0-10 points. This gain is limited to 30 seconds since the last reaction point.
- For giving reactions.
- For obtaining reactions.
- Message count
  - The number of messages the user typed. Message counting is separated by channels.

## Api access

The user can access the GrillBot REST API. The API may be restricted by client type.
[VUT FIT Production OpenAPI](https://grillbot.cloud/swagger)

### Control commands

#### user generateApiToken `{userMention}`

Creates token for API access. Token is GUID.

| Parameter   | Type | Description                                |
| ----------- | ---- | ------------------------------------------ |
| userMention | User | Tag, id or name (username, alias) of user. |

#### user releaseApiToken `{userMention}`

Releases token for API access.

| Parameter   | Type | Description                                |
| ----------- | ---- | ------------------------------------------ |
| userMention | User | Tag, id or name (username, alias) of user. |

## Web administration

The user can access the bot web administration.

### Control commands

#### user addToWebAdmin `{userMention}`

Creates password for user and web admin access. Password is sended to destination user private message channel.

| Parameter   | Type | Description                                |
| ----------- | ---- | ------------------------------------------ |
| userMention | User | Tag, id or name (username, alias) of user. |

#### user removeFromWebAdmin `{userMention}`

Removes password and web admin access.

| Parameter   | Type | Description                                |
| ----------- | ---- | ------------------------------------------ |
| userMention | User | Tag, id or name (username, alias) of user. |

## Other commands

### user info `{userMention}`

Gives full statistics and information about user.

Command returns in embed:

- ID (Discord snowflake)
- Username (nickname can be included)
- State (Online/Idle/Do Not Disturb/Offline)
- Account creation datetime
- When account was joined in guild (and order)
- Voice mute (Server side and client side)
- Roles
- Current points value
- Given and Obtained reactions
- Total message count
- Unverify statistics
- Permissions
- Active clients (if any)
- The most active text channel
- Channel of last message (and when)
- Detail flags (WebAdminAccess, ApiAccess, GuildOwner, Birthday, BotAdmin)
- Webadmin and api call statistics
  
| Parameter   | Type | Description                                |
| ----------- | ---- | ------------------------------------------ |
| userMention | User | Tag, id or name (username, alias) of user. |

### user access `{user}`

List of channels, where user have access. Ordered by channel position.

| Parameter | Type | Description                                |
| --------- | ---- | ------------------------------------------ |
| user      | User | Tag, id or name (username, alias) of user. |

### user setBotAdmin `{user}` `{isAdmin}`

Sets or removes full power permission.

| Parameter | Type | Description                                                    |
| --------- | ---- | -------------------------------------------------------------- |
| user      | User | Tag, id or name (username, alias) of user.                     |
| isAdmin   | bool | A flag that specifies whether the user is a bot administrator. |

### me

Simplified method of `user info {userMention}`.

Command **not returns** (instead of `user info`):

- Permissions
- Active clients
- The most active text channel
- Channel of last message (and when)
- Detail flags
- WebAdmin and API call statistics
