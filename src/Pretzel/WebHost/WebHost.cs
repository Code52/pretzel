using System;
using System.Collections.Generic;
using System.Globalization;
using Firefly.Http;
using Owin;
using Pretzel.Logic.Extensions;
using Gate;

namespace Pretzel
{
    public class WebHost : IDisposable
    {
        IDisposable host;
        readonly IWebContent content;

        public int Port { get; private set; }
        public bool IsRunning { get; private set; }

        public WebHost(string basePath, IWebContent content)
            : this(basePath, content, 8080)
        {
        }

        public WebHost(string basePath, IWebContent content, int port)
        {
            IsRunning = false;

            this.content = content;
            content.SetBasePath(basePath);

            Port = port;
        }

        public bool Start()
        {
            if (IsRunning)
            {
                return false;
            }

            var server = new ServerFactory();
            host = server.Create(ServerCallback, Port);

            IsRunning = true;

            return true;
        }

        public bool Stop()
        {
            if (!IsRunning)
            {
                return false;
            }

            host.Dispose();
            host = null;

            return true;
        }

        private void ServerCallback(IDictionary<string, object> env, ResultDelegate result, Action<Exception> fault)
        {
            var requestString = (string)env[OwinConstants.RequestPath];

            Tracing.Info(requestString);

            if (!content.IsAvailable(requestString))
            {
                SendText(result, env, "Page not found: " + requestString);
                return;
            }

            if (requestString.MimeType().IsBinaryMime())
            {
                var fileContents = content.GetBinaryContent(requestString);
                SendData(result, env, fileContents);
            }
            else
            {
                var fileContents = content.GetContent(requestString);
                SendText(result, env, fileContents);
            }
        }

        private void SendText(ResultDelegate result, IDictionary<string, object> env, string text)
        {
            var requestString = (string)env[OwinConstants.RequestPath];
            var response = new Response(result) { ContentType = requestString.MimeType() };
            response.Write(text);
            response.End();
        }

        private void SendData(ResultDelegate result, IDictionary<string, object> env, byte[] data)
        {
            var requestString = (string)env[OwinConstants.RequestPath];

            var response = new Response(result) { ContentType = requestString.MimeType() };
            response.Headers["Content-Range"] = new[] { string.Format("bytes 0-{0}", (data.Length - 1)) };
            response.Headers["Content-Length"] = new[] { data.Length.ToString(CultureInfo.InvariantCulture) };
            response.Write(new ArraySegment<byte>(data));
            response.End();
        }

        #region IDisposable
        private bool isDisposed;

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
