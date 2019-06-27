namespace Drolez
{
    using System;

    /// <summary>
    /// Additional command class info
    /// </summary>
    public class CommandInfo : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CommandInfo"/> class
        /// </summary>
        /// <param name="command">Command name</param>
        public CommandInfo(string command)
        {
            this.Command = command;
        }

        /// <summary>
        /// Gets or sets command name
        /// </summary>
        public string Command { get; set; } = string.Empty;
    }
}