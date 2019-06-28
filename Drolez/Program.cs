namespace Drolez
{
    using System;
    using System.IO;
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
        /// Gets SSL certificate
        /// </summary>
        internal static X509Certificate2 Certificate { get; private set; } = null;

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
        /// Entry point
        /// </summary>
        /// <param name="args">App arguments</param>
        public static void Main(string[] args)
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
            DatabaseAccess.DatabaseConnectionSettings databaseSettings = new DatabaseAccess.DatabaseConnectionSettings();
            string botToken = string.Empty;

            // Read config stuff
            try
            {
                XmlDocument document = new XmlDocument();
                document.Load("settings.xml");

                foreach (XmlNode config in document.SelectSingleNode("/settings"))
                {
                    switch (config.Name)
                    {
                        case "Token":
                            botToken = config.InnerText;
                            break;

                        case "CertificatePath":
                            Program.CertificatePath = config.InnerText;
                            break;

                        case "CertificatePassword":
                            Program.CertificatePassword = config.InnerText;
                            break;

                        case "DBServer":
                            databaseSettings.Server = config.InnerText;
                            break;

                        case "DBName":
                            databaseSettings.DatabaseName = config.InnerText;
                            break;

                        case "DBUser":
                            databaseSettings.User = config.InnerText;
                            break;

                        case "DBPassword":
                            databaseSettings.Password = config.InnerText;
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                // It broke
                ex.ToString();
                return;
            }

            // Load certificate
            try
            {
                if (string.IsNullOrWhiteSpace(Program.CertificatePath) || !File.Exists(Program.CertificatePath))
                {
                    throw new System.Security.VerificationException("Certificate file not found!");
                }

                Program.Certificate = new X509Certificate2(Program.CertificatePath, Program.CertificatePassword);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return;
            }

            // Setup environment
            CommandHandler.LoadCommands();
            Program.SetupWebSockets();

            // Start bot
            new Program().MainAsync(botToken).GetAwaiter().GetResult();

            // Stop server
            Program.Server.Dispose();
            DatabaseAccess.Close();
        }

        /// <summary>
        /// Async entry point
        /// </summary>
        /// <param name="token">Bot token</param>
        /// <returns>Task result</returns>
        public async Task MainAsync(string token)
        {
            Program.DiscordClient = new DW.DiscordSocketClient();
            Program.DiscordClient.Log += this.Log;

            await Program.DiscordClient.LoginAsync(DNET.TokenType.Bot, token);
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

            if (command == "ping")
            {
                socket.Send("pong");
            }

            if (command == null)
            {
                return;
            }

            if (command.StartsWith("auth/") && command.Length > 5 && !CommandHandler.IsRegistered(socket))
            {
                string tokenData = command.Substring(5);
                ulong userId = Auth.AuthenticateToken(tokenData, socket);

                if (userId == 0)
                {
                    return;
                }

                Program.RegisterClient(socket, userId);
            }
            else if (!CommandHandler.IsRegistered(socket) || command.StartsWith("register/") || string.IsNullOrWhiteSpace(command))
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
        /// Register client
        /// </summary>
        /// <param name="socket">Web socket</param>
        /// <param name="userId">User to register</param>
        private static void RegisterClient(WebSocket socket, ulong userId)
        {
            if (userId > 0)
            {
                DW.SocketUser user = Program.DiscordClient.GetUser(userId);
                int mutual = user.MutualGuilds.Count;

                if (user != null && mutual > 0)
                {
                    CommandHandler.ClientAdd(socket, user);
                    socket.Send("true");
                }
                else if (mutual == 0)
                {
                    socket.Send("none");
                }
                else
                {
                    socket.Send("false");
                }
            }
        }

        /// <summary>
        /// Setup web sockets server
        /// </summary>
        private static void SetupWebSockets()
        {
            // Setup web sockets server
            Program.Server = new WebSocketsServer(Program.Certificate);

            // Process incomming connections
            Program.Server.OnConnection += (sender, socket) =>
            {
                while (socket.State < WebSocketState.Closed)
                {
                    try
                    {
                        Program.DoSocketStuff(socket);
                    }
                    catch (Exception ex)
                    {
                        ex.ToString();
                    }
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