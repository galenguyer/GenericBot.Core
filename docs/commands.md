# Commands

### Administrator Commands
| Command    | Options        | Description                                                              |
|------------|----------------|--------------------------------------------------------------------------|
| config     |                |                                                                          |
|            | adminRoles     | add/remove a role from the server's adminRoles via RoleId                |
|            | moderatorRoles | add/remove a role from the server's moderator roles via RoleId           |
|            | userRoles      | add/remove a role from the server's user rolesvia RoleId                 |
|            | twitter        | true/false allow tweets to be sent from this server                      |
|            | prefix         | Change the prefix. If you want it to end in a space, use `"`'s around it |
| createRole | roleName       | Create a new role with no permissions                                    |
| command(s) |                |                                                                          |
|            | list           | List all custom commands                                                 |
|            | add            | Add a new custom command                                                 |
|            | remove         | Remove a custom command                                                  |
|            | toggleDelete   | toggle whether or not a command is automatically deleted                 |
| alias      |                          |                                                                                |
|            | add <command>  <alias>   | Add an alias to a core or custom command                                       |
|            | remove <command> <alias> | Remove a previously set alias                                                  |
| leaveguild |                          | Make the bot leave the server                                                  |
| archive    |                          | Output the current text channel into a file (Will probably blow everything up) |

 ### Moderator Commands
  
| Command    | Options        | Description                                                              |
|-----------|----------------|-------------------------------------------------------------------------------------------------|
| membersOf | roleName       | Get all members of a role. If you do `.*` I'll personally be very annoyed at you                |
| getRole   | roleName       | Get the ID of a role.                                                                           |
| giveaway  |                |                                                                                                 |
|           | start          | Start a new giveaway and delete the old one                                                     |
|           | close          | Close a giveaway. This action is irreversible.                                                  |
|           | roll           | Roll for a winner of a giveaway. This can be done as many times as you want                     |
| clear     | <count> [user] | Clear <count> messages from a channel. If you mention a user it will only delete their messages |
| ping      | ["verbose"]    | Get the ping time for the bot                                                                   |

### User Commands

| Command    | Options        | Description                                                              |
|-----------|----------------|-------------------------------------------------------------------------------------------------|
| userroles |           | Display a list of available roles                                                     |
| iam       | roleName  | Join a userRole                                                                       |
| iamnot    | roleName  | Leave a userRole                                                                      |
| wat       |           | The best command                                                                      |
| reportBug | <bug>     | DM the bot creator with a bug report                                                  |
| markov    |           | Create a markov chain from the last messages in the channel                           |
| g         |           | Join the active giveaway, if any                                                      |
| tweet     | <tweet>   | Send a tweet from the [@GenericBoTweets](https://twitter.com/GenericBoTweets) account |
| help      | [command] | Get the help for the entire bot or just a specific command                            |
