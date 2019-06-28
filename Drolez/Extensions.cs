namespace Drolez
{
    using System;
    using System.Data;
    using System.Linq;
    using System.Net.WebSockets;
    using System.Threading;
    using DNET = Discord;
    using JSON = Newtonsoft.Json;

    /// <summary>
    /// Extension methods
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// 1MB web socket buffer
        /// </summary>
        private const int WebSocketBufferSize = 1024 * 1024;

        /// <summary>
        /// Connection event handler
        /// </summary>
        /// <param name="sender">Object that raised exception</param>
        /// <param name="exception">Raised exception</param>
        public delegate void OnExceptionHandler(object sender, Exception exception);

        #region Debug

        /// <summary>
        /// Max length of debug output
        /// </summary>
        private const int MaxDebugLength = 50;

        private static TimeSpan debugCooldown = TimeSpan.FromSeconds(10);

        private static DateTime debugLastSendIN = DateTime.MinValue;
        private static DateTime debugLastSendOUT = DateTime.MinValue;

        /// <summary>
        /// Test channel, DEBUG out <!---------------------------------------------------------------------------------------------------- Will nuke later-->
        /// </summary>
        /// <param name="input">The input</param>
        /// <param name="message">The message</param>
        private static void XDEBUGOUT(bool input, string message)
        {
            if (DateTime.Now > (input ? debugLastSendIN + debugCooldown : debugLastSendOUT + debugCooldown))
            {
                if (input)
                {
                    debugLastSendIN = DateTime.Now;
                }
                else
                {
                    debugLastSendOUT = DateTime.Now;
                }

                string messageResult = message;

                if (messageResult.Length > Extensions.MaxDebugLength)
                {
                    messageResult = message.Substring(0, Math.Min(message.Length, Extensions.MaxDebugLength)) + "...";
                }

                ((DNET.ITextChannel)Program.DiscordClient.GetChannel(593508765144186890))
                    .SendMessageAsync((input ? "IN:" : "OUT:") + message.Length + "> " + messageResult);
            }
        }

        #endregion Debug

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
        /// Receive message from client
        /// </summary>
        /// <param name="socket">Web socket client</param>
        /// <returns>Received message</returns>
        public static string Receive(this WebSocket socket)
        {
            try
            {
                CancellationTokenSource source = new CancellationTokenSource();
                CancellationToken token = source.Token;

                byte[] array = new byte[Extensions.WebSocketBufferSize];
                ArraySegment<byte> buffer = new ArraySegment<byte>(array);

                WebSocketReceiveResult result = socket.ReceiveAsync(buffer, token).GetAwaiter().GetResult();
                string message = new string(buffer.ToArray().Where(part => part > 0).Select(part => (char)part).ToArray());

                // Test channel, DEBUG out <---------------------------------------------------------------------------------------------------- Will nuke later
                Extensions.XDEBUGOUT(true, message);

                return message;
            }
            catch (Exception)
            {
                // Do nothing
                return null;
            }
        }

        /// <summary>
        /// Convert DB record to string
        /// </summary>
        /// <param name="record">DB data</param>
        /// <param name="index">Record index</param>
        /// <returns>Record string value</returns>
        public static string RecordToString(this IDataRecord record, int index)
        {
            try
            {
                return record[index].ToString();
            }
            catch (Exception ex)
            {
                ex.ToString();
                return string.Empty;
            }
        }

        /// <summary>
        /// Send text to specific client
        /// </summary>
        /// <param name="socket">Web socket client</param>
        /// <param name="message">Message to send</param>
        public static void Send(this WebSocket socket, string message)
        {
            CancellationTokenSource source = new CancellationTokenSource();
            CancellationToken token = source.Token;
            socket.SendAsync(message.ToSegment(), WebSocketMessageType.Text, true, token);

            // Test channel, DEBUG out <---------------------------------------------------------------------------------------------------- Will nuke later
            Extensions.XDEBUGOUT(false, message);
        }

        /// <summary>
        /// Convert object to JSON string
        /// </summary>
        /// <param name="obj">Object to convert</param>
        /// <returns>JSON string</returns>
        public static string ToJSON(this object obj)
        {
            return JSON.JsonConvert.SerializeObject(obj);
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