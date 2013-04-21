using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using System.Threading;

namespace Gate.Middleware.WebSockets
{
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

    // This class implements the WebSocket layer on top of an opaque stream.
    // WebSocket Extension v0.4 is currently implemented.
    internal class WebSocketLayer
    {
        private Stream incoming;
        private Stream outgoing;

        private IDictionary<string, object> environment;

        public WebSocketLayer(IDictionary<string, object> opaqueEnv)
        {
            this.environment = opaqueEnv;
            this.environment["websocket.SendAsync"] = new WebSocketSendAsync(SendAsync);
            this.environment["websocket.ReceiveAsync"] = new WebSocketReceiveAsync(ReceiveAsync);
            this.environment["websocket.CloseAsync"] = new WebSocketCloseAsync(CloseAsync);
            this.environment["websocket.CallCancelled"] = this.environment["opaque.CallCancelled"];
            this.environment["websocket.Version"] = "1.0";

            this.incoming = (Stream)this.environment["opaque.Incoming"];
            this.outgoing = (Stream)this.environment["opaque.Outgoing"];
        }

        public IDictionary<string, object> Environment
        {
            get { return environment; }
        }

        // Add framing and send the data.  One frame per call to Send.
        public Task SendAsync(ArraySegment<byte> buffer, int messageType, bool endOfMessage, CancellationToken cancel)
        {
            throw new NotImplementedException();
        }

        // Receive frames, unmask them.
        // Should handle pings/pongs internally.
        // Should parse out Close frames.
        public Task<WebSocketReceiveTuple> ReceiveAsync(ArraySegment<byte> buffer, CancellationToken cancel)
        {
            throw new NotImplementedException();
        }

        // Send a close frame.  The WebSocket is not actually considered closed until a close frame has been both sent and received.
        public Task CloseAsync(int status, string description, CancellationToken cancel)
        {
            // This could just be a wrapper around SendAsync, or at least they could share an internal helper send method.
            throw new NotImplementedException();
        }

        // Shutting down.  Send a close frame if one has been received but not set. Otherwise abort (fail the Task).
        public Task CleanupAsync()
        {
            throw new NotImplementedException();
        }
    }
}
