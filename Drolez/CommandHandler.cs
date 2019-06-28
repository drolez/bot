namespace Drolez
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.WebSockets;
    using System.Threading.Tasks;
    using DW = Discord.WebSocket;

    /// <summary>
    /// Command handler
    /// </summary>
    public static class CommandHandler
    {
        /// <summary>
        /// List of all commands
        /// </summary>
        private static Dictionary<string, ICommand> commands = new Dictionary<string, ICommand>();

        /// <summary>
        /// All registered users
        /// </summary>
        private static Dictionary<WebSocket, DW.SocketUser> sockets = new Dictionary<WebSocket, DW.SocketUser>();

        /// <summary>
        /// Broadcast guild change to all relevan users
        /// </summary>
        /// <param name="guild">Discord guild</param>
        /// <param name="eventName">Event name</param>
        /// <returns>Completed task</returns>
        public static Task BroadcastGuildChange(DW.SocketGuild guild, string eventName)
        {
            if (eventName == "guildLeft")
            {
                DatabaseAccess.Command("DELETE * FROM `RoleFolders` WHERE `Guild`=`" + guild.Id + "`");
            }

            IEnumerable<Tuple<WebSocket, bool>> targets = guild.Users.ToList().Select(user =>
            {
                foreach (KeyValuePair<WebSocket, DW.SocketUser> socket in CommandHandler.sockets)
                {
                    if (socket.Value.Id == user.Id)
                    {
                        return new Tuple<WebSocket, bool>(socket.Key, user.Roles.FirstOrDefault(role => role.Permissions.Administrator) != null);
                    }
                }

                return null;
            }).Where(socket => socket != null);

            foreach (Tuple<WebSocket, bool> socket in targets)
            {
                try
                {
                    socket.Item1.Send(eventName, new Wrappers.Guild(guild, socket.Item2));
                }
                catch (Exception ex)
                {
                    ex.ToString();
                }
            }

            return Task.CompletedTask;
        }

        /// <summary>
        /// Broadcast role change to all relevant users
        /// </summary>
        /// <param name="role">Discord role</param>
        /// <param name="eventName">Event name</param>
        /// <returns>Completed task</returns>
        public static Task BroadcastRoleChange(DW.SocketRole role, string eventName)
        {
            IEnumerable<WebSocket> targets = role.Guild.Users.ToList().Select(user =>
            {
                foreach (KeyValuePair<WebSocket, DW.SocketUser> socket in CommandHandler.sockets)
                {
                    if (socket.Value.Id == user.Id)
                    {
                        return socket.Key;
                    }
                }

                return null;
            }).Where(socket => socket != null);

            foreach (WebSocket socket in targets)
            {
                try
                {
                    socket.Send(eventName, new Wrappers.Role(role));
                }
                catch (Exception ex)
                {
                    ex.ToString();
                }
            }

            return Task.CompletedTask;
        }

        /// <summary>
        /// Broadcast changed user to all his connected instances
        /// </summary>
        /// <param name="user">Discord user</param>
        /// <param name="eventName">Event name</param>
        /// <returns>Completed task</returns>
        public static Task BroadcastUserChange(DW.SocketUser user, string eventName)
        {
            foreach (KeyValuePair<WebSocket, DW.SocketUser> socket in CommandHandler.sockets)
            {
                if (socket.Value.Id == user.Id)
                {
                    socket.Key.Send(eventName, new Wrappers.User(user));
                }
            }

            return Task.CompletedTask;
        }

        /// <summary>
        /// Add client to message loop
        /// </summary>
        /// <param name="socket">Connected client</param>
        /// <param name="user">Discord user</param>
        public static void ClientAdd(WebSocket socket, DW.SocketUser user)
        {
            sockets.TryAdd(socket, user);
        }

        /// <summary>
        /// Remove client from message loop
        /// </summary>
        /// <param name="socket">Connected client</param>
        public static void ClientRemove(WebSocket socket)
        {
            if (sockets.ContainsKey(socket))
            {
                sockets.Remove(socket);
            }
        }

        /// <summary>
        /// Check if client got registered
        /// </summary>
        /// <param name="socket">Connected client</param>
        /// <returns>True if yes</returns>
        public static bool IsRegistered(WebSocket socket)
        {
            return sockets.ContainsKey(socket);
        }

        /// <summary>
        /// Load web commands
        /// </summary>
        public static void LoadCommands()
        {
            List<Type> commandsToLoad = typeof(CommandHandler).Assembly.GetTypes().Where(type => string.Equals(type.Namespace, "Drolez.Commands", StringComparison.Ordinal)).ToList();

            foreach (Type commandClass in commandsToLoad.Where(item => typeof(ICommand).IsAssignableFrom(item)))
            {
                try
                {
                    CommandInfo info = commandClass.GetCustomAttributes(typeof(CommandInfo), true).FirstOrDefault() as CommandInfo;

                    try
                    {
                        commands.Add(info.Command, (ICommand)Activator.CreateInstance(commandClass));

                        Console.WriteLine("Loaded command: " + info.Command);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Loading command '" + info.Command + "' failed: " + ex.ToString());
                    }
                }
                catch (Exception ex)
                {
                    ex.ToString();
                    Console.WriteLine("Loading command '" + commandClass.GetType().Name + "' failed: Command info missing!");
                }
            }
        }

        /// <summary>
        /// Process command input
        /// </summary>
        /// <param name="socket">Web socket client</param>
        /// <param name="commandMessage">Command requested</param>
        public static void ProcessCommand(WebSocket socket, string commandMessage)
        {
            // Test command, before interface gets implemented
            string[] commandStuff = commandMessage.Split('/').Select(part => part.Trim()).ToArray();
            string command = commandStuff.GetPart(0);

            if (!string.IsNullOrWhiteSpace(command) && CommandHandler.commands.ContainsKey(command))
            {
                if (!CommandHandler.commands[command].Run(socket, CommandHandler.sockets[socket], commandStuff.Skip(1).ToArray()))
                {
                    socket.Send("error", "Empty!");
                }

                return;
            }

            socket.Send("error", "Unknown!");
        }
    }
}