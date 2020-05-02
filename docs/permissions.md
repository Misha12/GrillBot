# GrillBot Permission system

Permissions are stored in database table `MethodPerms`. It have relation to table `MethodsConfig`.
This implementation provides, that permissions are set only for one guild and cannot be abused with invitation bot to another server.

First configuration from discord requires bot administrator permission. Bot administrators are defined in `appsettings.json` in property `Administrators`.

## DB Table description

### `MethodsConfig`
| Column     | Type         | Description                                                                                      |
| ---------- | ------------ | ------------------------------------------------------------------------------------------------ |
| ID         | INT          | Unique ID of the method.                                                                         |
| GuildID    | Varchar(30)  | Discord guild ID.                                                                                |
| Group      | Varchar(100) | A group of commands. If not specified, there is an empty string.                                 |
| Command    | Varchar(100) | Command called from the appropriate discord server. If not specified, there is an empty string.  |
| ConfigData | Varchar(MAX) | JSON configuration of the appropriate method. If the method does not have a config, then there is an empty object. ("`{}`"). |
| OnlyAdmins | BIT          | Flag that the method is accessible only to bot administrators.                                   |
| UsedCount  | BIGINT       | Usage counter for statistical purposes.                                                          |

### `MethodPerms`
| Column    | Type        | Description                                                                 |
| --------- | ----------- | --------------------------------------------------------------------------- |
| PermID    | INT         | Unique ID of permisssion.                                                   |
| MethodID  | INT         | Method ID. Foreign key to table `MethodsConfig`.                            |
| DiscordID | Varchar(30) | Discord ID of role or user.                                                 |
| PermType  | INT         | Type of ID saved in column `DiscordID`. PermType = { Role = 0, User = 1 }   |
| AllowType | INT         | Flag that describes allowed or banned access.                               |

## Discord commands to manage permissions.

All commands to manage with permissions are stored in group named `config`.

### config addMethod `{commandInfo}` `{onlyAdmins}` `{configJson}`

Creates a method in database and save configuration.

| Parametr    | Type   | Description                                                                         |
| ----------- | ------ | ----------------------------------------------------------------------------------- |
| commandInfo | string | Pair of parameters separated with slash. `{group}/{command}`. Slash is a required symbol. |
| onlyAdmins  | bool   | Flag that describes access only for bot administrators.                             |
| configJson  | string | Method configuration stored in JSON format.                                         |

### config listMethods

List of method stored in database.

### config switchOnlyAdmins `{methodID}` `{onlyAdmins}`

Sets access only for bot administrators.

| Parametr   | Type | Description                                             |
| ---------- | ---- | ------------------------------------------------------  |
| methodID   | int  | Unique ID of the method.                                |
| onlyAdmins | bool | Flag that describes access only for bot administrators. |

### config updateJsonConfig `{methodID}` `{jsonConfig}`

Updates JSON configuration of method.

| Parametr   | Type   | Description                       |
| ---------- | ------ | --------------------------------- |
| methodID   | int    | Unique ID of the method.          |
| jsonConfig | string | JSON configuration.               |

### config addPermission `{methodID}` `{targetID}` `{permType}` `{allowType}`

Insert method permissions.

| Parametr  | Type   | Description                                                                                                          |
| --------- | ------ | -------------------------------------------------------------------------------------------------------------------- |
| methodID  | int    | Unique ID of the method.                                                                                             |
| targetID  | UInt64 | Discord ID of user or role.                                                                                          |
| permType  | int    | Type of ID inserted in parameter `targetID`. Value is from enum `PermType` {Role=0, User=1}                          |
| allowType | int    | Flag that describes allowed or banned access. Value is from enum `AllowType` {Allow=0, Deny=1}                       |

### config listPermissions `{methodID}`

List permissions of method.

| Parametr | Type | Description                       |
| -------- | ---- | --------------------------------- |
| methodID | int  | Unique ID of the method.          |

### config removePermission `{methodID}` `{permID}`

Remove method permissions.

| Parametr | Type | Description                          |
| -------- | ---- | ------------------------------------ |
| methodID | int  | Unique ID of the method.             |
| permID   | int  | Unique ID of permission.             |

### config getJsonConfig `{methodID}`

Obtain the JSON configuration for the method.

| Parametr | Type | Description                       |
| -------- | ---- | --------------------------------- |
| methodID | int  | Unique ID of the method.          |
