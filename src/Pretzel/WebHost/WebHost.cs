using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Firefly.Http;
using Owin;
using System.Threading;
using System.IO;
using Pretzel.Logic.Extensions;

namespace Pretzel
{
    public class WebHost : IDisposable
    {
        IDisposable host;
        IWebContent content;

        public int Port { get; private set; }
        public bool IsRunning { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="basePath">Base path for server</param>
        public WebHost(string basePath, IWebContent content)
            : this(basePath, content, 8080)
        {
        }

        /// <summary>
        /// Constructor for port number
        /// </summary>
        /// <param name="basePath">Base path for server</param>
        /// <param name="port">Port number</param>
        public WebHost(string basePath, IWebContent content, int port)
        {
            IsRunning = false;

            this.content = content;
            content.SetBasePath(basePath);

            this.Port = port;
        }

        /// <summary>
        /// Launch server
        /// </summary>
        public bool Start()
        {
            if (IsRunning)
            {
                return false;
            }

            // Launch web server
            ServerFactory server = new ServerFactory();
            host = server.Create(ServerCallback, Port);

            IsRunning = true;

            return true;
        }

        /// <summary>
        /// Stop server
        /// </summary>
        public bool Stop()
        {
            if (!IsRunning)
            {
                return false;
            }

            // Stop host
            host.Dispose();
            host = null;

            return true;
        }

        /// <summary>
        /// Queries sent from the client end up here
        /// </summary>
        /// <param name="env"></param>
        /// <param name="result"></param>
        /// <param name="fault"></param>
        private void ServerCallback(IDictionary<string, object> env, ResultDelegate result, Action<Exception> fault)
        {
            string request = (string)env[OwinConstants.RequestPath];

            if (!content.IsAvailable(request))
            {
                // File not found
                SendText(result, "404 Not Found", "text/plain", "Page not found: " + request);
                return;
            }
            else
            {
                // Send page back
                string fileContents = content.GetContent(request);
                SendText(result, "200 OK", request.MimeType(), fileContents);
                return;
            }
        }

        /// <summary>
        /// Send data back to the user
        /// </summary>
        /// <param name="result">Result delegate from server-callback</param>
        /// <param name="bytes">byte-array to send</param>
        private void SendText(ResultDelegate result, string header, string contenttype, string text)
        {
            result(
                header,
                new Dictionary<string, IEnumerable<string>>(StringComparer.OrdinalIgnoreCase)
                    {
                        {"Content-Type", new[] {contenttype }}
                    },
                (Func<ArraySegment<byte>, bool> write, Func<Action, bool> flush, Action<Exception> end, CancellationToken cancellationToken) =>
                {
                    write(new ArraySegment<byte>(Encoding.Default.GetBytes(text)));
                    end(null);
                });
        }

        #region IDisposable
        private bool isDisposed = false;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public void Dispose(bool disposing)
        {
            if (!isDisposed)
            {
                if (disposing)
                {
                }

                if (host != null)
                {
                    host.Dispose();
                }

                IsRunning = false;
                isDisposed = true;
            }
        }

        ~WebHost()
        {
            Dispose(false);
        }
        #endregion
    }

    
}
