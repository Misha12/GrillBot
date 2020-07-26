# Reminder

Reminder is feature for notification user at specific time.

When a reminder is sent, the user can postpone it for up to five hours.

## Database table

Reminder data are stored in table `Reminders`. Table have relation with table `DiscordUsers`.


| Column            | Type         | Description                                                                                         |
| ----------------- | ------------ | --------------------------------------------------------------------------------------------------- |
| RemindID          | BIGINT       | Unique ID of remind.                                                                                |
| UserID            | BIGINT       | ID of user who receives remind notification. Foreign key to table `DiscordUsers`.                   |
| FromUserID        | BIGINT       | ID of user who sending notification. If sender and receiver are the same, then the value is `NULL`. |
| At                | DateTime     | DateTime of notification                                                                            |
| Message           | Varchar(max) | Message for user.                                                                                   |
| PostponeCounter   | INT          | Counter for postpone leaderboard.                                                                   |
| RemindMessageID   | Varchar(30)  | The ID of the message that can be used to postpone notification.                                    |
| OriginalMessageID | Varchar(30)  | The ID of the message that created the notification.                                                |

## Commands

### remind me `{at}` `{message}`

Creates remind notification for user. Sender and receiver are the same.

| Parameter | Type     | Description                                                                                                                                                                                                                             |
| --------- | -------- | --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| at        | DateTime | DateTime of notification. DateTime have format `"dd/MM/yyyy HH:mm"`, `"dd/MM/yyyy HH:mm(:ss)"`, `ISO 8601`, `"dd. MM. yyyy HH:mm(:ss)"`. Seconds are optional. Don't forget the quotes, otherwise it won't read the datetime correctly. |
| message   | string   | Message for user.                                                                                                                                                                                                                       |

This command allow copy remind between users with 🙋 emoji as reaction.

### remind get

Get all reminders for caller user.

### remind user `{user}` `{at}` `{message}`

Creates remind notification for specific user.

| Parameter | Type     | Description                                                                                                                                                                                                                             |
| --------- | -------- | --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| user      | IUser    | User identification (Name, ID, Mention, ...)                                                                                                                                                                                            |
| at        | DateTime | DateTime of notification. DateTime have format `"dd/MM/yyyy HH:mm"`, `"dd/MM/yyyy HH:mm(:ss)"`, `ISO 8601`, `"dd. MM. yyyy HH:mm(:ss)"`. Seconds are optional. Don't forget the quotes, otherwise it won't read the datetime correctly. |
| message   | string   | Message for user.                                                                                                                                                                                                                       |

This command allow copy remind between users with 🙋 emoji as reaction.

### remind all

Get all reminders.

### remind cancel `{id}`

Cancels remind without notification.

| Parameter | Type | Description   |
| --------- | ---- | ------------- |
| id        | long | ID of remind. |

### remind notify `{id}`

Cancels remind with notification.

| Parameter | Type | Description   |
| --------- | ---- | ------------- |
| id        | long | ID of remind. |

### remind leaderboard

Leaderboard of top 10 procrastinators who are constantly postponing notifications.
