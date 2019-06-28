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
                return new string(buffer.ToArray().Where(part => part > 0).Select(part => (char)part).ToArray());
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
        /// <param name="eventMutator">Type of event</param>
        /// <param name="data">Event data</param>
        public static void Send(this WebSocket socket, string eventMutator, object data)
        {
            CancellationTokenSource source = new CancellationTokenSource();
            CancellationToken token = source.Token;

            Wrappers.Event jsonEvent = new Wrappers.Event()
            {
                Data = data,
                DrolezEventMutationDescriptor = eventMutator
            };

            string message = jsonEvent.ToEventJSON();
            socket.SendAsync(message.ToSegment(), WebSocketMessageType.Text, true, token);
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