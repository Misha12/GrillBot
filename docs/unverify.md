# Features: Unverify

## Unverify

### Unverify example configuration

```json
{
    "MutedRoleID": 665205984330907668
}
```

- Muted role ID is optional. If role with this ID not exists, so it will be ignored

## Selfunverify

### Commands

| Command            | Parameters       | Parameter description                                                                        | Description                                |
| ------------------ | ---------------- | -------------------------------------------------------------------------------------------- | ------------------------------------------ |
| selfunverify       | time             | Time value have suffixes `m` - minutes, `h` - hours, `d` - days. Minimal time is 30 minutes. | Remove access to all channels.             |
|                    | [Optional] Roles | Roles which will not be removed. This roles have to be configured in selfunverify config.    |
| selfunverify roles | -                | -                                                                                            | List of optional roles, that can use keep. |

### Selfunverify example configuration

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
