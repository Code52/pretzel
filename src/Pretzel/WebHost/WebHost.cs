using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using Firefly.Http;
using Gate;
using Pretzel.Logic.Extensions;

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

        Task NewServerCallback(IDictionary<string, object> env)
        {
            var path = (string)env[OwinConstants.RequestPath];

            Tracing.Info(path);

            if (!content.IsAvailable(path))
            {
                var response = new Response { ContentType = path.MimeType() };
                using (var writer = new StreamWriter(response.OutputStream))
                {
                    writer.Write("Page not found: " + path);
                }
                return TaskHelpers.Completed();
            }

            if (path.MimeType().IsBinaryMime())
            {
                var fileContents = content.GetBinaryContent(path);
                var response = new Response { ContentType = path.MimeType() };
                response.Headers["Content-Range"] = new[] { string.Format("bytes 0-{0}", (fileContents.Length - 1)) };
                response.Headers["Content-Length"] = new[] { fileContents.Length.ToString(CultureInfo.InvariantCulture) };
                response.Write(new ArraySegment<byte>(fileContents));
            }
            else
            {
                var response = new Response { ContentType = path.MimeType() };
                using (var writer = new StreamWriter(response.OutputStream))
                {
                    writer.Write(content.GetContent(path));
                }
            }

            return TaskHelpers.Completed();
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
