# Drolez bot
This bot does management stuff, that Web API can't do.<br>
This bot contains WebSockets server with SSL support for .NET core

Written in C# for .NET core 2.2

Command/Event return JSON format: **{"Data":object,"action":string}**<br>

# Websocket Commands:

Example of error return: **{"Data":"Error message","action":"error"}**<br>
Example of roles-list return: **{"Data":roleListJSONObject,"action":"rolesList"}**

**auth/\<token>/\<TimeToLive(seconds)>**<br>
Returns: true on success<br>
Desc: On fail kicks client

**roles-list/\<guildId>**<br>
Returns: list of roles in a json format<br>
Desc: On fail either returns "ERR:Empty!" or "ERR:Unknown!"
  
**roles-list/\<guildId>/\<userId>**<br>
Returns: list of roles for specified user in a json format<br>
Desc: On fail either returns "ERR:Empty!" or "ERR:Unknown!"

**guilds/\<userId>**<br>
Returns: list of guilds for specified user in a json format<br>
Desc: On fail either returns "ERR:Empty!" or "ERR:Unknown!"

**role/\<guildId>/\<userId>**<br>
Returns: Role for specified user in guild in a json format<br>
Desc: On fail either returns "ERR:Empty!" or "ERR:Unknown!"

**role-set/\<guildId>/\<roleJSON>**<br>
Result: if role ID is 0, command will create a new role, otherwise it modifies existing one<br>
Desc: On fail either returns "ERR:Empty!" or "ERR:Unknown!"

**role-remove/\<guildId>/\<roleId>**<br>
Result: Removes existing role<br>
Desc: On fail either returns "ERR:Empty!" or "ERR:Unknown!"

**role-set-path/\<guildId>/\<roleId>/\<path>**<br>
Result: Changes directory path to the role (path eg.: */path/to/role/folder*)<br>
Desc: On fail either returns "ERR:Empty!" or "ERR:Unknown!"

**ping**<br>
Returns: pong

# Websocket Events:

Event JSON format:<br>
Data can contain *Role*, *Guild*, *User* object<br>
Action contains event name (see below).

List of events that get send to connected clients:<br>
**guildJoined**, **guildLeft**, **roleCreated**, **roleDeleted**, **roleUpdated**, **userLeft**
