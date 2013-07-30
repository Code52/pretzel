using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using Gate.Middleware;
using System.Threading.Tasks;
using Gate.Middleware.Utils;

namespace Owin
{
    using AppFunc = Func<IDictionary<string, object>, Task>;

    internal static class ShowExceptionsExtensions
    {
        public static IAppBuilder UseShowExceptions(this IAppBuilder builder)
        {
            return builder.UseFunc<AppFunc>(ShowExceptions.Middleware);
        }
    }
}

namespace Gate.Middleware
{
    using AppFunc = Func<IDictionary<string, object>, Task>;
    
    // Catches any exceptions throw from the App Delegate or Body Delegate and returns an HTML error page.
    // If possible a full 500 Internal Server Error is returned.  Otherwise error information is written
    // out as part of the existing response body.
    //
    // This is not recommended for production deployments, only development, as it may display sensitive
    // internal data to the end user.  It also does not honor content-length restrictions.
    internal static partial class ShowExceptions
    {
        public static AppFunc Middleware(AppFunc app)
        {
            return env =>
            {
                Action<Exception, Action<byte[], int, int>> showErrorMessage =
                    (ex, write) =>
                        ErrorPage(env, ex, text =>
                        {
                            var data = Encoding.ASCII.GetBytes(text);
                            write(data, 0, data.Length);
                        });

                var response = new Response(env);
                Func<Exception, Task> showErrorPage = ex =>
                {
                    response.Status = "500 Internal Server Error";
                    response.ContentType = "text/html";
                    showErrorMessage(ex, response.Write);
                    return TaskHelpers.Completed();
                };

                // Don't try to modify the headers after the first write has occurred.
                TriggerStream triggerStream = new TriggerStream(response.Body);
                response.Body = triggerStream;

                bool bodyHasStarted = false;
                triggerStream.OnFirstWrite = () =>
                {
                    bodyHasStarted = true;
                };

                try
                {
                    return app(env)
                        .Catch(errorInfo =>
                        {
                            if (!bodyHasStarted)
                            {
                                showErrorPage(errorInfo.Exception).Wait();
                            }
                            else
                            {
                                showErrorMessage(errorInfo.Exception, triggerStream.Write);
                            }
                            return errorInfo.Handled();
                        });
                }
                catch (Exception exception)
                {
                    if (!bodyHasStarted)
                    {
                        return showErrorPage(exception);
                    }
                    else
                    {
                        showErrorMessage(exception, triggerStream.Write);
                        return TaskHelpers.Completed();
                    }
                }
            };
        }

        static string h(object text)
        {
            return Convert.ToString(text).Replace("<", "&lt;").Replace(">", "&gt;");
        }


        static IEnumerable<Frame> StackFrames(Exception ex)
        {
            return StackFrames(StackTraces(ex).Reverse());
        }

        static IEnumerable<string> StackTraces(Exception ex)
        {
            for (var scan = ex; scan != null; scan = scan.InnerException)
            {
                yield return ex.StackTrace;
            }
        }

        static IEnumerable<Frame> StackFrames(IEnumerable<string> stackTraces)
        {
            foreach (var stackTrace in stackTraces.Where(value => !string.IsNullOrWhiteSpace(value)))
            {
                var heap = new Chunk { Text = stackTrace + "\r\n", End = stackTrace.Length + 2 };
                for (var line = heap.Advance("\r\n"); line.HasValue; line = heap.Advance("\r\n"))
                {
                    yield return StackFrame(line);
                }
            }
        }

        static Frame StackFrame(Chunk line)
        {
            line.Advance("  at ");
            var function = line.Advance(" in ").ToString();
            var file = line.Advance(":line ").ToString();
            var lineNumber = line.ToInt32();

            return string.IsNullOrEmpty(file)
                ? LoadFrame(line.ToString(), "", 0)
                : LoadFrame(function, file, lineNumber);
            ;
        }

        static Frame LoadFrame(string function, string file, int lineNumber)
        {
            var frame = new Frame { Function = function, File = file, Line = lineNumber };
            if (File.Exists(file))
            {
                var code = File.ReadAllLines(file);
                frame.PreContextLine = Math.Max(lineNumber - 6, 1);
                frame.PreContextCode = code.Skip(frame.PreContextLine - 1).Take(lineNumber - frame.PreContextLine).ToArray();
                frame.ContextCode = code.Skip(lineNumber - 1).FirstOrDefault();
                frame.PostContextCode = code.Skip(lineNumber).Take(6).ToArray();
            }
            return frame;
        }

        internal class Chunk
        {
            public string Text;
            public int Start;
            public int End;

            public bool HasValue
            {
                get { return Text != null; }
            }

            public Chunk Advance(string delimiter)
            {
                var indexOf = HasValue ? Text.IndexOf(delimiter, Start, End - Start) : -1;
                if (indexOf < 0)
                    return new Chunk();

                var chunk = new Chunk { Text = Text, Start = Start, End = indexOf };
                Start = indexOf + delimiter.Length;
                return chunk;
            }

            public override string ToString()
            {
                return HasValue ? Text.Substring(Start, End - Start) : "";
            }

            public int ToInt32()
            {
                int value;
                return HasValue && Int32.TryParse(
                    Text.Substring(Start, End - Start),
                    NumberStyles.Integer,
                    CultureInfo.InvariantCulture,
                    out value) ? value : 0;
            }
        }

        internal class Frame
        {
            public string Function;
            public string File;
            public int Line;

            public int PreContextLine;
            public string[] PreContextCode;
            public string ContextCode;
            public string[] PostContextCode;
        }
    }
}