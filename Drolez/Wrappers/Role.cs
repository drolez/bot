namespace Drolez.Wrappers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
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
            this.Color = new byte[3] { role.Color.R, role.Color.G, role.Color.B };

            // Add permissions
            this.Permissions = new List<Tuple<string, bool>>();
            this.Permissions.AddRange(
                role.Permissions.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .Where(property => property.PropertyType == typeof(bool))
                .Select(property => new Tuple<string, bool>(property.Name, (bool)property.GetValue(role.Permissions))));
        }

        /// <summary>
        /// Gets or sets role color
        /// </summary>
        public byte[] Color { get; set; }

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
        private List<Tuple<string, bool>> Permissions { get; set; }
    }
}