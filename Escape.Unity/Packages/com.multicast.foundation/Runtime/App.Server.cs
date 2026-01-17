namespace Multicast {
    using System;
    using System.Buffers;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Threading.Tasks;
    using Cysharp.Threading.Tasks;
    using JetBrains.Annotations;
    using MessagePack;
    using Server;
    using ServerData;
    using UniMob;
    using UnityEngine;
    using UnityEngine.Pool;

    public struct ServerRequests {
        [PublicAPI]
        [MustUseReturnValue]
        public UniTask<TResponse> Request<TRequest, TResponse>([NotNull] string urlRoute, [NotNull] TRequest request, ServerCallRetryStrategy retryStrategy)
            where TRequest : class, IServerRequest
            where TResponse : class, IServerResponse {
            return App.Current.ServerRequest<TRequest, TResponse>(urlRoute, request, retryStrategy);
        }

        [PublicAPI]
        [MustUseReturnValue]
        public UniTask Execute<TCommandContext, TCommandData, TCommandInterface>([NotNull] string urlRoute, [NotNull] TCommandInterface command, ServerCallRetryStrategy retryStrategy)
            where TCommandContext : class, IServerCommandContext
            where TCommandData : SdObject
            where TCommandInterface : class, IServerCommand<TCommandData> {
            return App.Current.ServerExecute<TCommandContext, TCommandData, TCommandInterface>(urlRoute, command, retryStrategy);
        }

        [PublicAPI]
        public void ConnectToEvents<TEvent>(Lifetime lifetime, string urlRoute, Action<bool> onConnectionLost)
            where TEvent : class, IServerEvent {
            App.Current.ServerConnectToEvents<TEvent>(lifetime, urlRoute, onConnectionLost).Forget();
        }
    }

    public partial class App {
        public static readonly Func<UniTask<string>> DefaultAuthDelegate =
            () => throw new ServerRequestException(ServerRequestFailReason.ConfigurationError, "Server requires authentication but AuthDelegate not configured for App");

        private static readonly Func<ServerAccessTokenInfo> ServerAccessTokenDelegate = () => ServerAccessTokenInfo;

        private static readonly Dictionary<Type, IServerCommandHandlerBase> ServerCommandHandlerCache = new Dictionary<Type, IServerCommandHandlerBase>();

        private static readonly ObjectPool<ArrayBufferWriter<byte>> ArrayBufferWriterPool = new ObjectPool<ArrayBufferWriter<byte>>(
            () => new ArrayBufferWriter<byte>(), actionOnRelease: it => it.Clear());

        private ServerAccessTokenInfo?   serverAccessTokenInfoCached;
        private Func<UniTask<string>>    authDelegateCached;
        private Func<Exception, UniTask> retryDelegateCached;
        private Func<Exception, UniTask> badRequestDelegateCached;

        [PublicAPI]
        public static ServerAccessTokenInfo ServerAccessTokenInfo =>
            Current.serverAccessTokenInfoCached ??= ServerAccessTokenInfo.ParseFromAccessToken(ServerAccessToken);

        [PublicAPI]
        public static string ServerAccessToken {
            get => PlayerPrefs.GetString($"Multicast.App.AT{EditorCloneKey}", string.Empty);
            private set {
                Current.serverAccessTokenInfoCached = null;
                PlayerPrefs.SetString($"Multicast.App.AT{EditorCloneKey}", value);
            }
        }

        public static Func<UniTask<string>> AuthDelegate {
            get => Current.authDelegateCached ?? DefaultAuthDelegate;
            set => Current.authDelegateCached = value;
        }

        public static Func<Exception, UniTask> RetryDelegate {
            get => Current.retryDelegateCached;
            set => Current.retryDelegateCached = value;
        }

        public static Func<Exception, UniTask> BadRequestDelegate {
            get => Current.badRequestDelegateCached;
            set => Current.badRequestDelegateCached = value;
        }

        public static ServerRequests Server => new ServerRequests();

        [CanBeNull] private UniTaskCompletionSource activeServerExecuteCompletionSource;

        private MessagePackSerializerOptions serverDataMessagePackOptions = MessagePackSerializer.DefaultOptions
            .WithSecurity(MessagePackSecurity.UntrustedData)
            .WithCompression(MessagePackCompression.Lz4BlockArray);

        [PublicAPI]
        [MustUseReturnValue]
        public static Func<TCommandInterface, Task> CreateServerClientSideExecutor<TCommandContext, TCommandData, TCommandInterface>()
            where TCommandContext : class, IServerCommandContext
            where TCommandData : SdObject
            where TCommandInterface : class, IServerCommand<TCommandData> {
            return Current.ServerExecuteClientSide<TCommandContext, TCommandData, TCommandInterface>;
        }

        internal async UniTask ServerConnectToEvents<TEvent>(Lifetime lifetime, string urlRoute, Action<bool> onConnectionLost)
            where TEvent : class, IServerEvent {
            var settings = Get<IServerSettings>();

            var requestUrl = settings.ServerUrl + urlRoute;

            requestUrl = requestUrl switch {
                _ when requestUrl.StartsWith("http://") => "ws://" + requestUrl.Substring("http://".Length),
                _ when requestUrl.StartsWith("https://") => "wss://" + requestUrl.Substring("https://".Length),
                _ => requestUrl,
            };

            var retryIndex       = 0;
            var isConnectionLost = false;

            while (lifetime.IsDisposed == false) {
                try {
                    await CoreWebSocket.Connect(lifetime, requestUrl, new CoreWebSocketArgs {
                        AccessTokenDelegate = App.ServerAccessTokenDelegate,
                        AuthDelegate        = App.ServerAuthorize,
                    }, onConnect: OnConnect, onReceive: OnEventReceived);

                    await UniTask.Delay(TimeSpan.FromSeconds(1));
                }
                catch (OperationCanceledException) {
                    // Cancelled by user
                }
                catch (ServerRequestException ex) {
                    ++retryIndex;

                    if (retryIndex == 1) {
                        Debug.LogWarning($"ServerEvents({urlRoute}): Connection lost, trying to reconnect");
                        await UniTask.Delay(TimeSpan.FromSeconds(1));
                        continue;
                    }

                    Debug.LogException(ex);

                    if (isConnectionLost == false) {
                        isConnectionLost = true;
                        onConnectionLost?.Invoke(true);
                    }

                    await UniTask.Delay(retryIndex switch {
                        < 5 => TimeSpan.FromSeconds(1),
                        < 10 => TimeSpan.FromSeconds(10),
                        _ => TimeSpan.FromSeconds(60),
                    });
                }
            }

            return;

            void OnConnect() {
                retryIndex = 0;

                if (isConnectionLost) {
                    isConnectionLost = false;
                    onConnectionLost?.Invoke(false);
                }
            }

            void OnEventReceived(ReadOnlySequence<byte> payload) {
                if (Debug.unityLogger.IsLogTypeAllowed(LogType.Log)) {
                    var eventJson = MessagePackSerializer.ConvertToJson(payload);
                    Debug.Log($"ServerEvents({urlRoute}): Event: {eventJson}");
                }

                var msg = MessagePackSerializer.Deserialize<TEvent>(payload);
                App.Current.appEventHub.Raise(msg);
            }
        }

        internal async UniTask<TResponse> ServerRequest<TRequest, TResponse>([NotNull] string urlRoute, [NotNull] TRequest request, ServerCallRetryStrategy retryStrategy)
            where TRequest : class, IServerRequest
            where TResponse : class, IServerResponse {
            if (urlRoute == null) {
                throw new ArgumentNullException(nameof(urlRoute));
            }

            if (request == null) {
                throw new ArgumentNullException(nameof(request));
            }

            var lifetime = Get<Lifetime>();
            var settings = Get<IServerSettings>();

            var requestUrl    = settings.ServerUrl + urlRoute;
            var requestBytes  = MessagePackSerializer.Serialize(request);
            var responseBytes = await SendWebRequestAsync(lifetime, requestUrl, requestBytes, retryStrategy);
            var response      = MessagePackSerializer.Deserialize<ServerResult<TResponse>>(responseBytes);

            if (response.ErrorCode != 0 || !string.IsNullOrEmpty(response.ErrorMessage)) {
                App.Analytics.Send(new ServerRequestUserErrorAnalyticEvent {
                    requestUrl   = requestUrl,
                    errorMessage = $"{response.ErrorCode}:{response.ErrorMessage}",
                });

                throw new ServerRequestException(ServerRequestFailReason.UserError, $"Url={requestUrl}, ErrorCode={response.ErrorCode}, ErrorMessage={response.ErrorMessage}") {
                    UserErrorMessage = response.ErrorMessage,
                };
            }

            if (Debug.unityLogger.IsLogTypeAllowed(LogType.Log)) {
                var requestJson  = MessagePackSerializer.ConvertToJson(requestBytes);
                var responseJson = MessagePackSerializer.ConvertToJson(responseBytes);
                Debug.Log($"ServerRequest({urlRoute}): Request: {requestJson}\nResponse: {responseJson}");
            }

            return response.Data;
        }

        internal UniTask ServerExecute<TCommandContext, TCommandData, TCommandInterface>([NotNull] string urlRoute, [NotNull] TCommandInterface command, ServerCallRetryStrategy retryStrategy)
            where TCommandContext : class, IServerCommandContext
            where TCommandData : SdObject
            where TCommandInterface : class, IServerCommand<TCommandData> {
            if (urlRoute == null) {
                throw new ArgumentNullException(nameof(urlRoute));
            }

            if (command == null) {
                throw new ArgumentNullException(nameof(command));
            }

            if (command is not IServerCommandExecutableFromClient) {
                throw new InvalidOperationException($"Server command '{command.GetType().Name}' does not marked as {nameof(IServerCommandExecutableFromClient)}");
            }

            if (!MessagePackUnionInfo<TCommandInterface>.RegisteredSubTypes.Contains(command.GetType())) {
                throw new InvalidOperationException($"Server command '{command.GetType().Name}' does not registered by UnionAttribute at '{typeof(TCommandInterface).Name}'");
            }

            var tcs = new UniTaskCompletionSource();
            Core().Forget();
            return tcs.Task;

            async UniTask Core() {
                var lifetime = Get<Lifetime>();
                var settings = Get<IServerSettings>();
                var sdData   = Get<TCommandData>();

                var requestUrl   = settings.ServerUrl + urlRoute;
                var requestBytes = MessagePackSerializer.Serialize<TCommandInterface>(command);

                var lastServerExecuteTcs = this.activeServerExecuteCompletionSource;
                var ourServerExecuteTcs  = new UniTaskCompletionSource();

                ArrayBufferWriter<byte> clientSideBytes = null;

                try {
                    this.activeServerExecuteCompletionSource = ourServerExecuteTcs;

                    if (command is IServerCommandClientSidePredictable) {
                        var commandDeserialized = MessagePackSerializer.Deserialize<TCommandInterface>(requestBytes);
                        await this.ServerExecuteClientSide<TCommandContext, TCommandData, TCommandInterface>(commandDeserialized);

                        clientSideBytes = ArrayBufferWriterPool.Get();
                        SdObjectSerializer.Serialize(sdData, clientSideBytes, this.serverDataMessagePackOptions);
                        tcs.TrySetResult();

                        App.RequestAppUpdateFlow();
                    }

                    if (lastServerExecuteTcs != null) {
                        await lastServerExecuteTcs.Task;
                    }

                    var responseBytes = await SendWebRequestAsync(lifetime, requestUrl, requestBytes, retryStrategy);

                    if (clientSideBytes != null && !clientSideBytes.WrittenSpan.SequenceEqual(responseBytes)) {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                        if (Application.isEditor || Debug.isDebugBuild) {
                            var clientSideJson = MessagePackSerializer.ConvertToJson(clientSideBytes.WrittenMemory);
                            var serverSideJson = MessagePackSerializer.ConvertToJson(responseBytes);
                            Debug.LogError($"ServerExecute client side prediction mismatch for command '{command.GetType().Name}', dump:" +
                                           $"\nCLIENT: {clientSideJson}" +
                                           $"\nSERVER: {serverSideJson}");
                        }
                        else
#endif
                        {
                            Debug.LogError($"ServerExecute client side prediction mismatch for command '{command.GetType().Name}'");
                        }
                    }

                    SdObjectSerializer.Deserialize(sdData, responseBytes, this.serverDataMessagePackOptions);

                    if (Debug.unityLogger.IsLogTypeAllowed(LogType.Log)) {
                        var commandJson = MessagePackSerializer.SerializeToJson<TCommandInterface>(command);
                        Debug.Log($"ServerExecute({urlRoute}): {command.GetType().Name} {commandJson}\nResponse: {sdData}");
                    }

                    App.RequestAppUpdateFlow();
                }
                catch (OperationCanceledException) {
                    tcs.TrySetCanceled();
                }
                catch (Exception ex) {
                    tcs.TrySetException(ex);
                }
                finally {
                    if (clientSideBytes != null) {
                        ArrayBufferWriterPool.Release(clientSideBytes);
                    }

                    ourServerExecuteTcs.TrySetResult();
                    tcs.TrySetResult();
                }
            }
        }

        private async Task ServerExecuteClientSide<TCommandContext, TCommandData, TCommandInterface>([NotNull] TCommandInterface command)
            where TCommandContext : class, IServerCommandContext
            where TCommandData : SdObject
            where TCommandInterface : class, IServerCommand<TCommandData> {
            if (command == null) {
                throw new ArgumentNullException(nameof(command));
            }

            if (command is not IServerCommandClientSidePredictable) {
                throw new InvalidOperationException($"Server command '{command.GetType().Name}' does not marked as {nameof(IServerCommandClientSidePredictable)}");
            }

            if (!ServerCommandHandlerCache.TryGetValue(command.GetType(), out var handlerUntyped) ||
                handlerUntyped is not IServerCommandHandler<TCommandContext, TCommandData> handler) {
                throw new InvalidOperationException($"Server command '{command.GetType().Name}' cannot be executed locally, no handler registered");
            }

            var context  = Get<TCommandContext>();
            var gameData = Get<TCommandData>();

            await handler.Execute(context, gameData, command);
        }

        public static async UniTask<bool> ServerAuthorize() {
            try {
                ServerAccessToken = await AuthDelegate.Invoke();

                if (Debug.unityLogger.IsLogTypeAllowed(LogType.Log)) {
                    Debug.Log($"ServerAccessToken updated: {ServerAccessTokenInfo}");
                }

                return true;
            }
            catch (Exception ex) {
                Debug.LogException(ex);
                return false;
            }
        }

        private static readonly int        ServerCallTimeout             = 1;
        private static readonly int        ServerCallRetryCount          = 2;
        private static readonly TimeSpan[] ServerCallRetryDelays         = { TimeSpan.FromMilliseconds(10), TimeSpan.FromMilliseconds(100) };
        private static readonly int[]      ServerCallRetryTimeoutSeconds = { 3, 1 };

        private static async UniTask<byte[]> SendWebRequestAsync(Lifetime lifetime, string requestUrl, byte[] requestBytes, ServerCallRetryStrategy retryStrategy) {
            while (true) {
                try {
                    return await CoreWebRequest.SendRequestAsync(lifetime, requestUrl, requestBytes, new CoreWebRequestArgs {
                        AuthDelegate        = App.ServerAuthorize,
                        AccessTokenDelegate = App.ServerAccessTokenDelegate,
                        TimeoutSeconds      = App.ServerCallTimeout,
                        RetryCount          = App.ServerCallRetryCount,
                        RetryDelays         = App.ServerCallRetryDelays,
                        RetryTimeoutSeconds = App.ServerCallRetryTimeoutSeconds,
                    });
                }
                catch (ServerRequestException ex) when (ex.Reason == ServerRequestFailReason.UserError) {
                    Debug.LogException(ex);

                    if (App.BadRequestDelegate == null) {
                        Debug.LogError("App.BadRequestDelegate doest not configured");
                        throw;
                    }

                    await App.BadRequestDelegate(ex);
                }
                catch (ServerRequestException ex) when (retryStrategy == ServerCallRetryStrategy.RetryWithUserDialog) {
                    Debug.LogException(ex);

                    if (App.RetryDelegate == null) {
                        Debug.LogError("App.RetryDelegate doest not configured. Skip RetryWithUserDialog server call retry strategy");
                        throw;
                    }

                    await App.RetryDelegate(ex);
                }
            }
        }

        internal void RegisterServerCommandHandler(
            [NotNull] Type commandContextType,
            [NotNull] Type commandDataType,
            [NotNull] Type commandType,
            [NotNull] IServerCommandHandlerBase handler) {
            if (commandContextType == null) {
                throw new ArgumentNullException(nameof(commandContextType));
            }

            if (commandDataType == null) {
                throw new ArgumentNullException(nameof(commandDataType));
            }

            if (commandType == null) {
                throw new ArgumentNullException(nameof(commandType));
            }

            if (handler == null) {
                throw new ArgumentNullException(nameof(handler));
            }

            try {
                ServerCommandHandlerCache[commandType] = handler;
            }
            catch (Exception) {
                Debug.LogWarning($"Failed to register command handler for command '{commandType.Name}'");
            }
        }

        private static class MessagePackUnionInfo<T> {
            public static HashSet<Type> RegisteredSubTypes { get; }

            static MessagePackUnionInfo() {
                RegisteredSubTypes = typeof(T)
                    .GetCustomAttributes<UnionAttribute>()
                    .Select(it => it.SubType)
                    .ToHashSet();
            }
        }
    }

    public enum ServerCallRetryStrategy {
        Throw,
        RetryWithUserDialog,
    }
}