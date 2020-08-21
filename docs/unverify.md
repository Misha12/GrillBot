# Unverify

Unverify is a functionality that allows the user on the server to remove all rights for a certain period of time.

Unverify has 2 types.

- **Unverify** (The user removes all rights to another user)
- **SelfUnverify** (The user removes all rights to himself.).

## Unverify configuration

Unverify have two JSON configurations.

### Primary unverify config

Required in both types of unverify.

#### Unverify example configuration

```json
{
    "MutedRoleID": 665205984330907668
}
```

- Muted role ID is optional if server have not muted role, set `0`. If role with this ID not exists, so it will be ignored.

### SelfUnverify config

Required in SelfUnverify. In unverify is SelfUnverify config ignored. In SelfUnverify is throwed exception.

#### SelfUnverify example configuration

`_` character is special key for roles, that not have group.

```json
{
    "MaxRolesToKeep": 5,
    "RolesToKeep": {
        "ABC": [ "def", "ghch", "ijkl", "mno" ],
        "_": [ "pqr", "etc" ]
    }
}
```

Roles is alias for items. It will later renamed to `Items`.

## Database tables

### Unverify

| Column            | Type          | Description                                                                                                                         |
| ----------------- | ------------- | ----------------------------------------------------------------------------------------------------------------------------------- |
| UserID            | BIGINT        | ID of user that have unverify. ForeignKey to table `DiscordUsers`.                                                                  |
| StartDateTime     | DATETIME2     | Date and time the user was denied access.                                                                                           |
| EndDateTime       | DATETIME2     | Date and time when access will be returned to the user.                                                                             |
| Reason            | NVARCHAR(MAX) | Reason of unverify.                                                                                                                 |
| Roles             | NVARCHAR(MAX) | An array of role IDs that the user had before removing access. Serialized as JSON array.                                            |
| Channels          | NVARCHAR(MAX) | An array of channels (and override values) that the user had before removing access. Serialized as JSON array of objects.           |
| SetLogOperationID | BIGINT        | ID of starting unverify operation. For `unverify list` command to reconstruct user's unverify. Foreign key to table `UnverifyLogs`. |

### UnverifyLog

| Column     | Type          | Description                                                                                            |
| ---------- | ------------- | ------------------------------------------------------------------------------------------------------ |
| ID         | BIGINT        | Unique ID of unverify operation                                                                        |
| Operation  | INT           | Type of unverify operations (Unverify, SelfUnverify, AutoRemove, Remove, Update)                       |
| FromUserID | BIGINT        | The ID of the user who performed the operation. ForeignKey to table `DiscordUsers`.                    |
| ToUserID   | BIGINT        | The ID of the user on whom the operation was performed. ForeignKey to table `DiscordUsers`.            |
| CreatedAt  | DATETIME2     | Date and time when the operation was performed.                                                        |
| JsonData   | NVARCHAR(MAX) | Detailed data of operation in JSON format. It has different models according to the type of operation. |

## Commands

### unverify `{time}` `{reason}` `{[tags]}`

Removes all rights from the user for a period of time.

| Parameter | Type   | Description                                                                                                                                                                          |
| --------- | ------ | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ |
| time      | string | Time of unverify. Format is `{time}{m/h/d/M/y}`, or `ISO 8601`. For example: `30m` or `2020-08-17T23:59:59`. **m**: minutes, **h**: hours, **d**: days, **M**: months, **y**: years. |
| reason    | string | Reason of unverify.                                                                                                                                                                  |
| tags      | User[] | Field to indicate the users to whom access is to be removed.                                                                                                                         |

### unverify remove `{identification}`

Early return of access.

| Parameter | Type | Description                                |
| --------- | ---- | ------------------------------------------ |
| user      | User | Tag, id or name (username, alias) of user. |

### unverify list

List of all people who have temporarily removed access.

### unverify update `{time}` `{identification}`

Update the time in the temporary access removal record.

| Parameter | Type   | Description                                                                                                                                                                         |
| --------- | ------ | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| time      | string | Time of unverify. Format is `{time}{m/h/d/M/y}`, or `ISO 8601`. For example: `30m` or `2020-08-17T23:59:59`. **m**: minutes, **h**: hours, **d**: days, **M**: months, **y**: years |
| user      | User   | Tag, id or name (username, alias) of user.                                                                                                                                          |

### unverify stats

Real Time statistics unverify

### selfunverify `{time}` `{[toKeep]}`

Removing rights to yourself.

| Parameter | Type     | Description                                                                                                                                                                         |
| --------- | -------- | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| time      | string   | Time of unverify. Format is `{time}{m/h/d/M/y}`, or `ISO 8601`. For example: `30m` or `2020-08-17T23:59:59`. **m**: minutes, **h**: hours, **d**: days, **M**: months, **y**: years |
| toKeep    | string[] | Array of role names or channel names, which the user wishes to keep.                                                                                                                |

### selfunverify defs

Definition of accesses that the user can keep.
