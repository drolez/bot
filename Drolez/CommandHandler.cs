namespace Drolez
{
    using Discord.WebSocket;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.WebSockets;

    /// <summary>
    /// Command handler
    /// </summary>
    public static class CommandHandler
    {
        /// <summary>
        /// All registered users
        /// </summary>
        private static Dictionary<WebSocket, SocketUser> sockets = new Dictionary<WebSocket, SocketUser>();

        /// <summary>
        /// Add client to message loop
        /// </summary>
        /// <param name="socket">Connected client</param>
        /// <param name="user">Discord user</param>
        public static void ClientAdd(WebSocket socket, SocketUser user)
        {
            sockets.TryAdd(socket, user);
        }

        /// <summary>
        /// Remove client from message loop
        /// </summary>
        /// <param name="socket">Connected client</param>
        public static void ClientRemove(WebSocket client)
        {
            if (sockets.ContainsKey(client))
            {
                sockets.Remove(client);
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
        /// Process command input
        /// </summary>
        /// <param name="socket">Websocket client</param>
        /// <param name="commandMessage">Command requested</param>
        public static void ProcessCommand(WebSocket socket, string commandMessage)
        {
            // Test command, before interface gets implemented
            string[] commandStuff = commandMessage.Split('/');
            string command = commandStuff.GetPart(0);
            string parameter1 = commandStuff.GetPart(1);

            if (command == "authorized" && !string.IsNullOrWhiteSpace(parameter1))
            {
                ulong user = 0;

                if (ulong.TryParse(parameter1, out user))
                {
                    SocketUser foundUser = Program.DiscordClient.GetUser(user);

                    if (foundUser != null)
                    {
                        string[] guilds = foundUser.MutualGuilds
                            .Where(guild => guild.Roles.FirstOrDefault(role => role.Permissions.Administrator) != null)
                            .Select(guild => guild.Id.ToString())
                            .ToArray();

                        socket.Send(" " + string.Join(',', guilds));
                        return;
                    }
                    else
                    {
                        socket.Send("ERR:NoUser!");
                        return;
                    }
                }
            }

            socket.Send("ERR:Unknown!");
        }
    }
}