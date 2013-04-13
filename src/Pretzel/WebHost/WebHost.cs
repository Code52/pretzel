using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using Firefly.Http;
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
            host = server.Create(NewServerCallback, Port);

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

        private Task NewServerCallback(IDictionary<string, object> env)
        {
            var requestString = (string)env[OwinConstants.RequestPath];

            Tracing.Info(requestString);

            if (!content.IsAvailable(requestString))
            {
                SendText(env, "Page not found: " + requestString);
                return TaskHelpers.Completed();
            }

            if (requestString.MimeType().IsBinaryMime())
            {
                var fileContents = content.GetBinaryContent(requestString);
                SendData(env, fileContents);
            }
            else
            {
                var fileContents = content.GetContent(requestString);
                SendText(env, fileContents);
            }

            return TaskHelpers.Completed();
        }


        private void SendText(IDictionary<string, object> env, string text)
        {
            var requestString = (string)env[OwinConstants.RequestPath];
            var response = new Response { ContentType = requestString.MimeType() };
            response.Write(text);
        }

        private void SendData(IDictionary<string, object> env, byte[] data)
        {
            var requestString = (string)env[OwinConstants.RequestPath];

            var response = new Response { ContentType = requestString.MimeType() };
            response.Headers["Content-Range"] = new[] { string.Format("bytes 0-{0}", (data.Length - 1)) };
            response.Headers["Content-Length"] = new[] { data.Length.ToString(CultureInfo.InvariantCulture) };
            response.Write(new ArraySegment<byte>(data));
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
