namespace Drolez
{
    using System;
    using System.Net.WebSockets;
    using DW = Discord.WebSocket;

    /// <summary>
    /// Command interface
    /// </summary>
    public interface ICommand
    {
        /// <summary>
        /// Run command
        /// </summary>
        /// <param name="socket">Web socket</param>
        /// <param name="discordId">Discord user who invoked command</param>
        /// <param name="parameters">Command parameters</param>
        bool Run(WebSocket socket, DW.SocketUser user, string[] parameters);
    }
}
