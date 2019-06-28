namespace Drolez
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Net;
    using System.Net.Security;
    using System.Net.Sockets;
    using System.Net.WebSockets;
    using System.Security.Cryptography.X509Certificates;
    using System.Threading;
    using System.Threading.Tasks;
    using NWS = Ninja.WebSockets;

    /// <summary>
    /// Web socket server class (because .NET does not have one)
    /// </summary>
    public class WebSocketsServer : IDisposable
    {
        /// <summary>
        /// Web socket server factory
        /// </summary>
        private readonly NWS.IWebSocketServerFactory socketServerFactory;

        /// <summary>
        /// Supported sub protocols
        /// </summary>
        private readonly HashSet<string> subProtocols;

        /// <summary>
        /// Server was stopped
        /// </summary>
        private bool isDisposed = false;

        /// <summary>
        /// TCP listener
        /// </summary>
        private TcpListener listener;

        /// <summary>
        /// Certificate file
        /// </summary>
        private X509Certificate2 serverCertificate;

        /// <summary>
        /// Initializes a new instance of the <see cref="WebSocketsServer"/> class
        /// </summary>
        /// <param name="serverCertificate">Certificate to use</param>
        public WebSocketsServer(X509Certificate2 serverCertificate = null)
        {
            this.serverCertificate = serverCertificate;
            this.socketServerFactory = new NWS.WebSocketServerFactory();
            this.subProtocols = new HashSet<string> { "chatV1", "chatV2", "chatV3" };
        }

        /// <summary>
        /// Connection event handler
        /// </summary>
        /// <param name="server">Web socket server</param>
        /// <param name="webSocket">Web socket</param>
        public delegate void OnConnectionHandler(WebSocketsServer server, WebSocket webSocket);

        /// <summary>
        /// On connection event
        /// </summary>
        public event OnConnectionHandler OnConnection;

        /// <summary>
        /// On Exception event
        /// </summary>
        public event Extensions.OnExceptionHandler OnException;

        /// <summary>
        /// Gets or sets keep alive interval time
        /// </summary>
        public TimeSpan KeepAliveInterval { get; set; } = TimeSpan.FromMinutes(4);

        /// <summary>
        /// Stop server
        /// </summary>
        public void Dispose()
        {
            if (!this.isDisposed)
            {
                this.isDisposed = true;

                try
                {
                    if (this.listener != null)
                    {
                        if (this.listener.Server != null)
                        {
                            this.listener.Server.Close();
                        }

                        this.listener.Stop();
                    }
                }
                catch (Exception ex)
                {
                    this.OnException?.Invoke(this, ex);
                }
            }
        }

        /// <summary>
        /// Accept connections
        /// </summary>
        /// <param name="port">Port number</param>
        /// <returns>Async task</returns>
        public async Task Listen(int port)
        {
            try
            {
                IPAddress address = IPAddress.Any;
                this.listener = new TcpListener(address, port);
                this.listener.Start();

                while (true)
                {
                    TcpClient client = await this.listener.AcceptTcpClientAsync();
                    this.ProcessTcpClient(client);
                }
            }
            catch (Exception ex)
            {
                this.OnException?.Invoke(this, ex);
            }
        }

        /// <summary>
        /// Certificate validation
        /// </summary>
        /// <param name="sender">The object</param>
        /// <param name="certificate">Used certificate</param>
        /// <param name="chain">SSL chain</param>
        /// <param name="sslPolicyErrors">SSL errors</param>
        /// <returns>True if ok</returns>
        private static bool AppCertificateValidation(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }

        /// <summary>
        /// Accept network stream
        /// </summary>
        /// <param name="stream">Network stream</param>
        /// <returns>Finished task</returns>
        private async Task AcceptStream(Stream stream)
        {
            NWS.WebSocketHttpContext context = await this.socketServerFactory.ReadHttpHeaderFromStreamAsync(stream);

            if (context.IsWebSocketRequest)
            {
                string protocol = this.GetSubProtocol(context.WebSocketRequestedProtocols);

                NWS.WebSocketServerOptions options = new NWS.WebSocketServerOptions()
                {
                    KeepAliveInterval = this.KeepAliveInterval,
                    SubProtocol = protocol
                };

                WebSocket webSocket = await this.socketServerFactory.AcceptWebSocketAsync(context, options);

                this.OnConnection?.Invoke(this, webSocket);
            }
        }

        /// <summary>
        /// Get requested sub protocol
        /// </summary>
        /// <param name="requested">List of requested protocols</param>
        /// <returns>Sub protocol</returns>
        private string GetSubProtocol(IEnumerable<string> requested)
        {
            foreach (string subProtocol in requested)
            {
                // Match the first sub protocol that we support (the client should pass the most preferable sub protocols first)
                if (this.subProtocols.Contains(subProtocol))
                {
                    return subProtocol;
                }
            }

            return null;
        }

        /// <summary>
        /// Process client connection
        /// </summary>
        /// <param name="client">TCP client</param>
        private void ProcessTcpClient(TcpClient client)
        {
            Task.Run(() => this.ProcessTcpClientAsync(client));
        }

        /// <summary>
        /// Process client connection async
        /// </summary>
        /// <param name="client">TCP client</param>
        /// <returns>Async task</returns>
        private async Task ProcessTcpClientAsync(TcpClient client)
        {
            CancellationTokenSource source = new CancellationTokenSource();

            if (this.isDisposed)
            {
                return;
            }

            try
            {
                if (this.serverCertificate != null)
                {
                    SslStream sslStream = new SslStream(client.GetStream(), false, WebSocketsServer.AppCertificateValidation);
                    sslStream.AuthenticateAsServer(this.serverCertificate, false, false);
                    await this.AcceptStream(sslStream);
                }
                else
                {
                    await this.AcceptStream(client.GetStream());
                }
            }
            catch (ObjectDisposedException)
            {
                // Do nothing
            }
            catch (Exception ex)
            {
                this.OnException?.Invoke(this, ex);
            }
            finally
            {
                this.TryStopClient(client, source);
            }
        }

        /// <summary>
        /// Try to stop TCP client
        /// </summary>
        /// <param name="client">TCP client</param>
        /// <param name="source">Cancelation token source</param>
        private void TryStopClient(TcpClient client, CancellationTokenSource source)
        {
            try
            {
                client.Client.Close();
                client.Close();
                source.Cancel();
            }
            catch (Exception ex)
            {
                this.OnException?.Invoke(this, ex);
            }
        }
    }
}