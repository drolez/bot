namespace Drolez.Commands
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.WebSockets;
    using DW = Discord.WebSocket;

    /// <summary>
    /// Authorized command
    /// Returns list of guilds where user is an admin
    /// </summary>
    [CommandInfo("authorized")]
    public class Authorized : ICommand
    {
        /// <summary>
        /// Run authorized command
        /// </summary>
        /// <param name="socket">Web socket</param>
        /// <param name="user">Discord user who invoked command</param>
        /// <param name="parameters">Command parameters</param>
        public bool Run(WebSocket socket, DW.SocketUser user, string[] parameters)
        {
            if (parameters.Length == 0 || string.IsNullOrWhiteSpace(parameters[0]))
            {
                // Not enough parameters
                return false;
            }
            
            ulong userId = 0;

            if (ulong.TryParse(parameters[0], out userId))
            {
                DW.SocketUser foundUser = Program.DiscordClient.GetUser(userId);

                if (foundUser != null)
                {
                    IEnumerable<Wrappers.Guild> guilds = foundUser.MutualGuilds
                        .Where(guild => guild.Roles.FirstOrDefault(role => role.Permissions.Administrator) != null)
                        .Select(guild => new Wrappers.Guild(guild));

                    socket.Send(" " + guilds.ToJSON());
                    return true;
                }
            }

            return false;
        }
    }
}
