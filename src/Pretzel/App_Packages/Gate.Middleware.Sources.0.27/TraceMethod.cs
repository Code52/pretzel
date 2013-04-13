using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Gate.Middleware;
using System.Threading.Tasks;

namespace Owin
{
    internal static class TraceExtensions
    {
        public static IAppBuilder UseTrace(this IAppBuilder builder)
        {
            return builder.UseType<TraceMethod>();
        }
    }
}

namespace Gate.Middleware
{
    using AppFunc = Func<IDictionary<string, object>, Task>;

    // This middleware implements support for the TRACE method by echoing back the request information as response body.
    // See RFC 2616 Section 9.8.
    // TODO: Is there a standard format for the output?
    internal class TraceMethod
    {
        private readonly AppFunc nextApp;

        public TraceMethod(AppFunc nextApp)
        {
            this.nextApp = nextApp;
        }

        public Task Invoke(IDictionary<string, object> env)
        {
            Request request = new Request(env);
            if (!"TRACE".Equals(request.Method, StringComparison.OrdinalIgnoreCase))
            {
                return this.nextApp(env);
            }

            Response response = new Response(env);
            response.ContentType = "message/http";

            StringBuilder builder = new StringBuilder();

            // Request line: Method, Uri, Version
            builder.Append(request.Method).Append(" ").Append(request.PathBase).Append(request.Path);
            if (request.QueryString != null)
            {
                builder.Append("?").Append(request.QueryString);
            }
            builder.Append(" ").Append(request.Protocol).AppendLine();


            // Headers
            foreach (KeyValuePair<string, string[]> pair in request.Headers)
            {
                foreach (string headerValue in pair.Value)
                {
                    builder.AppendLine(pair.Key + ": " + headerValue);
                }
            }

            // There must not be a request body for TRACE requests.

            response.ContentLength = builder.Length;
            return response.WriteAsync(builder.ToString());
        }
    }
}
