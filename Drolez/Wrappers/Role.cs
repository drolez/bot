namespace Drolez.Wrappers
{
    using System;
    using System.Collections.Generic;
    using DNET = Discord;
    using DW = Discord.WebSocket;

    /// <summary>
    /// Discord role wrapper
    /// </summary>
    [Serializable]
    public class Role
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Role" /> class.
        /// </summary>
        /// <param name="role">Discord role</param>
        public Role(DW.SocketRole role)
        {
            this.Identifier = role.Id;
            this.Name = role.Name;
            this.Color = new List<int> { role.Color.R, role.Color.G, role.Color.B };
            this.Permissions = role.Permissions;
        }

        /// <summary>
        /// Gets or sets role color
        /// </summary>
        public List<int> Color { get; set; }

        /// <summary>
        /// Gets or sets identifier
        /// </summary>
        public ulong Identifier { get; set; }

        /// <summary>
        /// Gets or sets role name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets permissions
        /// </summary>
        public DNET.GuildPermissions Permissions { get; set; }
    }
}