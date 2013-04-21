using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Gate.Middleware;
using Owin;
using System.Threading.Tasks;
using System.Diagnostics;
using Gate;

namespace Owin
{
    internal static class TracerExtensions
    {
        public static IAppBuilder UseRequestTracer(this IAppBuilder builder)
        {
            var builderProperties = new BuilderProperties(builder.Properties);
            TraceSource traceSource = builderProperties.Get<TraceSource>("host.TraceSource");
            return builder.UseType<RequestTracer>(traceSource);
        }
    }
}

namespace Gate.Middleware
{
    using AppFunc = Func<IDictionary<string, object>, Task>;

    // This middleware traces incoming and outgoing environment and properties variables, headers, etc.
    internal class RequestTracer
    {
        private readonly AppFunc nextApp;
        private TraceSource traceSource;

        public RequestTracer(AppFunc next)
        {
            nextApp = next;
        }

        public RequestTracer(AppFunc next, TraceSource source)
        {
            nextApp = next;
            traceSource = source;
        }
        
        public Task Invoke(IDictionary<string, object> env)
        {
            // The TraceSource is assumed to be the same across all requests.

            traceSource = traceSource ?? new Request(env).Get<TraceSource>("host.TraceSource");

            if (traceSource == null)
            {
                return nextApp(env);
            }

            try
            {
                TraceCall(env);
                return nextApp(env).Then(() =>
                {
                    TraceResult(env);
                })
                .Catch(errorInfo =>
                {
                    TraceException(errorInfo.Exception, "asynchronously");
                    return errorInfo.Throw();
                });
            }
            catch (Exception ex)
            {
                TraceException(ex, "synchronously");
                throw;
            }
        }

        private void TraceCall(IDictionary<string, object> env)
        {
            var req = new Request(env);
            traceSource.TraceEvent(TraceEventType.Start, 0, "Request: Environment#{0}", env.Count);

            traceSource.TraceInformation("Environment: ");
            TraceDictionary(env);

            traceSource.TraceInformation("Headers: ");
            TraceHeaders(req.Headers);
        }

        private void TraceResult(IDictionary<string, object> env)
        {
            var resp = new Response(env);
            traceSource.TraceEvent(TraceEventType.Stop, 0, "Request: Environment#{0}", env.Count);

            traceSource.TraceInformation("Environment: ");
            TraceDictionary(env);

            traceSource.TraceInformation("Headers: ");
            TraceHeaders(resp.Headers);
        }

        private void TraceDictionary(IDictionary<string, object> dictionary)
        {
            foreach (KeyValuePair<string, object> pair in dictionary)
            {
                traceSource.TraceData(TraceEventType.Verbose, 0, 
                    string.Format("{0} - T:{1}, V:{2}", pair.Key,
                    (pair.Value == null ? "(null)" : pair.Value.GetType().FullName),
                    (pair.Value == null ? "(null)" : pair.Value.ToString())));
            }
        }

        private void TraceHeaders(IDictionary<string, string[]> headers)
        {
            foreach (KeyValuePair<string, string[]> header in headers)
            {
                foreach (string value in header.Value)
                {
                    traceSource.TraceData(TraceEventType.Verbose, 0,
                        string.Format("{0}: {1}", header.Key,
                        (value == null ? "(null)" : value)));
                }
            }
        }

        private void TraceException(Exception exception, string place)
        {
            traceSource.TraceEvent(TraceEventType.Error, 0, "An exception was thrown {0}: {1}", place, exception);
        }
    }
}
