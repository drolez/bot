namespace Drolez
{
    using System;
    using System.Net;
    using System.Net.WebSockets;
    using System.Security.Cryptography.X509Certificates;
    using System.Threading.Tasks;
    using System.Xml;
    using DNET = Discord;
    using DW = Discord.WebSocket;

    /// <summary>
    /// Program entry class
    /// </summary>
    public class Program
    {
        /// <summary>
        /// Gets certificate password
        /// </summary>
        internal static string CertificatePassword { get; private set; } = string.Empty;

        /// <summary>
        /// Gets certificate file path
        /// </summary>
        internal static string CertificatePath { get; private set; } = string.Empty;

        /// <summary>
        /// Gets Discord client
        /// </summary>
        internal static DW.DiscordSocketClient DiscordClient { get; private set; }

        /// <summary>
        /// Gets web sockets server
        /// </summary>
        internal static WebSocketsServer Server { get; private set; } = null;

        /// <summary>
        /// Gets secret token. Shhh!
        /// </summary>
        internal static string Token { get; private set; } = string.Empty;

        /// <summary>
        /// Entry point
        /// </summary>
        /// <param name="args">App arguments</param>
        public static void Main(string[] args)
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;

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

                    // Read config nodes
                    if (config.Name == "CertificatePath")
                    {
                        Program.CertificatePath = config.InnerText;
                    }

                    // Read config nodes
                    if (config.Name == "CertificatePassword")
                    {
                        Program.CertificatePassword = config.InnerText;
                    }
                }
            }
            catch (Exception ex)
            {
                // It broke
                ex.ToString();
                return;
            }

            // Setup environment
            CommandHandler.LoadCommands();
            Program.SetupWebSockets();

            // Start bot
            new Program().MainAsync(args).GetAwaiter().GetResult();

            // Stop server
            Program.Server.Dispose();
        }

        /// <summary>
        /// Async entry point
        /// </summary>
        /// <param name="args">App arguments</param>
        /// <returns>Task result</returns>
        public async Task MainAsync(string[] args)
        {
            Program.DiscordClient = new DW.DiscordSocketClient();
            Program.DiscordClient.Log += this.Log;

            await Program.DiscordClient.LoginAsync(DNET.TokenType.Bot, Program.Token);
            await Program.DiscordClient.StartAsync();

            await Task.Delay(-1);
        }

        /// <summary>
        /// This does socket stuff, when socket stuff happens
        /// </summary>
        /// <param name="socket">Web socket</param>
        private static void DoSocketStuff(WebSocket socket)
        {
            string command = socket.Receive();

            if (command == null)
            {
                return;
            }

            if (command.StartsWith("register/") && command.Length > 9)
            {
                string userId = command.Substring(9);
                ulong id = 0;
                ulong.TryParse(userId, out id);

                if (id > 0)
                {
                    DW.SocketUser user = Program.DiscordClient.GetUser(id);

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
                try
                {
                    CommandHandler.ProcessCommand(socket, command);
                }
                catch (Exception ex)
                {
                    socket.Send("ERR:" + ex.Message.ToString());
                }
            }
        }

        /// <summary>
        /// Setup web sockets server
        /// </summary>
        private static void SetupWebSockets()
        {
            if (string.IsNullOrWhiteSpace(Program.CertificatePath))
            {
                // Setup unsecure web sockets
                Program.Server = new WebSocketsServer();
            }
            else
            {
                // Setup web sockets server
                Program.Server = new WebSocketsServer(new X509Certificate2(Program.CertificatePath, Program.CertificatePassword));
            }

            // Process incomming connections
            Program.Server.OnConnection += (sender, socket) =>
            {
                while (socket.State < WebSocketState.Closed)
                {
                    Program.DoSocketStuff(socket);
                }

                CommandHandler.ClientRemove(socket);
            };

            // Log exceptions
            Program.Server.OnException += (sender, exception) =>
            {
                Console.WriteLine(DateTime.Now.ToString("T") + " ERR> " + exception.Message +
                    (exception.InnerException != null ? " > " + exception.InnerException.Message : string.Empty));
            };

            // Start websockets
            Task.Run(() => Program.Server.Listen(4555));
        }

        /// <summary>
        /// Discord log
        /// </summary>
        /// <param name="message">The message</param>
        /// <returns>Task result</returns>
        private Task Log(DNET.LogMessage message)
        {
            Console.WriteLine(message.ToString());
            return Task.CompletedTask;
        }
    }
}