using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Owin;
using System.IO;

namespace Gate.Middleware.WebSockets
{
    using AppFunc = Func<IDictionary<string, object>, Task>;

    using OpaqueUpgrade =
        Action
        <
            IDictionary<string, object>, // Opaque Upgrade parameters
            Func // OpaqueFunc callback
            <
                IDictionary<string, object>, // Opaque environment
                Task // Complete
            >
        >;

    using OpaqueFunc =
        Func
        <
            IDictionary<string, object>, // Opaque environment
            Task // Complete
        >;

    using WebSocketAccept =
        Action
        <
            IDictionary<string, object>, // WebSocket Accept parameters
            Func // WebSocketFunc callback
            <
                IDictionary<string, object>, // WebSocket environment
                Task // Complete
            >
        >;

    using WebSocketFunc =
        Func
        <
            IDictionary<string, object>, // WebSocket environment
            Task // Complete
        >;

    using WebSocketSendAsync =
        Func
        <
            ArraySegment<byte> /* data */,
            int /* messageType */,
            bool /* endOfMessage */,
            CancellationToken /* cancel */,
            Task
        >;

    using WebSocketReceiveAsync =
        Func
        <
            ArraySegment<byte> /* data */,
            CancellationToken /* cancel */,
            Task
            <
                Tuple
                <
                    int /* messageType */,
                    bool /* endOfMessage */,
                    int? /* count */,
                    int? /* closeStatus */,
                    string /* closeStatusDescription */
                >
            >
        >;

    using WebSocketReceiveTuple =
        Tuple
        <
            int /* messageType */,
            bool /* endOfMessage */,
            int? /* count */,
            int? /* closeStatus */,
            string /* closeStatusDescription */
        >;

    using WebSocketCloseAsync =
        Func
        <
            int /* closeStatus */,
            string /* closeDescription */,
            CancellationToken /* cancel */,
            Task
        >;

    // This class demonstrates how to support WebSockets on a server that only supports opaque streams.
    // WebSocket Extension v0.4 is currently implemented.
    internal static class OpaqueToWebSocket
    {
        public static IAppBuilder UseWebSockets(this IAppBuilder builder)
        {
            return builder.UseFunc<AppFunc>(OpaqueToWebSocket.Middleware);
        }

        public static AppFunc Middleware(AppFunc app)
        {
            return env =>
            {
                var request = new Request(env);
                string opaqueSupport = request.Get<string>("opaque.Support");
                OpaqueUpgrade opaqueUpgrade = request.Get<OpaqueUpgrade>("opaque.Upgrade");
                string websocketSupport = request.Get<string>("websocket.Support");
                WebSocketAccept webSocketAccept = request.Get<WebSocketAccept>("websocket.Accept");

                if (opaqueSupport == "opaque.Upgrade" // If we have opaque support
                    && opaqueUpgrade != null
                    && websocketSupport != "websocket.Accept" // and no current websocket support
                    && webSocketAccept == null) // and this request is a websocket request...
                {
                    // This middleware is adding support for websockets.
                    env["websocket.Support"] = "websocket.Accept";

                    if (IsWebSocketRequest(env))
                    {
                        IDictionary<string, object> acceptOptions = null;
                        WebSocketFunc webSocketFunc = null;

                        // Announce websocket support
                        env["websocket.Accept"] = new WebSocketAccept(
                            (options, callback) =>
                            {
                                acceptOptions = options;
                                webSocketFunc = callback;
                                env[OwinConstants.ResponseStatusCode] = 101;
                            });


                        return app(env).Then(() =>
                        {
                            Response response = new Response(env);
                            if (response.StatusCode == 101
                                && webSocketFunc != null)
                            {
                                SetWebSocketResponseHeaders(env, acceptOptions);

                                opaqueUpgrade(acceptOptions, opaqueEnv =>
                                {
                                    WebSocketLayer webSocket = new WebSocketLayer(opaqueEnv);
                                    return webSocketFunc(webSocket.Environment)
                                        .Then(() => webSocket.CleanupAsync());
                                });
                            }
                        });
                    }
                }

                // else
                return app(env);
            };
        }

        // Inspect the method and headers to see if this is a valid websocket request.
        // See RFC 6455 section 4.2.1.
        private static bool IsWebSocketRequest(IDictionary<string, object> env)
        {
            throw new NotImplementedException();
        }

        // Se the websocket response headers.
        // See RFC 6455 section 4.2.2.
        private static void SetWebSocketResponseHeaders(IDictionary<string, object> env, IDictionary<string, object> acceptOptions)
        {
            throw new NotImplementedException();
        }
    }
}
