using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Gate.Middleware;
using Gate.Middleware.StaticFiles;
using Owin;
using System.Threading.Tasks;

namespace Owin
{
    using AppFunc = Func<IDictionary<string, object>, Task>;

    // Serves static files from the given root directory for any matching URLs.
    internal static class StaticExtensions
    {
        public static IAppBuilder UseStatic(this IAppBuilder builder, string root, IEnumerable<string> urls)
        {
            return builder.UseFunc<AppFunc>(app => Static.Middleware(app, root, urls));
        }

        public static IAppBuilder UseStatic(this IAppBuilder builder, IEnumerable<string> urls)
        {
            return builder.UseFunc<AppFunc>(app => Static.Middleware(app, urls));
        }

        public static IAppBuilder UseStatic(this IAppBuilder builder, string root)
        {
            return builder.UseFunc<AppFunc>(app => Static.Middleware(app, root));
        }

        public static IAppBuilder UseStatic(this IAppBuilder builder)
        {
            return builder.UseFunc<AppFunc>(Static.Middleware);
        }
    }
    
}

namespace Gate.Middleware
{
    using AppFunc = Func<IDictionary<string, object>, Task>;


    internal class Static
    {
        private readonly AppFunc app;
        private readonly FileServer fileServer;
        private readonly IEnumerable<string> urls;

        public Static(AppFunc app, IEnumerable<string> urls)
            : this(app, null, urls)
        { }

        public Static(AppFunc app, string root = null, IEnumerable<string> urls = null)
        {
            this.app = app;

            if (root == null)
            {
                root = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "public");
            }

            if (!Directory.Exists(root))
            {
                throw new DirectoryNotFoundException(string.Format("Invalid root directory: {0}", root));
            }

            if (urls == null)
            {
                var rootDirectory = new DirectoryInfo(root);
                var files = rootDirectory.GetFiles("*").Select(fi => "/" + fi.Name);
                var directories = rootDirectory.GetDirectories().Select(di => "/" + di.Name);
                urls = files.Concat(directories);
            }

            this.urls = urls;

            fileServer = new FileServer(root);
        }

        public static AppFunc Middleware(AppFunc app, string root, IEnumerable<string> urls)
        {
            return new Static(app, root, urls).Invoke;
        }

        public static AppFunc Middleware(AppFunc app, string root)
        {
            return new Static(app, root).Invoke;
        }

        public static AppFunc Middleware(AppFunc app, IEnumerable<string> urls)
        {
            return new Static(app, urls).Invoke;
        }

        public static AppFunc Middleware(AppFunc app)
        {
            return new Static(app).Invoke;
        }

        public Task Invoke(IDictionary<string, object> env)
        {
            var path = env[OwinConstants.RequestPath].ToString();
            var method = env[OwinConstants.RequestMethod].ToString();

            if (("GET".Equals(method, StringComparison.OrdinalIgnoreCase)
                    || "HEAD".Equals(method, StringComparison.OrdinalIgnoreCase))
                && urls.Any(path.StartsWith))
            {
                return fileServer.Invoke(env);
            }
            else
            {
                return app.Invoke(env);
            }
        }
    }
}
