namespace Drolez.Wrappers
{
    using DW = Discord.WebSocket;

    /// <summary>
    /// User wrapper
    /// </summary>
    public class User
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="User"/> class
        /// </summary>
        /// <param name="user">Discord user</param>
        public User(DW.SocketUser user)
        {
            this.Avatar = user.GetAvatarUrl();
            this.Discriminator = user.Discriminator;
            this.Id = user.Id.ToString();
            this.Username = user.Username;
        }

        /// <summary>
        /// Gets or sets avatar
        /// </summary>
        public string Avatar { get; set; }

        /// <summary>
        /// Gets or sets discriminator
        /// </summary>
        public string Discriminator { get; set; }

        /// <summary>
        /// Gets or sets user ID
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets discord user name
        /// </summary>
        public string Username { get; set; }
    }
}