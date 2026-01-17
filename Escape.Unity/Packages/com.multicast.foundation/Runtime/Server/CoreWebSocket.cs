namespace Multicast.Server {
    using System;
    using System.Buffers;
    using System.Net.WebSockets;
    using System.Threading;
    using Cysharp.Threading.Tasks;
    using JetBrains.Annotations;
    using UniMob;
    using UnityEngine;

    [RequireFieldsInit]
    public struct CoreWebSocketArgs {
        public Func<UniTask<bool>>         AuthDelegate;
        public Func<ServerAccessTokenInfo> AccessTokenDelegate;
    }

    public static class CoreWebSocket {
        public static int ReceiveBufferSizeBytes   = 1024 * 32;
        public static int HeartbeatIntervalSeconds = 30;

        [PublicAPI]
        // throws ServerRequestException, OperationCancelledException
        public static async UniTask Connect(Lifetime outerLifetime, string requestUrl, CoreWebSocketArgs args, Action onConnect, Action<ReadOnlySequence<byte>> onReceive) {
            // ClientWebSocket does not work on WebGL, use websocket jslib
            var webSocket      = new ClientWebSocket();
            var socketLc       = outerLifetime.CreateNested();
            var socketLifetime = socketLc.Lifetime;
            var receiveBuffer  = ArrayPool<byte>.Shared.Rent(ReceiveBufferSizeBytes);

            try {
                var accessToken = args.AccessTokenDelegate();

                if (accessToken.State != ServerAccessTokenState.Valid) {
                    await args.AuthDelegate();
                }
                
                if (!string.IsNullOrEmpty(accessToken.Raw)) {
                    // does not work on WebGL, pass ass query parameter
                    webSocket.Options.SetRequestHeader("Authorization", $"Bearer {accessToken.Raw}");
                }

                Debug.Log($"[CoreWebSocket] Connecting to websocket ({requestUrl})");
                await webSocket.ConnectAsync(new Uri(requestUrl), socketLifetime);
                Debug.Log($"[CoreWebSocket] Connected to websocket ({requestUrl})");
                InvokeConnectCallback();
                await UniTask.WhenAny(ReceiveLoop(), HeartbeatLoop());
                
                throw new Exception($"[CoreWebSocket] Websocket connection unexpectedly closed ({requestUrl}). Probably server is broken");
            }
            catch (WebSocketException webSocketException) {
                throw new ServerRequestException(ServerRequestFailReason.NetworkError, $"[CoreWebSocket] An exception thrown at websocket connection ({requestUrl}), Error={webSocketException.Message}");
            }
            finally {
                ArrayPool<byte>.Shared.Return(receiveBuffer, clearArray: true);
                socketLc.Dispose();
                webSocket.Dispose();
                await SafeClose();
                Debug.Log($"[CoreWebSocket] Disconnected from websocket ({requestUrl})");
            }

            // ReSharper disable AccessToDisposedClosure

            // throws WebSocketException, OperationCancelledException
            async UniTask ReceiveLoop() {
                while (socketLifetime.IsDisposed == false && webSocket.State == WebSocketState.Open) {
                    var receiveResult = await webSocket.ReceiveAsync(new ArraySegment<byte>(receiveBuffer), socketLifetime);

                    if (receiveResult.CloseStatus.HasValue) {
                        Debug.LogError($"[CoreWebSocket] Connection closed by server ({requestUrl}), Status={receiveResult.CloseStatus.Value}");
                        await SafeClose();
                        break;
                    }

                    if (!receiveResult.EndOfMessage) {
                        Debug.LogError($"[CoreWebSocket] Received message is too large ({requestUrl})");
                        await SafeClose();
                        break;
                    }

                    InvokeReceiveCallback(new ReadOnlySequence<byte>(receiveBuffer, 0, receiveResult.Count));
                }
            }

            // throws OperationCancelledException
            async UniTask HeartbeatLoop() {
                while (socketLifetime.IsDisposed == false && webSocket.State == WebSocketState.Open) {
                    try {
                        await webSocket.SendAsync(ArraySegment<byte>.Empty, WebSocketMessageType.Binary, true, socketLifetime);
                    }
                    catch (WebSocketException webSocketException) {
                        Debug.LogWarning($"[CoreWebSocket] Failed to send websocket heartbeat ({requestUrl}), Message={webSocketException.Message}");
                    }

                    await UniTask.Delay(TimeSpan.FromSeconds(HeartbeatIntervalSeconds), DelayType.Realtime, cancellationToken: socketLifetime);
                }
            }

            async UniTask SafeClose() {
                try {
                    socketLc.Dispose();
                    
                    if (webSocket.State == WebSocketState.Open) {
                        await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "", CancellationToken.None);
                    }
                }
                catch (WebSocketException webSocketException) {
                    Debug.LogError($"[CoreWebSocket] Failed to disconnect from websocket ({requestUrl}), Error={webSocketException.Message}");
                }
            }

            async void InvokeConnectCallback() {
                try {
                    onConnect?.Invoke();
                }
                catch (Exception ex) {
                    Debug.LogException(ex);
                }
            }

            // throws nothing
            async void InvokeReceiveCallback(ReadOnlySequence<byte> payload) {
                try {
                    onReceive?.Invoke(payload);
                }
                catch (Exception ex) {
                    Debug.LogException(ex);
                }
            }

            // ReSharper restore AccessToDisposedClosure
        }
    }
}