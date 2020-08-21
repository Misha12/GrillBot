# GrillBot Permission system

Permissions are stored in database table `MethodPerms`. It have relation to table `MethodsConfig`.
This implementation provides, that permissions are set only for one guild and cannot be abused with invitation bot to another server.

First configuration from discord requires bot administrator permission. Bot administrators are defined in `appsettings.json` in property `Administrators`.

## DB Table description

### `MethodsConfig`

| Column     | Type         | Description                                                                                                                  |
| ---------- | ------------ | ---------------------------------------------------------------------------------------------------------------------------- |
| ID         | INT          | Unique ID of the method.                                                                                                     |
| GuildID    | Varchar(30)  | Discord guild ID.                                                                                                            |
| Group      | Varchar(100) | A group of commands. If not specified, there is an empty string.                                                             |
| Command    | Varchar(100) | Command called from the appropriate discord server. If not specified, there is an empty string.                              |
| ConfigData | Varchar(MAX) | JSON configuration of the appropriate method. If the method does not have a config, then there is an empty object. ("`{}`"). |
| OnlyAdmins | BIT          | Flag that the method is accessible only to bot administrators.                                                               |
| UsedCount  | BIGINT       | Usage counter for statistical purposes.                                                                                      |

### `MethodPerms`

| Column    | Type        | Description                                                               |
| --------- | ----------- | ------------------------------------------------------------------------- |
| PermID    | INT         | Unique ID of permisssion.                                                 |
| MethodID  | INT         | Method ID. Foreign key to table `MethodsConfig`.                          |
| DiscordID | Varchar(30) | Discord ID of role or user.                                               |
| PermType  | INT         | Type of ID saved in column `DiscordID`. PermType = { Role = 0, User = 1 } |
| AllowType | INT         | Flag that describes allowed or banned access.                             |

## Discord commands to manage permissions

All commands to manage with permissions are stored in group named `config`.

### config addMethod `{commandInfo}` `{onlyAdmins}` `{configJson}`

Creates a method in database and save configuration.

| Parameter   | Type   | Description                                                                               |
| ----------- | ------ | ----------------------------------------------------------------------------------------- |
| commandInfo | string | Pair of parameters separated with slash. `{group}/{command}`. Slash is a required symbol. |
| onlyAdmins  | bool   | Flag that describes access only for bot administrators.                                   |
| configJson  | JSON   | Method configuration stored in JSON format.                                               |

### config listMethods

List of method stored in database.

### config switchOnlyAdmins `{methodID}` `{onlyAdmins}`

Sets access only for bot administrators.

| Parameter  | Type              | Description                                                           |
| ---------- | ----------------- | --------------------------------------------------------------------- |
| method     | GroupCommandMatch | Unique ID (or text identification `{group}/{command}`) of the method. |
| onlyAdmins | bool              | Flag that describes access only for bot administrators.               |

### config updateJsonConfig `{methodID}` `{jsonConfig}`

Updates JSON configuration of method.

| Parameter  | Type              | Description                                                           |
| ---------- | ----------------- | --------------------------------------------------------------------- |
| method     | GroupCommandMatch | Unique ID (or text identification `{group}/{command}`) of the method. |
| jsonConfig | string            | JSON configuration.                                                   |

### config addPermission `{methodID}` `{targetID}` `{permType}` `{allowType}`

Insert method permissions.

| Parameter | Type              | Description                                                                                    |
| --------- | ----------------- | ---------------------------------------------------------------------------------------------- |
| method    | GroupCommandMatch | Unique ID (or text identification `{group}/{command}`) of the method.                          |
| targetID  | UInt64            | Discord ID of user or role.                                                                    |
| permType  | int               | Type of ID inserted in parameter `targetID`. Value is from enum `PermType` {Role=0, User=1}    |
| allowType | int               | Flag that describes allowed or banned access. Value is from enum `AllowType` {Allow=0, Deny=1} |

### config getMethod `{methodID}`

Get all informations about method.

| Parameter | Type              | Description                                                           |
| --------- | ----------------- | --------------------------------------------------------------------- |
| method    | GroupCommandMatch | Unique ID (or text identification `{group}/{command}`) of the method. |

### config removePermission `{methodID}` `{permID}`

Remove method permissions.

| Parameter | Type              | Description                                                           |
| --------- | ----------------- | --------------------------------------------------------------------- |
| method    | GroupCommandMatch | Unique ID (or text identification `{group}/{command}`) of the method. |
| permID    | int               | Unique ID of permission.                                              |

### config getJson `{methodID}`

Obtain the JSON configuration for the method.

| Parameter | Type              | Description                                                           |
| --------- | ----------------- | --------------------------------------------------------------------- |
| method    | GroupCommandMatch | Unique ID (or text identification `{group}/{command}`) of the method. |

### config removeMethod

Removes method configuration from database.

| Parameter | Type              | Description                                                           |
| --------- | ----------------- | --------------------------------------------------------------------- |
| method    | GroupCommandMatch | Unique ID (or text identification `{group}/{command}`) of the method. |

### config removeGuild

Removes all configurations and permissions for specified guild ID.

| Parameter | Type   | Description                 |
| --------- | ------ | --------------------------- |
| guildID   | UInt64 | Unique discord ID of guild. |

### config export

Exports full JSON configuration from context guild.

### config import

Batch import of configurations to database.
This method requires JSON file as attachment with JSON content of type `Array<MethodsConfig>`.

### config rename `{id}` `{group}` `{command}`

Rename method without any other configuration change.

| Parameter | Type   | Description                     |
| --------- | ------ | ------------------------------- |
| id        | int    | Unique ID  of the method.       |
| group     | string | New group name of the method.   |
| command   | string | New command name of the method. |
