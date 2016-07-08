using System;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using Gate;
using Pretzel.Logic.Extensions;
using Owin;
using Microsoft.Owin.Hosting;

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

			var options = new StartOptions
            {
                ServerFactory = "Nowin",
                Port = Port
            };

            Startup.Content = content;
            host = WebApp.Start<Startup>(options);

            IsRunning = true;

            return true;
        }

        public class Startup
        {
            public static IWebContent Content { get; set; }

            public void Configuration(IAppBuilder app)
            {
                app.Run(context =>
                {
                    var path = context.Request.Path.Value;
                    var env = context.Request.Environment;

                    Tracing.Info(path);

                    if (!Content.IsAvailable(path))
                    {
                        var path404 = "/404.html";
                        var use404 = Content.IsAvailable(path404);

                        var response = new Response(env) { ContentType = use404 ? path404.MimeType() : path.MimeType() };
                        using (var writer = new StreamWriter(response.OutputStream))
                        {
                            if (use404)
                            {
                                writer.Write(Content.GetContent(path404));
                            }
                            else
                            {
                                writer.Write("Page not found: " + path);
                            }
                        }
                        return TaskHelpers.Completed();
                    }

                    if (path.MimeType().IsBinaryMime())
                    {
                        var fileContents = Content.GetBinaryContent(path);
                        var response = new Response(env) { ContentType = path.MimeType() };
                        response.Headers["Content-Range"] = new[] { string.Format("bytes 0-{0}", (fileContents.Length - 1)) };
                        response.Headers["Content-Length"] = new[] { fileContents.Length.ToString(CultureInfo.InvariantCulture) };
                        response.Write(new ArraySegment<byte>(fileContents));
                    }
                    else if (Content.IsDirectory(path) && !path.EndsWith("/")) 
                    {
                        // if path is a directory without trailing slash, redirects to the same url with a trailing slash
                        var response = new Response(env) { Status = "301 Moved Permanently" };
                        response.Headers["Location"] = new[] { String.Format("http://localhost:{0}{1}/", context.Request.LocalPort, path) };
                    }
                    else
                    {
                        var response = new Response(env) { ContentType = path.MimeType() };
                        using (var writer = new StreamWriter(response.OutputStream))
                        {
                            writer.Write(Content.GetContent(path));
                        }
                    }
                    
                    return Task.Delay(0);
                });
            }
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
