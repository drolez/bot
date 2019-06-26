namespace Drolez
{
    using Discord;
    using System;
    using System.Linq;
    using System.Net.WebSockets;
    using System.Threading;

    /// <summary>
    /// Extension methods
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// Get array field
        /// </summary>
        /// <param name="array">String array</param>
        /// <param name="index">Field index</param>
        /// <returns>Field value</returns>
        public static string GetPart(this string[] array, int index)
        {
            if (array.Length < index)
            {
                return string.Empty;
            }

            return array[index];
        }

        /// <summary>
        /// Recieve message from client
        /// </summary>
        /// <param name="socket">Websocket client</param>
        /// <returns>Recieved message</returns>
        public static string Recieve(this WebSocket socket)
        {
            try
            {
                CancellationTokenSource source = new CancellationTokenSource();
                CancellationToken token = source.Token;

                byte[] array = new byte[2048];
                ArraySegment<byte> buffer = new ArraySegment<byte>(array);

                socket.ReceiveAsync(buffer, token).GetAwaiter().GetResult();
                string message = new string(buffer.ToArray().Where(part => part > 0).Select(part => (char)part).ToArray());

                // Test channel, DEBUG out <---------------------------------------------------------------------------------------------------- Will nuke later
                ((ITextChannel)Program.DiscordClient.GetChannel(593508765144186890)).SendMessageAsync("IN:" + message.Length + "> " + message);

                return message;
            }
            catch (Exception ex)
            {
                ConsoleColor color = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("ERR>" + ex.ToString());
                Console.ForegroundColor = color;
            }

            return null;
        }

        /// <summary>
        /// Send text to specific client
        /// </summary>
        /// <param name="socket">Websocket client</param>
        /// <param name="message">Message to send</param>
        public static void Send(this WebSocket socket, string message)
        {
            CancellationTokenSource source = new CancellationTokenSource();
            CancellationToken token = source.Token;
            socket.SendAsync(message.ToSegment(), WebSocketMessageType.Text, true, token);

            // Test channel, DEBUG out <---------------------------------------------------------------------------------------------------- Will nuke later
            ((ITextChannel)Program.DiscordClient.GetChannel(593508765144186890)).SendMessageAsync("OUT:" + message.Length + "> " + message);
        }

        /// <summary>
        /// Convert string to byte array segment
        /// </summary>
        /// <param name="text">String to convert</param>
        /// <returns>Array segment</returns>
        public static ArraySegment<byte> ToSegment(this string text)
        {
            return new ArraySegment<byte>(text.Select(letter => (byte)letter).ToArray());
        }
    }
}