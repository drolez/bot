namespace Drolez
{
    using Discord;
    using Discord.WebSocket;
    using System;
    using System.Linq;
    using System.Net;
    using System.Net.WebSockets;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Xml;

    /// <summary>
    /// Program entry class
    /// </summary>
    public class Program
    {
        /// <summary>
        /// Name of the bot in discord
        /// </summary>
        internal static string BotName = string.Empty;

        /// <summary>
        /// Web sockets server
        /// </summary>
        internal static WebSockets.WebSocketServer Server = null;

        /// <summary>
        /// Secret token. Shhh!
        /// </summary>
        internal static string Token = string.Empty;

        /// <summary>
        /// Discord client
        /// </summary>
        private static DiscordSocketClient client;

        /// <summary>
        /// Entry point
        /// </summary>
        /// <param name="args">App arguments</param>
        public static void Main(string[] args)
        {
            // Read config stuff
            try
            {
                XmlDocument document = new XmlDocument();
                document.Load("settings.xml");

                foreach (XmlNode config in document.SelectSingleNode("/settings"))
                {
                    // Read config nodes
                    if (config.Name == "BotName")
                    {
                        Program.BotName = config.InnerText;
                    }
                    else if (config.Name == "Token")
                    {
                        Program.Token = config.InnerText;
                    }
                }
            }
            catch (Exception ex)
            {
                // It broke
                ex.ToString();
                return;
            }

            // Setup web sockets server
            Program.Server = new WebSockets.WebSocketServer();
            Program.Server.Connected += (sender, socket) =>
            {
                new Thread(() =>
                {
                    while (socket.State < WebSocketState.Closed)
                    {
                        Program.DoSocketStuff(socket);
                    }
                }).Start();
            };

            // Start websockets
            Program.Server.Bind(new IPEndPoint(new IPAddress(new byte[] { 0, 0, 0, 0 }), 4555));
            Program.Server.StartAccept();

            // Start bot
            new Program().MainAsync(args).GetAwaiter().GetResult();
            Program.Server.Dispose();
        }

        /// <summary>
        /// Async entry point
        /// </summary>
        /// <param name="args">App arguments</param>
        /// <returns>Task result</returns>
        public async Task MainAsync(string[] args)
        {
            Program.client = new DiscordSocketClient();
            Program.client.Log += this.Log;

            await Program.client.LoginAsync(TokenType.Bot, Program.Token);
            await Program.client.StartAsync();

            await Task.Delay(-1);
        }

        /// <summary>
        /// This does socket stuff, when socket stuff happens
        /// </summary>
        /// <param name="socket">Web socket</param>
        private static void DoSocketStuff(WebSocket socket)
        {
            ArraySegment<byte> buffer = new ArraySegment<byte>();
            socket.ReceiveAsync(buffer, new CancellationToken()).GetAwaiter().GetResult();

            string text = new string(buffer.Select(part => (char)part).ToArray());
            Program.client.Guilds.First().TextChannels.First().SendMessageAsync(text);
        }

        /// <summary>
        /// Discord log
        /// </summary>
        /// <param name="message">The message</param>
        /// <returns>Task result</returns>
        private Task Log(LogMessage message)
        {
            Console.WriteLine(message.ToString());
            return Task.CompletedTask;
        }
    }
}