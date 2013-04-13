using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using Gate.Middleware.Utils;
using Owin;
using System.Threading.Tasks;

namespace Gate.Middleware.StaticFiles
{
    using AppFunc = Func<IDictionary<string, object>, Task>;
    using SendFileFunc = Func<string, long, long?, Task>;

    // Used by the Static middleware to send static files to the client.
    internal class FileServer
    {
        private const int OK = 200;
        private const int PartialContent = 206;
        private const int NotFound = 404;
        private const int Forbidden = 403;
        private const int RequestedRangeNotSatisfiable = 416;

        private readonly string root;

        public FileServer(string root)
        {
            this.root = root;
        }

        public Task Invoke(IDictionary<string, object> env)
        {
            var req = new Request(env);
            string pathInfo = req.Path;

            if (pathInfo.StartsWith("/"))
            {
                pathInfo = pathInfo.Substring(1);
            }

            if (pathInfo.Contains(".."))
            {
                return Fail(Forbidden, "Forbidden").Invoke(env);
            }

            string path = Path.Combine(root ?? string.Empty, pathInfo);

            if (!File.Exists(path))
            {
                return Fail(NotFound, "File not found: " + pathInfo).Invoke(env);
            }

            try
            {
                return Serve(env, path);
            }
            catch (UnauthorizedAccessException)
            {
                return Fail(Forbidden, "Forbidden").Invoke(env);
            }
        }

        private static AppFunc Fail(int status, string body, string headerName = null, string headerValue = null)
        {
            return env =>
                {
                    Request request = new Request(env);
                    Response response = new Response(env);
                    response.StatusCode = status;
                    response.Headers
                        .SetHeader("Content-Type", "text/plain")
                        .SetHeader("Content-Length", body.Length.ToString(CultureInfo.InvariantCulture))
                        .SetHeader("X-Cascade", "pass");

                    if (headerName != null && headerValue != null)
                    {
                        response.Headers.SetHeader(headerName, headerValue);
                    }

                    if ("HEAD".Equals(request.Method, StringComparison.OrdinalIgnoreCase))
                    {
                        return TaskHelpers.Completed();
                    }

                    return response.WriteAsync(body);
                };
        }

        private Task Serve(IDictionary<string, object> env, string path)
        {
            Request request = new Request(env);
            Response response = new Response(env);

            var fileInfo = new FileInfo(path);
            var size = fileInfo.Length;
            Tuple<long, long> range;

            if (!RangeHeader.IsValid(request.Headers))
            {
                response.StatusCode = OK;
                range = new Tuple<long, long>(0, size - 1);
            }
            else
            {
                var ranges = RangeHeader.Parse(request.Headers, size);

                if (ranges == null)
                {
                    // Unsatisfiable.  Return error and file size.
                    return Fail(
                        RequestedRangeNotSatisfiable,
                        "Byte range unsatisfiable",
                        "Content-Range", "bytes */" + size)
                        .Invoke(env);
                }

                if (ranges.Count() > 1)
                {
                    // TODO: Support multiple byte ranges.
                    response.StatusCode = OK;
                    range = new Tuple<long, long>(0, size - 1);
                }
                else
                {
                    // Partial content
                    range = ranges.First();
                    response.StatusCode = PartialContent;
                    response.Headers.SetHeader("Content-Range", "bytes " + range.Item1 + "-" + range.Item2 + "/" + size);
                    size = range.Item2 - range.Item1 + 1;
                }
            }

            response.Headers
                .SetHeader("Last-Modified", fileInfo.LastWriteTimeUtc.ToHttpDateString())
                .SetHeader("Content-Type", Mime.MimeType(fileInfo.Extension, "text/plain"))
                .SetHeader("Content-Length", size.ToString(CultureInfo.InvariantCulture));

            if ("HEAD".Equals(request.Method, StringComparison.OrdinalIgnoreCase))
            {
                // Suppress the body.
                return TaskHelpers.Completed();
            }

            FileBody body = new FileBody(path, range);

            //TODO: update for current send file spec
            //var req = new Request(env);
            //SendFileFunc sendFile = env.Get<SendFileFunc>("sendfile.Func");
            //if (sendFile != null)
            //{
            //    return body.Start(sendFile);
            //}

            return body.Start(response.OutputStream);
        }
    }
}
