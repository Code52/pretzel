using Owin;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Gate.Middleware
{
    using AppFunc = Func<IDictionary<string, object>, Task>;

    // Reads the X-Http-Method-Override header value to replace the request method. This is useful when 
    // intermediate client, proxy, firewall, or server software does not understand or permit the necessary 
    // methods.
    internal static class MethodOverride
    {
        public static IAppBuilder UseMethodOverride(this IAppBuilder builder)
        {
            return builder.UseFunc<AppFunc>(Middleware);
        }

        public static AppFunc Middleware(AppFunc app)
        {
            return env =>
            {
                var req = new Request(env);
                var method = req.Headers.GetHeader("x-http-method-override");
                if (!string.IsNullOrWhiteSpace(method))
                    req.Method = method;

                return app(env);
            };
        }
    }
}
