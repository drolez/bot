namespace Drolez.Commands
{
    using System.Linq;
    using System.Net.WebSockets;
    using DW = Discord.WebSocket;
    using JSON = Newtonsoft.Json;

    /// <summary>
    /// Role-set command
    /// Modify or add role
    /// </summary>
    [CommandInfo("role-set")]
    public class SetRole : ICommand
    {
        /// <summary>
        /// Run role-set command
        /// </summary>
        /// <param name="socket">Web socket</param>
        /// <param name="user">Discord user who invoked command</param>
        /// <param name="parameters">Command parameters</param>
        /// <returns>True on success</returns>
        public bool Run(WebSocket socket, DW.SocketUser user, string[] parameters)
        {
            ulong guildId = 0;

            if (parameters.Length > 1 ||
                string.IsNullOrWhiteSpace(parameters[0]) ||
                string.IsNullOrWhiteSpace(parameters[1]) ||
                ulong.TryParse(parameters[0], out guildId) ||
                guildId == 0)
            {
                // Not enough parameters
                return false;
            }

            // reconstruct botched JSON :D
            string json = string.Join('/', parameters.Skip(1).ToArray());
            Wrappers.Role role = JSON.JsonConvert.DeserializeObject<Wrappers.Role>(json);
            DW.SocketGuild foundGuild = user.MutualGuilds.FirstOrDefault(guild => guild.Id == guildId);

            if (role != null && foundGuild != null)
            {
                DW.SocketGuildUser foundUser = foundGuild.GetUser(user.Id);

                if (foundUser != null && foundUser.Roles.Any(userRole => userRole.Permissions.Administrator))
                {
                    role.Save(foundGuild);
                    return true;
                }
            }

            return false;
        }
    }
}