# Drolez bot
# Created with love for Discord Hack Week
This bot does management stuff, that Web API can't do.<br>
This bot contains WebSockets server with SSL support for .NET core<br>
Web dashboard using this bot: https://drolez.studio/

Written in C# for .NET core 2.2

Command/Event return JSON format:
```json
{"Data":{},"action":""}
```

# Websocket Commands:

Example of error return:
```json
{"Data":"Error message","action":"error"}
```
Example of roles-list return:
```json
{"Data":[{},{},,,,],"action":"rolesList"}
```

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
```json
{"Data":{"Identifier":0,,,,},"action":"userLeft"}
```

List of events that get send to connected clients:<br>
**guildJoined**, **guildLeft**, **roleCreated**, **roleDeleted**, **roleUpdated**, **userLeft**

# Extensibility

You can easilly add more commands to this bot.<br>
By implementing **ICommand** interface and adding **CommandInfo** attribute to it.<br>
Than just place it in the Commands folder, rebuild it and run!<br>
Commands in **Drolez.Commands** namespace will load automatically on start-up.
```C#
namespace Drolez.Commands
{
    using System.Net.WebSockets;
    using DW = Discord.WebSocket;

    /// <summary>
    /// string in the CommandInfo is the command name
    /// </summary>
    [CommandInfo("my-amazing-command")]
    public class MyAmazingCommand : ICommand
    {
        /// <summary>
        /// Run my amazing example command
        /// </summary>
        /// <param name="socket">Web socket</param>
        /// <param name="user">Discord user who invoked this command</param>
        /// <param name="parameters">Command parameters</param>
        /// <returns>True on success</returns>
        public bool Run(WebSocket socket, DW.SocketUser user, string[] parameters)
        {
            // First parameter is what client will see in Action parameter, second is and object (Data)
            socket.Send("myAmazingCommand", "Hello");
            return true;
        }
    }
}
```
