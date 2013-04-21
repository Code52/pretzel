using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using Gate.Utils;
using Owin;
using System;
using System.Collections.Generic;
using Gate.Middleware;
using Gate.Middleware.Utils;

namespace Owin
{
    internal static class ContentLengthExtensions
    {
        public static IAppBuilder UseContentLength(this IAppBuilder builder)
        {
            return builder.UseType<ContentLength>();
        }
    }
}

namespace Gate.Middleware
{
    using AppFunc = Func<IDictionary<string, object>, Task>;

    // This middleware defaults to Content-Length if the app does not specify Content-Length 
    // or Transfer-Encoding.  The Content-Length is determined by buffering the response body.
    internal class ContentLength
    {
        private readonly AppFunc nextApp;

        public ContentLength(AppFunc nextApp)
        {
            this.nextApp = nextApp;
        }

        public Task Invoke(IDictionary<string, object> env)
        {
            Request request = new Request(env);
            Response response = new Response(env);
            Stream orriginalStream = response.OutputStream;
            TriggerStream triggerStream = new TriggerStream(orriginalStream);
            response.OutputStream = triggerStream;
            MemoryStream buffer = null;

            triggerStream.OnFirstWrite = () =>
            {
                if (IsStatusWithNoNoEntityBody(response.StatusCode)
                    || response.Headers.ContainsKey("Content-Length")
                    || response.Headers.ContainsKey("Transfer-Encoding"))
                {
                    return;
                }

                // Buffer
                buffer = new MemoryStream();
                triggerStream.InnerStream = buffer;
            };

            env[OwinConstants.ResponseBody] = triggerStream;

            return nextApp(env).Then(() =>
            {
                if (buffer != null)
                {
                    if (buffer.Length == 0)
                    {
                        response.Headers.SetHeader("Content-Length", "0");
                    }
                    else
                    {
                        response.Headers.SetHeader("Content-Length", buffer.Length.ToString(CultureInfo.InvariantCulture));

                        // Suppress the body for HEAD requests.
                        if (!"HEAD".Equals(request.Method, StringComparison.OrdinalIgnoreCase))
                        {
                            buffer.Seek(0, SeekOrigin.Begin);
                            return buffer.CopyToAsync(orriginalStream);
                        }
                    }
                }
                else if (!IsStatusWithNoNoEntityBody(response.StatusCode)
                    && !response.Headers.ContainsKey("Content-Length")
                    && !response.Headers.ContainsKey("Transfer-Encoding"))
                {
                    // There were no Writes.
                    response.Headers.SetHeader("Content-Length", "0");
                }
                return TaskHelpers.Completed();
            });
        }

        private static bool IsStatusWithNoNoEntityBody(int status)
        {
            return (status >= 100 && status < 200) ||
                status == 204 ||
                status == 205 ||
                status == 304;
        }
    }
}

