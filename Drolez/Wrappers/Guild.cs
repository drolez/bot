namespace Drolez.Wrappers
{
    using System;
    using DW = Discord.WebSocket;

    /// <summary>
    /// Discord guild wrapper
    /// </summary>
    [Serializable]
    public class Guild
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Guild" /> class.
        /// </summary>
        /// <param name="guild">Discord guild</param>
        /// <param name="admin">Is user administrator for this guild?</param>
        public Guild(DW.SocketGuild guild, bool admin)
        {
            this.Identifier = guild.Id.ToString();
            this.Icon = guild.IconUrl;
            this.IconIdentifier = guild.IconId;
            this.MemberCount = guild.MemberCount;
            this.Name = guild.Name;
            this.IsAdministrator = admin;
        }

        /// <summary>
        /// Gets or sets a value indicating whether user is an administrator in this guild
        /// </summary>
        public bool IsAdministrator { get; set; }

        /// <summary>
        /// Gets or sets guild icon
        /// </summary>
        public string Icon { get; set; }

        /// <summary>
        /// Gets or sets guild icon identifier
        /// </summary>
        public string IconIdentifier { get; set; }

        /// <summary>
        /// Gets or sets identifier
        /// </summary>
        public string Identifier { get; set; }

        /// <summary>
        /// Gets or sets member count
        /// </summary>
        public int MemberCount { get; set; }

        /// <summary>
        /// Gets or sets guild name
        /// </summary>
        public string Name { get; set; }
    }
}