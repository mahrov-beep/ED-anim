namespace Game.ServerRunner.Core;

using System.Buffers;
using System.Net.WebSockets;
using JetBrains.Annotations;
using MessagePack;
using Multicast;

public static class ServerEvents<TEvent> where TEvent : class, IServerEvent {
    [PublicAPI]
    public static async Task AcceptHttpContextAsync(ILogger logger, HttpContext httpContext, InitializeDelegate initializer) {
        if (httpContext.WebSockets.IsWebSocketRequest == false) {
            logger.LogError("WebSocketServerEvents, Unexpected non-websocket http request");
            httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
            return;
        }

        if (!httpContext.TryGetUserId(out var userId)) {
            logger.LogError("WebSocketServerEvents, Request without userId claim");
            httpContext.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return;
        }

        try {
            using var webSocket = await httpContext.WebSockets.AcceptWebSocketAsync();

            await ProcessSocket(logger, webSocket, initializer, userId);
        }
        catch (WebSocketException webSocketException) {
            logger.LogWarning("WebSocketServerEvents, A websocket exception occured in Accept (UserId={UserId}, Message={Message})", userId, webSocketException.Message);
        }
        catch (Exception ex) {
            logger.LogError(ex, "WebSocketServerEvents, An exception occured (UserId={UserId})", userId);
        }
    }

    private static async Task ProcessSocket(ILogger logger, WebSocket webSocket, InitializeDelegate initializer, Guid userId) {
        var keepAliveTimeout = TimeSpan.FromSeconds(60);

        DisposeDelegate disposer = null;
        try {
            disposer = await initializer(userId, Send, Close);

            while (webSocket.State == WebSocketState.Open) {
                var timeout       = new CancellationTokenSource(keepAliveTimeout);
                var receiveResult = await webSocket.ReceiveAsync(ArraySegment<byte>.Empty, timeout.Token);

                if (receiveResult.MessageType == WebSocketMessageType.Close) {
                    break;
                }

                if (!receiveResult.EndOfMessage) {
                    logger.LogWarning("WebSocketServerEvents, Unexpected incoming message");
                    break;
                }
            }
        }
        catch (OperationCanceledException) {
            // receive timeout
        }
        catch (WebSocketException webSocketException) when (webSocketException.WebSocketErrorCode == WebSocketError.ConnectionClosedPrematurely) {
            // close without handshake
        }
        catch (Exception ex) {
            logger.LogError(ex, "WebSocketServerEvents, An exception occured in Receive (UserId={UserId})", userId);
        }
        finally {
            await Close("");
            await (disposer?.Invoke() ?? Task.CompletedTask);
        }

        // ReSharper disable AccessToDisposedClosure
        async Task Send(TEvent e) {
            try {
                var sendBuffer = new ArrayBufferWriter<byte>();
                MessagePackSerializer.Serialize(sendBuffer, e, cancellationToken: CancellationToken.None);
                await webSocket.SendAsync(sendBuffer.WrittenMemory, WebSocketMessageType.Binary, true, CancellationToken.None);
            }
            catch (Exception ex) {
                logger.LogError(ex, "WebSocketServerEvents, An exception occured in Send (UserId={UserId})", userId);
                await Close("");
            }
        }

        async Task Close(string reason) {
            if (WebSocketCanSend(webSocket)) {
                try {
                    await webSocket.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, reason, CancellationToken.None);
                }
                catch {
                    // close failed
                }
            }
        }
    }

    private static bool WebSocketCanSend(WebSocket ws) {
        return ws.State is not (WebSocketState.Aborted or WebSocketState.Closed or WebSocketState.CloseSent);
    }

    public delegate Task<DisposeDelegate> InitializeDelegate(Guid userId, SendDelegate send, CloseDelegate close);

    public delegate Task DisposeDelegate();

    public delegate Task SendDelegate(TEvent evt);

    public delegate Task CloseDelegate(string reason);
}