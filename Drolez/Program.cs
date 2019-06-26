namespace Drolez
{
    using Discord;
    using Discord.WebSocket;
    using System;
    using System.Net.WebSockets;
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
        /// Secret token. Shhh!
        /// </summary>
        internal static string Token = string.Empty;

        /// <summary>
        /// Discord client
        /// </summary>
        private DiscordSocketClient client;

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

            // Start bot
            new Program().MainAsync(args).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Async entry point
        /// </summary>
        /// <param name="args">App arguments</param>
        /// <returns>Task result</returns>
        public async Task MainAsync(string[] args)
        {
            this.client = new DiscordSocketClient();
            this.client.Log += this.Log;

            await this.client.LoginAsync(TokenType.Bot, Program.Token);
            await this.client.StartAsync();



            await Task.Delay(-1);
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