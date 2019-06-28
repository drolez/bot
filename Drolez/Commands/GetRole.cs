namespace Drolez.Commands
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.WebSockets;
    using System.Text;
    using DW = Discord.WebSocket;

    /// <summary>
    /// Roles command
    /// Returns list of roles for specified guild and user
    /// </summary>
    [CommandInfo("role")]
    public class GetRole : ICommand
    {
        /// <summary>
        /// Run roles command
        /// </summary>
        /// <param name="socket">Web socket</param>
        /// <param name="user">Discord user who invoked command</param>
        /// <param name="parameters">Command parameters</param>
        /// <returns>True on success</returns>
        public bool Run(WebSocket socket, DW.SocketUser user, string[] parameters)
        {
            if (parameters.Length == 0 || string.IsNullOrWhiteSpace(parameters[0]) || string.IsNullOrWhiteSpace(parameters[1]))
            {
                // Not enough parameters
                return false;
            }

            ulong roleId = 0;
            ulong guildId = 0;

            if (!ulong.TryParse(parameters[0], out guildId) || !ulong.TryParse(parameters[1], out roleId))
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

            DW.SocketRole foundRole = guild.GetUser(user.Id).Roles.FirstOrDefault(role => role.Id == roleId);

            if (foundRole != null)
            {
                socket.Send("role", new Wrappers.Role(foundRole));
                return true;
            }

            return false;
        }
    }
}
