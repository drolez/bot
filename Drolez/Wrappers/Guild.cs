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
        public Guild(DW.SocketGuild guild)
        {
            this.Identifier = guild.Id;
            this.Icon = guild.IconUrl;
            this.IconIdentifier = guild.IconId;
            this.MemberCount = guild.MemberCount;
            this.Name = guild.Name;
        }

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
        public ulong Identifier { get; set; }

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