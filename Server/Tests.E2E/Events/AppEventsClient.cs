namespace Tests.E2E.Events;

using System.Buffers;
using System.Net.WebSockets;
using System.Threading.Channels;
using Game.Shared;
using Game.Shared.ServerEvents;
using MessagePack;
public sealed class AppEventsClient : IAsyncDisposable {
    private readonly ClientWebSocket          socket = new ClientWebSocket();
    private readonly Channel<IAppServerEvent> channel;
    private readonly CancellationTokenSource  cts = new CancellationTokenSource();
    private readonly Task                     readerTask;

    private AppEventsClient(ClientWebSocket socket, Channel<IAppServerEvent> channel, Task readerTask) {
        this.socket     = socket;
        this.channel    = channel;
        this.readerTask = readerTask;
    }

    public static async Task<AppEventsClient> ConnectAsync(E2EHost host, string accessToken) {
        var ws = new ClientWebSocket();
        ws.Options.SetRequestHeader("Authorization", $"Bearer {accessToken}");

        var uri = new Uri(new Uri(host.ServerBaseAddress, SharedConstants.UrlRoutes.ServerEvents.APP).ToString().Replace("http://", "ws://"));
        await ws.ConnectAsync(uri, CancellationToken.None);

        var ch   = Channel.CreateUnbounded<IAppServerEvent>();
        var task = Task.Run(async () => await ReadLoopAsync(ws, ch.Writer));
        return new AppEventsClient(ws, ch, task);
    }

    public async Task<T?> WaitForAsync<T>(TimeSpan timeout) where T : class, IAppServerEvent {
        using var timeoutCts = new CancellationTokenSource(timeout);
        try {
            while (!timeoutCts.IsCancellationRequested) {
                if (await channel.Reader.WaitToReadAsync(timeoutCts.Token)) {
                    while (channel.Reader.TryRead(out var ev)) {
                        if (ev is T t) {
                            return t;
                        }
                        // Skip events of other types
                    }
                }
            }
        }
        catch (OperationCanceledException) {
            // Timeout reached, return null
        }
        return null;
    }

    private static async Task ReadLoopAsync(ClientWebSocket socket, ChannelWriter<IAppServerEvent> writer) {
        var chunk = new byte[64 * 1024];
        var accum = new ArrayBufferWriter<byte>(64 * 1024);
        try {
            while (socket.State == WebSocketState.Open) {
                var result = await socket.ReceiveAsync(new ArraySegment<byte>(chunk), CancellationToken.None);
                if (result.MessageType == WebSocketMessageType.Close)
                    break;

                accum.Clear();
                accum.Write(chunk.AsSpan(0, result.Count));

                while (!result.EndOfMessage) {
                    result = await socket.ReceiveAsync(new ArraySegment<byte>(chunk), CancellationToken.None);
                    if (result.MessageType == WebSocketMessageType.Close)
                        break;
                    accum.Write(chunk.AsSpan(0, result.Count));
                }

                var bytes = accum.WrittenMemory.ToArray();
                var ev    = MessagePackSerializer.Deserialize<IAppServerEvent>(bytes);
                await writer.WriteAsync(ev);
            }
        }
        catch { }
        finally {
            writer.TryComplete();
        }
    }

    public async ValueTask DisposeAsync() {
        try {
            cts.Cancel();
            if (socket.State == WebSocketState.Open) {
                await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "", CancellationToken.None);
            }
        }
        catch { }
    }
}