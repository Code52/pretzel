using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Owin;
using System.Threading.Tasks;
using Gate.Middleware;
using System.IO;
using Gate;

namespace Owin
{
    internal static class LoggerExtensions
    {
        public static IAppBuilder UseRequestLogger(this IAppBuilder builder)
        {
            return builder.UseType<RequestLogger>();
        }
    }
}

namespace Gate.Middleware
{
    using AppFunc = Func<IDictionary<string, object>, Task>;

    // This middleware logs incoming and outgoing environment and properties variables, headers, etc.
    internal class RequestLogger
    {
        private readonly AppFunc nextApp;
        private TextWriter loggerOverride;

        public RequestLogger(AppFunc next)
        {
            nextApp = next;
        }

        public RequestLogger(AppFunc next, TextWriter loggerOverride)
        {
            nextApp = next;
            this.loggerOverride = loggerOverride;
        }

        public Task Invoke(IDictionary<string, object> env)
        {
            var req = new Request(env);
            var logger = loggerOverride ?? req.TraceOutput;

            if (logger == null)
            {
                return nextApp(env);
            }

            LogCall(logger, env);
            return nextApp(env).Then(() =>
            {
                LogResult(logger, env);
            });
        }

        private void LogCall(TextWriter logger, IDictionary<string, object> env)
        {
            var req = new Request(env);

            logger.WriteLine("{0} - Request: Environment#{1}", DateTime.Now, env.Count);

            logger.WriteLine("Environment: ");
            LogDictionary(logger, env);

            logger.WriteLine("Headers: ");
            LogHeaders(logger, req.Headers);
        }

        private void LogResult(TextWriter logger, IDictionary<string, object> env)
        {
            var resp = new Response(env);

            logger.WriteLine("{0} - Response: Environment#{1}", DateTime.Now, env.Count);

            logger.WriteLine("Environment: ");
            LogDictionary(logger, env);

            logger.WriteLine("Headers: ");
            LogHeaders(logger, resp.Headers);
        }

        private void LogDictionary(TextWriter logger, IDictionary<string, object> dictionary)
        {
            foreach (KeyValuePair<string, object> pair in dictionary)
            {
                logger.WriteLine("{0} - T:{1}, V:{2}", pair.Key,
                    (pair.Value == null ? "(null)" : pair.Value.GetType().FullName),
                    (pair.Value == null ? "(null)" : pair.Value.ToString()));
            }
        }

        private void LogHeaders(TextWriter logger, IDictionary<string, string[]> headers)
        {
            foreach (KeyValuePair<string, string[]> header in headers)
            {
                foreach (string value in header.Value)
                {
                    logger.WriteLine("{0}: {1}", header.Key, value ?? "(null)");
                }
            }
        }
    }
}
