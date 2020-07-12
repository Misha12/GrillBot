# Reminder

Reminder is feature for notification user at specific time.

## Database table

Reminder data are stored in table `Reminders`. Table have relation with table `DiscordUsers`.


| Column     | Type         | Description                                                                                         |
| ---------- | ------------ | --------------------------------------------------------------------------------------------------- |
| RemindID   | BIGINT       | Unique ID of remind.                                                                                |
| UserID     | BIGINT       | ID of user who receives remind notification. Foreign key to table `DiscordUsers`.                   |
| FromUserID | BIGINT       | ID of user who sending notification. If sender and receiver are the same, then the value is `NULL`. |
| At         | DateTime     | DateTime of notification                                                                            |
| Message    | Varchar(max) | Message for user.                                                                                   |

## Commands

### remind me `{at}` `{message}`

Creates remind notification for user. Sender and receiver are the same.

| Parameter | Type     | Description                                                                                          |
| --------- | -------- | ---------------------------------------------------------------------------------------------------- |
| at        | DateTime | DateTime of notification. DateTime have format `"dd. MM. yyyy HH:mm:ss"` or `"yyyy-MM-ddTHH:mm:ss"`. |
| message   | string   | Message for user.                                                                                    |

### remind get

Get all reminders for caller user.

### remind user `{user}` `{at}` `{message}`

Creates remind notification for specific user.

| Parameter | Type     | Description                                                                                          |
| --------- | -------- | ---------------------------------------------------------------------------------------------------- |
| user      | IUser    | User identification (Name, ID, Mention, ...)                                                         |
| at        | DateTime | DateTime of notification. DateTime have format `"dd. MM. yyyy HH:mm:ss"` or `"yyyy-MM-ddTHH:mm:ss"`. |
| message   | string   | Message for user.                                                                                    |

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
