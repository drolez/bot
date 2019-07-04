namespace Drolez.Wrappers
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.SqlClient;
    using System.Linq;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;
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
            this.GuildIdentifier = role.Guild.Id.ToString();
            this.Identifier = role.Id.ToString();
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
        public string GuildIdentifier { get; set; }

        /// <summary>
        /// Gets or sets identifier
        /// </summary>
        public string Identifier { get; set; }

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
        /// Update database
        /// </summary>
        /// <param name="id">Role identifier</param>
        /// <param name="path">Folder path</param>
        /// <param name="guild">Guild identifier</param>
        public static void UpdateDatabase(ulong id, string path, ulong guild)
        {
            SqlCommand command = null;

            if (DatabaseAccess.Read("SELECT * FROM `RoleFolders` WHERE `Id`=`" + id.ToString() + "`") == null)
            {
                // Create
                command = new SqlCommand("INSERT INTO `RoleFolders` (`Id`, `Folder`, `Guild`) VALUES ('@id', '@path', '@guild')", DatabaseAccess.Database);
            }
            else
            {
                // Update
                command = new SqlCommand("UPDATE INTO `RoleFolders` SET `Id`=@id, `Folder`=@folder, `Guild`=@guild", DatabaseAccess.Database);
            }

            command.Parameters.Add("@id", SqlDbType.BigInt).Value = id;
            command.Parameters.Add("@folder", SqlDbType.Text).Value = path;
            command.Parameters.Add("@guild", SqlDbType.BigInt).Value = guild;
            command.ExecuteNonQuery();
        }

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
                    .Where(folder => !string.IsNullOrWhiteSpace(folder)));

                if (string.IsNullOrWhiteSpace(this.Path) || !this.Path.StartsWith('/'))
                {
                    this.Path = "/";
                }
            }

            Role.UpdateDatabase(ulong.Parse(this.Identifier), this.Path, ulong.Parse(this.GuildIdentifier));

            if (guild != null)
            {
                if (this.Identifier == "0" || string.IsNullOrWhiteSpace(this.Identifier))
                {
                    // Create role
                    DNET.Color? color = null;

                    if (this.Color != null && this.Color.Count == 3)
                    {
                        color = new DNET.Color(this.Color[0], this.Color[1], this.Color[2]);
                    }

                    Task.Run(() => guild.CreateRoleAsync(this.Name, this.Permissions, color));
                    return true;
                }
                else
                {
                    // Modify role
                    DW.SocketRole role = guild.GetRole(ulong.Parse(this.Identifier));

                    if (role != null)
                    {
                        Task.Run(() => role.ModifyAsync(changed =>
                        {
                            changed.Name = this.Name;
                            changed.Permissions = this.Permissions;

                            if (this.Color != null && this.Color.Count == 3)
                            {
                                changed.Color = new DNET.Optional<DNET.Color>(new DNET.Color(this.Color[0], this.Color[1], this.Color[2]));
                            }
                        }));

                        return true;
                    }
                }
            }

            return false;
        }
    }
}