﻿namespace Drolez.Wrappers
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.SqlClient;
    using System.Linq;
    using System.Text.RegularExpressions;
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
        public Role()
        {
            // Empty
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Role" /> class.
        /// </summary>
        /// <param name="role">Discord role</param>
        public Role(DW.SocketRole role)
        {
            this.GuildIdentifier = role.Guild.Id;
            this.Identifier = role.Id;
            this.Name = role.Name;
            this.Color = new List<int> { role.Color.R, role.Color.G, role.Color.B };
            this.Permissions = role.Permissions;

            SqlDataReader reader = DatabaseAccess.Read("SELECT * FROM RoleFolders WHERE Id=" + this.Identifier.ToString());

            if (reader == null)
            {
                this.Path = "/";
            }
            else
            {
                reader.NextResult();
                string fromDb = reader.RecordToString(1);

                this.Path = string.IsNullOrWhiteSpace(fromDb) ? "/" : fromDb;
            }
        }

        /// <summary>
        /// Gets or sets role color
        /// </summary>
        public List<int> Color { get; set; }

        /// <summary>
        /// Gets or sets guild identifier
        /// </summary>
        public ulong GuildIdentifier { get; set; }

        /// <summary>
        /// Gets or sets identifier
        /// </summary>
        public ulong Identifier { get; set; }

        /// <summary>
        /// Gets or sets role name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets item order
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        /// Gets or sets permissions
        /// </summary>
        public DNET.GuildPermissions Permissions { get; set; }

        /// <summary>
        /// Save role settings to DB
        /// </summary>
        /// <param name="guild">Discord guild containing role</param>
        /// <returns>True on success modify/insert</returns>
        public bool Save(DW.SocketGuild guild = null)
        {
            if (!string.IsNullOrWhiteSpace(this.Path))
            {
                this.Path = "/" + string.Join(
                    '/',
                    this.Path.Trim().Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(folder => Regex.Replace(folder, "^[0-9A-Za-z ]+$", string.Empty).Trim())
                    .Where(folder => string.IsNullOrWhiteSpace(folder)));

                if (string.IsNullOrWhiteSpace(this.Path) || !this.Path.StartsWith('/'))
                {
                    this.Path = "/";
                }

                if (DatabaseAccess.Read("SELECT * FROM `RoleFolders` WHERE `Id`=`" + this.Identifier.ToString() + "`") == null)
                {
                    // Create
                    SqlCommand command = new SqlCommand("INSERT INTO `RoleFolders` (`Id`, `Folder`, `Guild`) VALUES ('@id', '@path', '@guild')", DatabaseAccess.Database);
                    command.Parameters.Add("@id", SqlDbType.BigInt).Value = this.Identifier;
                    command.Parameters.Add("@folder", SqlDbType.Text).Value = this.Path;
                    command.Parameters.Add("@guild", SqlDbType.BigInt).Value = this.GuildIdentifier;
                    command.ExecuteNonQuery();
                }
                else
                {
                    // Update
                    SqlCommand command = new SqlCommand("UPDATE INTO `RoleFolders` SET `Id`=@id, `Folder`=@folder, `Guild`=@guild", DatabaseAccess.Database);
                    command.Parameters.Add("@id", SqlDbType.BigInt).Value = this.Identifier;
                    command.Parameters.Add("@folder", SqlDbType.Text).Value = this.Path;
                    command.Parameters.Add("@guild", SqlDbType.BigInt).Value = this.GuildIdentifier;
                    command.ExecuteNonQuery();
                }
            }

            if (guild != null)
            {
                if (this.Identifier == 0)
                {
                    // Create role
                    DNET.Color? color = null;

                    if (this.Color != null && this.Color.Count == 3)
                    {
                        color = new DNET.Color(this.Color[0], this.Color[1], this.Color[2]);
                    }

                    guild.CreateRoleAsync(this.Name, this.Permissions, color).RunSynchronously();
                    return true;
                }
                else
                {
                    // Modify role
                    DW.SocketRole role = guild.GetRole(this.Identifier);

                    if (role != null)
                    {
                        role.ModifyAsync(changed =>
                        {
                            changed.Name = this.Name;
                            changed.Permissions = this.Permissions;

                            if (this.Color != null && this.Color.Count == 3)
                            {
                                changed.Color = new DNET.Optional<DNET.Color>(new DNET.Color(this.Color[0], this.Color[1], this.Color[2]));
                            }
                        }).RunSynchronously();

                        return true;
                    }
                }
            }

            return false;
        }
    }
}