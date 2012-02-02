using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Firefly.Http;
using Owin;
using System.Threading;
using System.IO;
using Pretzel.Logic.Extensions;
using Gate;

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
            string requestString = (string)env[OwinConstants.RequestPath];

            var request = new Gate.Request(env);
            var response = new Gate.Response(result);

            if (!content.IsAvailable(requestString))
            {
                // File not found
                SendText(result, env, "Page not found: " + requestString);
                return;
            }
            else
            {
                // Send page back
                if (requestString.MimeType().IsBinaryMime())
                {
                    byte[] fileContents = content.GetBinaryContent(requestString);
                    SendData(result, env, fileContents);
                }
                else
                {
                    string fileContents = content.GetContent(requestString);
                    SendText(result, env, fileContents);
                }

                return;
            }
        }

        /// <summary>
        /// Send data back to the user
        /// </summary>
        /// <param name="result">Result delegate from server-callback</param>
        /// <param name="bytes">byte-array to send</param>
        private void SendText(ResultDelegate result, IDictionary<string, object> env, string text, int httpCode = 200)
        {
            Response response = new Response(result);
            Request request = new Request(env);

            string requestString = (string)env[OwinConstants.RequestPath];

            response.ContentType = requestString.MimeType();

            response.Write(text);

            response.End();
        }

        /// <summary>
        /// Send data back to the user
        /// </summary>
        /// <param name="result">Result delegate from server-callback</param>
        /// <param name="bytes">byte-array to send</param>
        private void SendData(ResultDelegate result, IDictionary<string, object> env, byte[] data)
        {
            Response response = new Response(result);
            Request request = new Request(env);

            string requestString = (string)env[OwinConstants.RequestPath];

            response.ContentType = requestString.MimeType();
            response.Headers["Content-Range"] = new string[] { "bytes 0-" + (data.Length - 1).ToString() };
            response.Headers["Content-Length"] = new string[] { data.Length.ToString() };

            response.Write(new ArraySegment<byte>(data));

            response.End();
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
