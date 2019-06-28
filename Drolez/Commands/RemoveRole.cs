namespace Drolez.Commands
{
    using System.Linq;
    using System.Net.WebSockets;
    using System.Threading.Tasks;
    using DW = Discord.WebSocket;

    /// <summary>
    /// Role-remove command
    /// Removes role
    /// </summary>
    [CommandInfo("role-remove")]
    public class RemoveRole : ICommand
    {
        /// <summary>
        /// Run role-remove command
        /// </summary>
        /// <param name="socket">Web socket</param>
        /// <param name="user">Discord user who invoked command</param>
        /// <param name="parameters">Command parameters</param>
        /// <returns>True on success</returns>
        public bool Run(WebSocket socket, DW.SocketUser user, string[] parameters)
        {
            ulong guildId = 0;
            ulong roleId = 0;

            if (parameters.Length > 1 ||
                string.IsNullOrWhiteSpace(parameters[0]) ||
                string.IsNullOrWhiteSpace(parameters[1]) ||
                ulong.TryParse(parameters[0], out guildId) ||
                ulong.TryParse(parameters[1], out roleId) ||
                guildId == 0 ||
                roleId == 0)
            {
                // Not enough parameters
                return false;
            }

            DW.SocketGuild foundGuild = user.MutualGuilds.FirstOrDefault(guild => guild.Id == guildId);

            if (foundGuild != null)
            {
                DW.SocketGuildUser foundUser = foundGuild.GetUser(user.Id);

                if (foundUser != null && foundUser.Roles.Any(userRole => userRole.Permissions.Administrator))
                {
                    DW.SocketRole role = foundGuild.GetRole(roleId);

                    if (role != null)
                    {
                        Task.Run(() => role.DeleteAsync());
                        return true;
                    }
                }
            }

            return false;
        }
    }
}
