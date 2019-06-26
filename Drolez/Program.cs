namespace Drolez
{
    using Discord;
    using Discord.WebSocket;
    using System;
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
        /// Discord client
        /// </summary>
        internal static DiscordSocketClient DiscordClient;

        /// <summary>
        /// Web sockets server
        /// </summary>
        internal static WebSockets.WebSocketServer Server = null;

        /// <summary>
        /// Secret token. Shhh!
        /// </summary>
        internal static string Token = string.Empty;

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
                    if (config.Name == "Token")
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
                    Console.WriteLine("OH someones there!");

                    while (socket.State < WebSocketState.Closed)
                    {
                        Program.DoSocketStuff(socket);
                    }

                    CommandHandler.ClientRemove(socket);
                }).Start();
            };

            // Start websockets
            Program.Server.Bind(new IPEndPoint(new IPAddress(new byte[] { 192, 168, 1, 3 }), 4555));
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
            Program.DiscordClient = new DiscordSocketClient();
            Program.DiscordClient.Log += this.Log;

            await Program.DiscordClient.LoginAsync(TokenType.Bot, Program.Token);
            await Program.DiscordClient.StartAsync();

            await Task.Delay(-1);
        }

        /// <summary>
        /// This does socket stuff, when socket stuff happens
        /// </summary>
        /// <param name="socket">Web socket</param>
        private static void DoSocketStuff(WebSocket socket)
        {
            string command = socket.Recieve();

            if (command.StartsWith("register/") && command.Length > 9)
            {
                string userId = command.Substring(9);
                ulong id = 0;
                ulong.TryParse(userId, out id);

                if (id > 0)
                {
                    SocketUser user = Program.DiscordClient.GetUser(id);

                    if (user != null)
                    {
                        CommandHandler.ClientAdd(socket, user);
                        socket.Send("true");
                    }
                    else
                    {
                        socket.Send("false");
                    }
                }
            }
            else if (!CommandHandler.IsRegistered(socket) || string.IsNullOrWhiteSpace(command))
            {
                socket.Send(command == null ? "ERR:Recieve error, Check logs!" : "ERR:Invalid!");
            }
            else
            {
                CommandHandler.ProcessCommand(socket, command);
            }
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