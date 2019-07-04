namespace Drolez.Commands
{
    using System;
    using System.Linq;
    using System.Net.WebSockets;
    using DW = Discord.WebSocket;

    /// <summary>
    /// Roles-list command
    /// Returns list of roles for specified guild and user
    /// </summary>
    [CommandInfo("roles-list")]
    public class ListRoles : ICommand
    {
        /// <summary>
        /// Run roles-list command
        /// </summary>
        /// <param name="socket">Web socket</param>
        /// <param name="user">Discord user who invoked command</param>
        /// <param name="parameters">Command parameters</param>
        /// <returns>True on success</returns>
        public bool Run(WebSocket socket, DW.SocketUser user, string[] parameters)
        {
            if (parameters.Length == 0 || string.IsNullOrWhiteSpace(parameters[0]))
            {
                // Not enough parameters
                return false;
            }

            ulong guildId = 0;

            if (!ulong.TryParse(parameters[0], out guildId))
            {
                // NO guild specified
                return false;
            }

            DW.SocketGuild guild = user.MutualGuilds.FirstOrDefault(mutualGuild => mutualGuild.Id == guildId);

            if (guild == null)
            {
                // User is not in this guild
                return false;
            }

            ulong userId = 0;

            if (parameters.Length > 1 && !string.IsNullOrWhiteSpace(parameters[1]) && ulong.TryParse(parameters[1], out userId))
            {
                // List roles for specific user
                socket.Send("rolesList", guild.GetUser(userId).Roles.Select(role => new Wrappers.Role(role)));
                return true;
            }

            // List all roles in guild
            socket.Send("rolesList", guild.Roles.Select(role => new Wrappers.Role(role)));
            return true;
        }
    }
}