namespace Drolez
{
    using System;
    using System.IO;
    using System.Net;
    using System.Net.WebSockets;
    using System.Threading;
    using System.Threading.Tasks;
    using JSON = Newtonsoft.Json;

    /// <summary>
    /// Authentication class
    /// </summary>
    public static class Auth
    {
        /// <summary>
        /// Authentication url
        /// </summary>
        private const string Url = "https://discordapp.com/api/users/@me";

        /// <summary>
        /// Authenticate token data
        /// </summary>
        /// <param name="tokenData">Token data</param>
        /// <param name="socket">Web socket</param>
        /// <returns>User Id</returns>
        public static ulong AuthenticateToken(string tokenData, WebSocket socket)
        {
            CancellationTokenSource source = new CancellationTokenSource();

            try
            {
                string[] tokenArray = tokenData.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
                int timeToLive = 0;

                if (tokenArray.Length < 2 || !int.TryParse(tokenArray[1], out timeToLive) || timeToLive <= 0)
                {
                    socket.CloseAsync(WebSocketCloseStatus.PolicyViolation, "Invalid token data!", source.Token).GetAwaiter().GetResult();
                    return 0;
                }

                // Check user credentials
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(Auth.Url);

                if (Program.Certificate != null)
                {
                    request.ClientCertificates.Add(Program.Certificate);
                }

                request.Method = "Get";
                request.ContentLength = 0;
                request.Headers.Add("Authorization", "Bearer " + tokenArray[0]);
                request.ContentType = "application/x-www-form-urlencoded";

                string result = string.Empty;

                using (HttpWebResponse response = request.GetResponse() as HttpWebResponse)
                {
                    result = new StreamReader(response.GetResponseStream()).ReadToEnd();
                }

                Console.WriteLine(result);

                // Get user info
                DiscordUser user = JSON.JsonConvert.DeserializeObject<DiscordUser>(result);

                // Set token time to live
                Task.Delay(new TimeSpan(0, 0, timeToLive)).ContinueWith(o =>
                {
                    CommandHandler.ClientRemove(socket);
                    socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Token expired!", source.Token).GetAwaiter().GetResult();
                });

                return ulong.Parse(user.id);
            }
            catch (Exception ex)
            {
                socket.CloseAsync(WebSocketCloseStatus.ProtocolError, ex.Message, source.Token).GetAwaiter().GetResult();
                return 0;
            }
        }

        /// <summary>
        /// Discord user from web API
        /// Do not change property names, must be same as in JSON!
        /// </summary>
        public class DiscordUser
        {
            /// <summary>
            /// Gets or sets avatar
            /// </summary>
            public string avatar { get; set; }

            /// <summary>
            /// Gets or sets discriminator
            /// </summary>
            public string discriminator { get; set; }

            /// <summary>
            /// Gets or sets user ID
            /// </summary>
            public string id { get; set; }

            /// <summary>
            /// Gets or sets a value indicating whether the user has two factor enabled on their account
            /// </summary>
            public bool mfa_enabled { get; set; }

            /// <summary>
            /// Gets or sets discord user name
            /// </summary>
            public string username { get; set; }
        }
    }
}