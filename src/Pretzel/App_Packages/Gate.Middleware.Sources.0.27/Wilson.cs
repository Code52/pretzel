using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Owin;
using System.Collections.Generic;
using Timer = System.Timers.Timer;

namespace Gate.Middleware
{
    using AppFunc = Func<IDictionary<string, object>, Task>;

    // A sample application that reads query parameters and returns a simple HTML page.
    // It can also be used to demonstrate error handling via the 'crash' parameter.
    internal class Wilson
    {
        public static AppFunc App(bool asyncReply)
        {
            return asyncReply ? WilsonAsync.App() : App();
        }

        public static AppFunc App()
        {
            return new Wilson().Invoke;
        }

        public Task Invoke(IDictionary<string, object> env)
        {
            var request = new Request(env);
            var response = new Response(env) {ContentType = "text/html"};
            var wilson = "left - right\r\n123456789012\r\nhello world!\r\n";

            var href = "?flip=left";
            if (request.Query["flip"] == "left")
            {
                wilson = wilson.Split(new[] {System.Environment.NewLine}, StringSplitOptions.RemoveEmptyEntries)
                    .Select(line => new string(line.Reverse().ToArray()))
                    .Aggregate("", (agg, line) => agg + line + System.Environment.NewLine);
                href = "?flip=right";
            }
            response.Write("<title>Wilson</title>");
            response.Write("<pre>");
            response.Write(wilson);
            response.Write("</pre>");
            if (request.Query["flip"] == "crash")
            {
                throw new ApplicationException("Wilson crashed!");
            }
            response.Write("<p><a href='" + href + "'>flip!</a></p>");
            response.Write("<p><a href='?flip=crash'>crash!</a></p>");

            return TaskHelpers.Completed();
        }        
    }

    internal class WilsonAsync
    {
        public static AppFunc App()
        {
            return new WilsonAsync().Invoke;
        }

        public Task Invoke(IDictionary<string, object> env)
        {
            var request = new Request(env);
            var response = new Response(env)
            {
                ContentType = "text/html",
            };
            var wilson = "left - right\r\n123456789012\r\nhello world!\r\n";

            var href = "?flip=left";
            if (request.Query["flip"] == "left")
            {
                wilson = wilson.Split(new[] {System.Environment.NewLine}, StringSplitOptions.RemoveEmptyEntries)
                    .Select(line => new string(line.Reverse().ToArray()))
                    .Aggregate("", (agg, line) => agg + line + System.Environment.NewLine);
                href = "?flip=right";
            }

            return TimerLoop(350, 
                () => response.Write("<title>Hutchtastic</title>"),
                () => response.Write("<pre>"), 
                () => response.Write(wilson), 
                () => response.Write("</pre>"), 
                () =>
                {
                    if (request.Query["flip"] == "crash")
                    {
                        throw new ApplicationException("Wilson crashed!");
                    }
                }, 
                () => response.Write("<p><a href='" + href + "'>flip!</a></p>"),
                () => response.Write("<p><a href='?flip=crash'>crash!</a></p>"));
        }

        static Task TimerLoop(double interval, params Action[] steps)
        {
            TaskCompletionSource<object> tcs = new TaskCompletionSource<object>();
            var iter = steps.AsEnumerable().GetEnumerator();
            var timer = new Timer(interval);
            timer.Elapsed += (sender, e) =>
            {
                if (iter != null && iter.MoveNext())
                {
                    try
                    {
                        iter.Current();
                    }
                    catch (Exception ex)
                    {
                        iter = null;
                        timer.Stop();
                        tcs.TrySetException(ex);
                    }
                }
                else
                {
                    tcs.TrySetResult(null);
                    timer.Stop();
                }
            };
            timer.Start();
            return tcs.Task;
        }
    }
}