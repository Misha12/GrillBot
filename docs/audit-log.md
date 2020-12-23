# Features: Audit log

Audit log storing logging data from selected events to database and periodicaly downloading Discord Audit log.
Downloading from Discord Audit log is because discord allows only a few months.

Audit logs can browse and delete from WebAdmin.

## Database

### Tables

Audit log database using more tables to store data. `DiscordUsers` to store users who occur in logs, `Files` to store files from specific events (such as MessageDeleted event) and `AuditLogs` to store logs.

#### AuditLogs

| Column       | Type          | Description                                                                                              |
| ------------ | ------------- | -------------------------------------------------------------------------------------------------------- |
| Id           | bigint        | Unique ID of log item.                                                                                   |
| CreatedAt    | datetime2     | Local datetime of log item creation.                                                                     |
| UserId       | bigint        | ID of user. Foreign key to table `DiscordUsers`                                                          |
| GuildId      | nvarchar(30)  | ID of guild where log item was created.                                                                  |
| DcAuditLogId | nvarchar(30)  | ID of Discord Audit log item. If log item was created from event, this column contains NULL.             |
| JsonData     | nvarchar(max) | Data of log item.                                                                                        |
| Type         | int           | Type of log item. See [AuditLogType.cs](../src/Grillbot/Enums/AuditLogType.cs) for detailed information. |

#### Files

| Column         | Type           | Description                                                      |
| -------------- | -------------- | ---------------------------------------------------------------- |
| Filename       | nvarchar(450)  | Name of stored file.                                             |
| Content        | varbinary(max) | Binary data of file.                                             |
| AuditLogItemId | bigint         | ID of assigned audit log item. Foreign key to table `AuditLogs`. |

## Commands

### log import `{loggerChannel}` (**Deprecated**)

Imports logger data from old logger channel and converts to new Audit logs.

| Parameter     | Type        | Description                                |
| ------------- | ----------- | ------------------------------------------ |
| loggerChannel | TextChannel | Channel identification (ID, Name, Mention) |

### log clear `{before}`

Removes old log before specific date.

| Parameter | Type     | Description                                                                                          |
| --------- | -------- | ---------------------------------------------------------------------------------------------------- |
| before    | DateTime | Break date and time. All records before this date will be deleted. DateTime is in `ISO 8601` format. |
