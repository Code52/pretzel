using System;
using System.Globalization;
using System.Threading.Tasks;
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

                    Tracing.Info(path);

                    if (!Content.IsAvailable(path))
                    {
                        var path404 = "/404.html";
                        context.Response.StatusCode = 404;

                        if (Content.IsAvailable(path404))
                        {
                            context.Response.ContentType = path404.MimeType();
                            return context.Response.WriteAsync(Content.GetContent(path404));
                        }

                        context.Response.ContentType = path.MimeType();
                        return context.Response.WriteAsync("Page not found: " + path);
                    }

                    if (path.MimeType().IsBinaryMime())
                    {
                        context.Response.ContentType = path.MimeType();
                        var fileContents = Content.GetBinaryContent(path);
                        context.Response.Headers["Content-Range"] = string.Format("bytes 0-{0}", (fileContents.Length - 1));
                        context.Response.Headers["Content-Length"] = fileContents.Length.ToString(CultureInfo.InvariantCulture);
                        return context.Response.WriteAsync(fileContents);
                    }

                    if (Content.IsDirectory(path) && !path.EndsWith("/"))
                    {
                        // if path is a directory without trailing slash, redirects to the same url with a trailing slash
                        context.Response.StatusCode = 301;
                        context.Response.Headers["location"] = String.Format("http://localhost:{0}{1}/", context.Request.LocalPort, path);
                        return Task.Delay(0);
                    }

                    context.Response.ContentType = path.MimeType();
                    return context.Response.WriteAsync(Content.GetContent(path));
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
