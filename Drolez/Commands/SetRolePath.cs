namespace Drolez.Commands
{
    using System.Linq;
    using System.Net.WebSockets;
    using System.Text.RegularExpressions;
    using DW = Discord.WebSocket;

    /// <summary>
    /// Role-set-path command
    /// Modify role path
    /// </summary>
    [CommandInfo("role-set-path")]
    public class SetRolePath : ICommand
    {
        /// <summary>
        /// Run role-set-path command
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

            // recostruct path
            string path = "/" + string.Join('/', parameters.Skip(2).Select(folder => Regex.Replace(folder, "^[0-9A-Za-z ]+$", string.Empty).Trim()).Where(folder => !string.IsNullOrWhiteSpace(folder)));

            DW.SocketGuild foundGuild = user.MutualGuilds.FirstOrDefault(guild => guild.Id == guildId);

            if (foundGuild != null)
            {
                DW.SocketGuildUser foundUser = foundGuild.GetUser(user.Id);

                if (foundUser != null && foundUser.Roles.Any(userRole => userRole.Permissions.Administrator) && foundUser.Roles.FirstOrDefault(role => role.Id == roleId) != null)
                {
                    Wrappers.Role.UpdateDatabase(roleId, path, guildId);
                    return true;
                }
            }

            return false;
        }
    }
}