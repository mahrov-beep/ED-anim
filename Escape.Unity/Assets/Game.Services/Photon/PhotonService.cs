namespace Game.Services.Photon {
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Cysharp.Threading.Tasks;
    using global::Photon.Client;
    using global::Photon.Deterministic;
    using global::Photon.Realtime;
    using JetBrains.Annotations;
    using Multicast;
    using Quantum;
    using UniMob;
    using UnityEngine;
    using UnityEngine.SceneManagement;
    using Input = Quantum.Input;
    using LogType = UnityEngine.LogType;
    using UniTask = Cysharp.Threading.Tasks.UniTask;

    public class PhotonService {
        private static WeakReference<byte[]> weakFrameSerializeBuffer;
        
        private readonly EventBetter message = new EventBetter();

        private CancellationTokenSource connectCancellation;
        private CancellationTokenSource connectLinkedCancellation;

        public RealtimeClient Client { get; }

        [CanBeNull] public QuantumRunner Runner { get; set; }

        /// <summary>
        /// Return true if connecting or connected to any server.
        /// </summary>
        public bool IsConnected => this.Client?.IsConnected ?? false;

        /// <summary>
        /// Return the current ping (only in room).
        /// </summary>
        public int Ping => this.Runner?.Session != null ? this.Runner.Session.Stats.Ping : 0;

        [CanBeNull] public Frame VerifiedFrame {
            get {
                if (Runner == null) {
                    return null;
                }

                if (Runner.State != SessionRunner.SessionState.Running) {
                    return null;
                }

                var game = Runner.Game;
                return game?.Frames.Verified;
            }
        }

        [CanBeNull] public Frame PredictedFrame {
            get {
                if (Runner == null) {
                    return null;
                }

                if (Runner.State != SessionRunner.SessionState.Running) {
                    return null;
                }

                var game = Runner.Game;
                return game?.Frames.Predicted;
            }
        }

        public bool IsReconnectPossible => QuantumReconnectInformation.Load().HasTimedOut == false;

        public string CurrentGameId => this.Client.CurrentRoom?.Name;

        public Dictionary<int, Player> CurrentGameUsers => this.Client.CurrentRoom?.Players;

        public bool IsCurrentRoomQuantumStartAllowed => (bool)(this.Client.CurrentRoom?.CustomProperties[PhotonConstants.RoomProperties.START_QUANTUM] ?? false);

        public static bool HasDevAppVersion => !string.IsNullOrWhiteSpace(DevAppVersionPref);

        public static string DevAppVersionPref {
            get => PlayerPrefs.GetString("PhotonService.DevAppVersion", "");
            set => PlayerPrefs.SetString("PhotonService.DevAppVersion", value);
        }

        private static string ReconnectFrameFileName {
            get => PlayerPrefs.GetString($"Photon.Frame{App.EditorCloneKey}", "");
            set => PlayerPrefs.SetString($"Photon.Frame{App.EditorCloneKey}", value);
        }

        public event Action<string> UnexpectedlyDisconnected;

        public bool TryGetPredicted(out Frame f) {
            f = PredictedFrame;
            return f != null;
        }

        public Player GetPlayerByActorId(int actorId) => this.Client.CurrentRoom?.GetPlayer(actorId);

        public Guid GetUserIdByActorId(int actorId) {
            if (this.Client.CurrentRoom?.GetPlayer(actorId) is { } player &&
                Guid.TryParse(player.UserId, out var userId)) {
                return userId;
            }

            return Guid.Empty;
        }


        public PhotonService(Lifetime lifetime) {
            this.Client = new RealtimeClient();

            lifetime.Register(this.Client.CallbackMessage.ListenManual<OnErrorInfoMsg>(this.OnErrorInfo));
            lifetime.Register(this.Client.CallbackMessage.ListenManual<OnDisconnectedMsg>(msg => this.OnDisconnect(msg.cause)));
            lifetime.Register(QuantumCallback.SubscribeManual<CallbackPluginDisconnect>(this.OnPluginDisconnect));

            lifetime.Register(this.Client.CallbackMessage.ListenManual<OnRoomPropertiesUpdateMsg>(this.OnRoomPropertiesUpdated));
            lifetime.Register(this.Client.CallbackMessage.ListenManual<OnPlayerPropertiesUpdateMsg>(this.OnPlayerPropertiesUpdated));

            lifetime.RegisterToEvent(
                e => this.Client.StateChanged += e,
                e => this.Client.StateChanged -= e,
                new Action<ClientState, ClientState>(this.OnClientStateChanged)
            );

            App.Events.Listen(lifetime, (ApplicationFocusEvent _) => {
                this.RefreshReconnectInfo(saveFrameSnapshot: false);

                if (this.Runner != null &&
                    this.Runner.Session.GameMode == DeterministicGameMode.Multiplayer &&
                    this.Runner.Communicator.IsConnected == false) {
                    this.OnDisconnect(DisconnectCause.ServerTimeout);
                }
            });
            App.Events.Listen(lifetime, (ApplicationPauseEvent _) => this.RefreshReconnectInfo(saveFrameSnapshot: true));
            App.Events.Listen(lifetime, (ApplicationQuitEvent _) => this.RefreshReconnectInfo(saveFrameSnapshot: true));
        }

        public void RefreshReconnectInfo(bool saveFrameSnapshot) {
            if (this.Runner == null || this.Client == null || this.Client.CurrentRoom == null) {
                return;
            }

            var info = (QuantumReconnectInformation)QuantumReconnectInformation.Load();
            info.DefaultTimeout = TimeSpan.FromSeconds(PhotonServerSettings.Global.PlayerTtlInSeconds);
            info.Set(this.Client);
            Debug.Log($"Refresh QuantumReconnectInformation expiration: {info}");

            if (saveFrameSnapshot) {
                this.SaveFrameSnapshotForReconnect();
            }
        }

        private void SaveFrameSnapshotForReconnect() {
            try {
                if (this.Runner?.Game?.Frames?.Verified is { } f) {
                    if (!string.IsNullOrEmpty(ReconnectFrameFileName)) {
                        var oldFilePath = Path.Combine(Application.persistentDataPath, ReconnectFrameFileName);
                        if (File.Exists(oldFilePath)) {
                            File.Delete(oldFilePath);
                            Debug.Log($"[{nameof(PhotonService)}]: Cleanup previous quantum game snapshot");
                        }

                        ReconnectFrameFileName = string.Empty;
                    }

                    if (this.Runner.Session.GameMode == DeterministicGameMode.Multiplayer) {
                        ReconnectFrameFileName = $"QuantumData.{f.Number}";
                        var newFilePath = Path.Combine(Application.persistentDataPath, ReconnectFrameFileName);

                        if (weakFrameSerializeBuffer == null || !weakFrameSerializeBuffer.TryGetTarget(out var buffer)) {
                            buffer               = new byte[1024 * 1024 * 5]; // default is 20 MB
                            weakFrameSerializeBuffer = new WeakReference<byte[]>(buffer);
                            Debug.Log($"[{nameof(PhotonService)}]: Allocate frameSerialize buffer of {buffer.Length / 1024 / 1024} MB");
                        }

                        var serializedFrame = f.Serialize(DeterministicFrameSerializeMode.Blit, buffer);

                        using (var fs = new FileStream(newFilePath, FileMode.Create, FileAccess.Write, FileShare.Read)) {
                            fs.Write(serializedFrame);
                        }
                    }
                }
            }
            catch (Exception ex) {
                Debug.LogException(ex);
            }
        }

        private bool TryLoadSnapshotForReconnect(out int frameNumber, out byte[] frameData) {
            frameNumber = 0;
            frameData   = null;

            try {
                if (string.IsNullOrEmpty(ReconnectFrameFileName)) {
                    return false;
                }

                if (!int.TryParse(ReconnectFrameFileName.Substring("QuantumData.".Length), out frameNumber)) {
                    return false;
                }

                var filePath = Path.Combine(Application.persistentDataPath, ReconnectFrameFileName);
                if (!File.Exists(filePath)) {
                    return false;
                }

                frameData = File.ReadAllBytes(filePath);
                return true;
            }
            catch (Exception ex) {
                Debug.LogException(ex);
                return false;
            }
        }

        private void OnRoomPropertiesUpdated(OnRoomPropertiesUpdateMsg m) {
            if (Debug.unityLogger.IsLogTypeAllowed(LogType.Log)) {
                Debug.Log($"[{nameof(PhotonService)}]: OnRoomPropertiesUpdated, Table={m.changedProps.ToStringFull()}");
            }
        }

        private void OnPlayerPropertiesUpdated(OnPlayerPropertiesUpdateMsg m) {
            if (Debug.unityLogger.IsLogTypeAllowed(LogType.Log)) {
                Debug.Log($"[{nameof(PhotonService)}]: OnPlayerPropertiesUpdated, ActorNumber={m.targetPlayer.ActorNumber}, Table={m.changedProps.ToStringFull()}");
            }
        }

        [PublicAPI]
        public async Task<ConnectResult> ConnectAndJoinGameAsync(PhotonGameSetupArgs setupArgs, PhotonGameConnectArgs connectArgs, IProgress<string> progress, IProgress<string> parameters, IProgress<float> progress01) {
            PatchArgs(setupArgs);

            if (Debug.unityLogger.IsLogTypeAllowed(LogType.Log)) {
                Debug.Log($"[{nameof(PhotonService)}]: ConnectAndJoinGameAsync(" +
                          $"Session={connectArgs.Session}, Region={connectArgs.Region}, Creating={connectArgs.Creating}, Reconnect={connectArgs.Reconnecting})");
            }

            if (setupArgs.AuthValues == null) {
                return ConnectResult.Fail(ConnectFailReason.NoAuthValues, "AuthValues is null");
            }

            if (setupArgs.RuntimePlayer == null) {
                return ConnectResult.Fail(ConnectFailReason.NoRuntimePlayer, "RuntimePlayer is null");
            }

            if (setupArgs.RuntimeConfig == null) {
                return ConnectResult.Fail(ConnectFailReason.NoRuntimeConfig, "RuntimeConfig is null");
            }

            var serverSettings = PhotonServerSettings.Global;

            if (string.IsNullOrEmpty(serverSettings.AppSettings.AppIdQuantum)) {
                return ConnectResult.Fail(ConnectFailReason.NoAppId, "AppId is missing");
            }

            if (this.connectCancellation != null) {
                throw new Exception("Connection instance still in use");
            }

            if (connectArgs.Reconnecting == false) {
                if (Debug.unityLogger.IsLogTypeAllowed(LogType.Log)) {
                    Debug.Log($"[{nameof(PhotonService)}]: ConnectAndJoinGameAsync :: QuantumReconnectInformation.Reset(), due to Reconnecting=false");
                }

                QuantumReconnectInformation.Reset();
            }

            // CONNECT ---------------------------------------------------------------

            // Cancellation is used to stop the connection process at any time.
            this.connectCancellation       = new CancellationTokenSource();
            this.connectLinkedCancellation = AsyncSetup.CreateLinkedSource(this.connectCancellation.Token);

            var arguments = new MatchmakingArguments {
                PhotonSettings = new AppSettings(serverSettings.AppSettings) {
                    AppVersion  = setupArgs.AppVersion ?? serverSettings.AppSettings.AppVersion,
                    FixedRegion = connectArgs.Region,
                },
                CustomProperties      = setupArgs.CustomRoomProperties,
                CustomLobbyProperties = setupArgs.CustomLobbyProperties,
                ReconnectInformation  = QuantumReconnectInformation.Load(),
                EmptyRoomTtlInSeconds = serverSettings.EmptyRoomTtlInSeconds,
                PlayerTtlInSeconds    = serverSettings.PlayerTtlInSeconds,
                MaxPlayers            = setupArgs.MaxPlayerCount,
                RoomName              = connectArgs.Session,
                CanOnlyJoin           = string.IsNullOrEmpty(connectArgs.Session) == false && !connectArgs.Creating,
                PluginName            = "QuantumPlugin",
                AsyncConfig = new AsyncConfig {
                    TaskFactory       = AsyncConfig.CreateUnityTaskFactory(),
                    CancellationToken = this.connectLinkedCancellation.Token,
                },
                NetworkClient = this.Client,
                AuthValues    = setupArgs.AuthValues,
            };

            // Connect to Photon
            try {
                void OnClientStateChangedDuringConnecting(OnStateChanged m) {
                    progress.Report(GetProgressFromClientState(m.State));
                    if (GetProgress01FromClientState(m.State) is { } p01) {
                        progress01.Report(p01);
                    }
                }

                using (this.message.ListenManual<OnStateChanged>(OnClientStateChangedDuringConnecting)) {
                    if (connectArgs.Reconnecting == false) {
                        if (Debug.unityLogger.IsLogTypeAllowed(LogType.Log)) {
                            Debug.Log($"[{nameof(PhotonService)}]: ConnectAndJoinGameAsync :: MatchmakingExtensions.ConnectToRoomAsync()");
                        }

                        await MatchmakingExtensions.ConnectToRoomAsync(arguments);
                    }
                    else {
                        if (Debug.unityLogger.IsLogTypeAllowed(LogType.Log)) {
                            Debug.Log($"[{nameof(PhotonService)}]: ConnectAndJoinGameAsync :: MatchmakingExtensions.ReconnectToRoomAsync()");
                        }

                        await MatchmakingExtensions.ReconnectToRoomAsync(arguments);
                    }
                }
            }
            catch (Exception e) {
                Debug.LogException(e);
                return new ConnectResult {
                    FailReason     = ConnectFailReason.ConnectingFailed,
                    DebugMessage   = e.Message,
                    WaitForCleanup = this.CleanupAsync(),
                };
            }

            if (string.IsNullOrEmpty(this.Client.UserId)) {
                return ConnectResult.Fail(ConnectFailReason.UserIdRejectedByServer, "UserId returned by server is null", this.CleanupAsync());
            }

            // Save region summary
            if (!string.IsNullOrEmpty(this.Client.SummaryToCache)) {
                serverSettings.BestRegionSummary = this.Client.SummaryToCache;
            }

            if (Debug.unityLogger.IsLogTypeAllowed(LogType.Log)) {
                Debug.Log(
                    $"[{nameof(PhotonService)}]: ConnectAndJoinGameAsync :: JoinedGame={this.Client.CurrentRoom?.Name}, RoomProperties={this.Client.CurrentRoom?.CustomProperties.ToStringFull()}");
            }

            var disconnectCause = DisconnectCause.None;

            //  Make sure to notice socket disconnects during the rest of the connection/start process
            void OnDisconnectDuringConnecting(OnDisconnectedMsg m) {
                // ReSharper disable AccessToDisposedClosure
                if (this.connectCancellation is { IsCancellationRequested: false }) {
                    disconnectCause = m.cause;
                    this.connectCancellation.Cancel();
                }
                // ReSharper disable AccessToDisposedClosure
            }

            using (this.Client.CallbackMessage.ListenManual<OnDisconnectedMsg>(OnDisconnectDuringConnecting)) {
                // LOAD SCENE ---------------------------------------------------------------

                var preloadMap = false;
                if (setupArgs.RuntimeConfig != null
                    && setupArgs.RuntimeConfig.Map.Id.IsValid
                    && setupArgs.RuntimeConfig.SimulationConfig.Id.IsValid) {
                    if (QuantumUnityDB.TryGetGlobalAsset(setupArgs.RuntimeConfig.SimulationConfig, out var simulationConfigAsset)) {
                        // Only preload the scene if SimulationConfig.AutoLoadSceneFromMap is not enabled.
                        // Caveat: preloading the scene here only works if the runtime config is not expected to change (e.g. by other clients/random matchmaking or webhooks)
                        preloadMap = simulationConfigAsset.AutoLoadSceneFromMap == SimulationConfig.AutoLoadSceneFromMapMode.Disabled;
                    }
                }

                if (preloadMap == false) {
                    Debug.LogError($"[{nameof(PhotonService)}]: SimulationConfig is configured for auto scene load, required SimulationConfig.AutoLoadSceneFromMapMode.Disabled");
                }

                if (preloadMap) {
                    progress.Report("LOADING_SCENE");

                    if (!QuantumUnityDB.TryGetGlobalAsset(setupArgs.RuntimeConfig.Map, out var map)) {
                        return new ConnectResult {
                            FailReason     = ConnectFailReason.MapNotFound,
                            DebugMessage   = $"Requested map '{setupArgs.RuntimeConfig.Map}' not found.",
                            WaitForCleanup = this.CleanupAsync(),
                        };
                    }

                    using (new ConnectionServiceScope(this.Client)) {
                        try {
                            if (Debug.unityLogger.IsLogTypeAllowed(LogType.Log)) {
                                Debug.Log($"[{nameof(PhotonService)}]: ConnectAndJoinGameAsync :: SceneManager.LoadSceneAsync()");
                            }

                            // Load Unity scene async
                            await SceneManager.LoadSceneAsync(map.Scene, LoadSceneMode.Single).ToUniTask(progress01);
                            SceneManager.SetActiveScene(SceneManager.GetSceneByName(map.Scene));

                            // Check if cancellation was triggered while loading the map
                            if (this.connectLinkedCancellation.Token.IsCancellationRequested) {
                                throw new TaskCanceledException();
                            }
                        }
                        catch (Exception e) {
                            Debug.LogException(e);
                            return new ConnectResult {
                                FailReason      = TryDetermineFailReason(disconnectCause) ?? ConnectFailReason.LoadingFailed,
                                DisconnectCause = disconnectCause,
                                DebugMessage    = e.Message,
                                WaitForCleanup  = this.CleanupAsync(),
                            };
                        }

                        SceneManager.SetActiveScene(SceneManager.GetSceneByName(map.Scene));
                    }
                }

                // WAIT FOR START -----------------------------------------------------------

                using (new ConnectionServiceScope(this.Client)) {
                    const int estimateWaitTime     = 20;
                    var       remainingWaitSeconds = estimateWaitTime;

                    while (this.IsCurrentRoomQuantumStartAllowed == false) {
                        var currentPlayers = this.Client.CurrentRoom?.PlayerCount ?? 0;
                        var maxPlayers     = this.Client.CurrentRoom?.MaxPlayers ?? 0;

                        progress.Report("wait_for_players");
                        parameters.Report($"{currentPlayers}/{maxPlayers}) ({remainingWaitSeconds}");
                        progress01.Report(1f - (1f * remainingWaitSeconds / estimateWaitTime));

                        if (this.Client.LocalPlayer.IsMasterClient && currentPlayers >= maxPlayers) {
                            this.Client.CurrentRoom?.SetCustomProperties(new PhotonHashtable {
                                [PhotonConstants.RoomProperties.START_QUANTUM] = true,
                            });
                        }

                        await UniTask.Delay(TimeSpan.FromSeconds(1));

                        --remainingWaitSeconds;
                    }
                }

                // START GAME ---------------------------------------------------------------

                progress.Report("STARTING");

                // Register to plugin disconnect messages to display errors
                string pluginDisconnectReason = null;
                using (QuantumCallback.SubscribeManual<CallbackPluginDisconnect>(m => pluginDisconnectReason = m.Reason)) {
                    try {
                        await this.StartGame(setupArgs,
                            mode: DeterministicGameMode.Multiplayer,
                            deterministicGuidSource: this.Client.CurrentRoom!.Name,
                            cancellationToken: this.connectLinkedCancellation.Token,
                            isReconnecting: connectArgs.Reconnecting
                        );
                    }
                    catch (Exception e) {
                        Debug.LogException(e);
                        return new ConnectResult {
                            FailReason      = TryDetermineFailReason(disconnectCause, pluginDisconnectReason) ?? ConnectFailReason.RunnerFailed,
                            DisconnectCause = disconnectCause,
                            DebugMessage    = pluginDisconnectReason ?? e.Message,
                            WaitForCleanup  = this.CleanupAsync(),
                        };
                    }
                }
            }

            this.connectCancellation.Dispose();
            this.connectCancellation = null;
            this.connectLinkedCancellation.Dispose();
            this.connectLinkedCancellation = null;

            if (this.Runner == null) {
                return new ConnectResult {
                    FailReason     = ConnectFailReason.RunnerFailed,
                    DebugMessage   = "Runner is null",
                    WaitForCleanup = this.CleanupAsync(),
                };
            }

            if (Debug.unityLogger.IsLogTypeAllowed(LogType.Log)) {
                Debug.Log($"[{nameof(PhotonService)}]: ConnectAndJoinGameAsync succeeded");
            }

            this.RefreshReconnectInfo(saveFrameSnapshot: false);

            return new ConnectResult { Success = true };
        }

        [PublicAPI]
        public async Task StartGame(PhotonGameSetupArgs setupArgs, DeterministicGameMode mode, string deterministicGuidSource,
            CancellationToken cancellationToken, bool isReconnecting = false) {
            PatchArgs(setupArgs);

            setupArgs.RuntimeConfig.DeterministicGuidNamespace = DeterministicGuid.Create(
                namespaceId: new Guid("0914bbc9-44f3-46d1-853d-e115fec5d5a5"),
                name: deterministicGuidSource).ToString();

            var snapshotFrameNumber = 0;
            var snapshotFrameData   = default(byte[]);
            var useSnapshot         = isReconnecting && this.TryLoadSnapshotForReconnect(out snapshotFrameNumber, out snapshotFrameData);

            var sessionRunnerArguments = new SessionRunner.Arguments {
                RunnerFactory             = QuantumRunnerUnityFactory.DefaultFactory,
                GameParameters            = QuantumRunnerUnityFactory.CreateGameParameters,
                ClientId                  = this.Client.UserId,
                RuntimeConfig             = setupArgs.RuntimeConfig,
                SessionConfig             = QuantumDeterministicSessionConfigAsset.DefaultConfig,
                GameMode                  = mode,
                PlayerCount               = setupArgs.MaxPlayerCount,
                Communicator              = mode == DeterministicGameMode.Multiplayer ? new QuantumNetworkCommunicator(this.Client) : null,
                CancellationToken         = cancellationToken,
                RecordingFlags            = setupArgs.RecordingFlags,
                DeltaTimeType             = setupArgs.DeltaTimeType,
                StartGameTimeoutInSeconds = setupArgs.StartGameTimeoutInSeconds,
                GameFlags                 = setupArgs.GameFlags,
                OnShutdown                = this.OnSessionRunnerShutdown,

                InitialTick = useSnapshot ? snapshotFrameNumber : 0,
                FrameData   = useSnapshot ? snapshotFrameData : null,
            };

            if (Debug.unityLogger.IsLogTypeAllowed(LogType.Log)) {
                Debug.Log($"[{nameof(PhotonService)}]: ConnectAndJoinGameAsync :: SessionRunner.StartAsync()");
            }

            // Start Quantum and wait for the start protocol to complete
            this.Runner = (QuantumRunner)await SessionRunner.StartAsync(sessionRunnerArguments);

            if (this.Runner != null) {
                this.Runner.Game.AddPlayer(setupArgs.RuntimePlayer);
            }

            GC.Collect();
        }

        [PublicAPI]
        public void CancelConnect() {
            if (Debug.unityLogger.IsLogTypeAllowed(LogType.Log)) {
                Debug.Log($"[{nameof(PhotonService)}]: CancelConnect()");
            }

            this.connectCancellation?.Cancel();
        }

        [PublicAPI]
        public async Task DisconnectAsync(ConnectFailReason reason) {
            if (Debug.unityLogger.IsLogTypeAllowed(LogType.Log)) {
                Debug.Log($"[{nameof(PhotonService)}]: DisconnectAsync({reason})");
            }

            if (this.connectCancellation != null) {
                this.CancelConnect();
                return;
            }

            if (reason == ConnectFailReason.UserRequest) {
                QuantumReconnectInformation.Reset();
                
                // Explicitly leave the room to remove inactive player state
                // This prevents "Found inactive UserId" error when rejoining matchmaking
                if (this.Client != null && this.Client.InRoom) {
                    try {
                        if (Debug.unityLogger.IsLogTypeAllowed(LogType.Log)) {
                            Debug.Log($"[{nameof(PhotonService)}]: Leaving room explicitly to prevent inactive state");
                        }
                        this.Client.OpLeaveRoom(becomeInactive: false);
                        // Give a small delay to allow the leave operation to complete
                        await UniTask.Delay(TimeSpan.FromMilliseconds(100));
                    }
                    catch (Exception e) {
                        Debug.LogException(e);
                    }
                }
            }

            // Stop the running game
            await this.CleanupAsync();
        }

        [PublicAPI]
        public async Task<List<PhotonOnlineRegion>> RequestAvailableOnlineRegionsAsync() {
            if (Debug.unityLogger.IsLogTypeAllowed(LogType.Log)) {
                Debug.Log($"[{nameof(PhotonService)}]: RequestAvailableOnlineRegionsAsync()");
            }

            var appSettings = PhotonServerSettings.Global.AppSettings;

            if (string.IsNullOrEmpty(appSettings.AppIdQuantum)) {
                throw new InvalidOperationException("AppId is missing");
            }

            var regions = await this.Client.ConnectToNameserverAndWaitForRegionsAsync(appSettings);

            return regions.EnabledRegions
                .Select(r => new PhotonOnlineRegion { Code = r.Code, Ping = r.Ping })
                .ToList();
        }

        private static void PatchArgs(PhotonGameSetupArgs setupArgs) {
            // limit player count
            setupArgs.MaxPlayerCount = Math.Min(setupArgs.MaxPlayerCount, Input.MaxCount);

            // runtime config alterations
            {
                // always re roll the seed if 0.
                if (setupArgs.RuntimeConfig.Seed == 0) {
                    setupArgs.RuntimeConfig.Seed = Guid.NewGuid().GetHashCode();
                }

                // if SimulationConfig not set, try to get from global default configs
                if (setupArgs.RuntimeConfig.SimulationConfig.Id.IsValid == false && QuantumDefaultConfigs.TryGetGlobal(out var defaultConfigs)) {
                    setupArgs.RuntimeConfig.SimulationConfig = defaultConfigs.SimulationConfig;
                }

                // if SystemsConfig not set, try to get from global default configs
                if (setupArgs.RuntimeConfig.SystemsConfig.Id.IsValid == false && QuantumDefaultConfigs.TryGetGlobal(out var defaultSystems)) {
                    setupArgs.RuntimeConfig.SystemsConfig = defaultSystems.SystemsConfig;
                }
            }
        }

        /// <summary>
        /// Match errors to one error number.
        /// </summary>
        /// <param name="disconnectCause">Photon disconnect reason</param>
        /// <param name="pluginDisconnectReason">Plugin disconnect message</param>
        /// <returns></returns>
        public static ConnectFailReason? TryDetermineFailReason(DisconnectCause disconnectCause, string pluginDisconnectReason = null) {
            if (AsyncConfig.Global.IsCancellationRequested) {
                return ConnectFailReason.ApplicationQuit;
            }

            switch (disconnectCause) {
                case DisconnectCause.DisconnectByClientLogic:
                    if (string.IsNullOrEmpty(pluginDisconnectReason) == false) {
                        return ConnectFailReason.PluginError;
                    }

                    return ConnectFailReason.Disconnect;
            }

            return null;
        }

        private async Task CleanupAsync() {
            if (Debug.unityLogger.IsLogTypeAllowed(LogType.Log)) {
                Debug.Log($"[{nameof(PhotonService)}]: CleanupAsync()");
            }

            this.connectCancellation?.Dispose();
            this.connectCancellation = null;
            this.connectLinkedCancellation?.Dispose();
            this.connectLinkedCancellation = null;

            if (this.Runner != null) {
                var oldRunner = this.Runner;
                this.Runner = null;

                try {
                    if (Debug.unityLogger.IsLogTypeAllowed(LogType.Log)) {
                        Debug.Log($"[{nameof(PhotonService)}]: CleanupAsync :: Runner.ShutdownAsync()");
                    }

                    if (oldRunner.State != SessionRunner.SessionState.ShuttingDown && 
                        oldRunner.State != SessionRunner.SessionState.Shutdown) {
                        if (AsyncConfig.Global.IsCancellationRequested) {
                            oldRunner.Shutdown();
                        }
                        else {
                            await oldRunner.ShutdownAsync();
                        }
                    }
                }
                catch (Exception e) {
                    Debug.LogException(e);
                }
            }

            if (this.Client != null) {
                try {
                    if (Debug.unityLogger.IsLogTypeAllowed(LogType.Log)) {
                        Debug.Log($"[{nameof(PhotonService)}]: CleanupAsync :: Client.DisconnectAsync()");
                    }

                    if (AsyncConfig.Global.IsCancellationRequested) {
                        this.Client.Disconnect();
                    }
                    else {
                        await this.Client.DisconnectAsync();
                    }
                }
                catch (Exception e) {
                    Debug.LogException(e);
                }
            }
            
            GC.Collect();
        }

        /// <summary>
        /// React to plugin disconnects that are received by the protocol.
        /// </summary>
        /// <param name="callback"></param>
        private void OnPluginDisconnect(CallbackPluginDisconnect callback) {
            // Debug.LogError($"[{nameof(PhotonService)}]: OnPluginDisconnect, Reason = {callback.Reason}");

            // App.RequestAppUpdateFlow(new PhotonAppUpdateFlowArgs.PhotonUnexpectedDisconnect {
            //     Reason = callback.Reason,
            // });
        }

        /// <summary>
        /// Connection signaled disconnect, stopping the menu connection object.
        /// If enabled trying to perform a reconnection
        /// </summary>
        /// <param name="cause">Photon disconnect cause</param>
        private void OnDisconnect(DisconnectCause cause) {
            if (cause is DisconnectCause.ApplicationQuit or DisconnectCause.DisconnectByClientLogic) {
                Debug.Log($"[{nameof(PhotonService)}]: OnDisconnect, Reason = {cause}");
            }
            else {
                Debug.LogError($"[{nameof(PhotonService)}]: OnDisconnect, Reason = {cause}");
            }

            if (cause == DisconnectCause.DisconnectByClientLogic) {
                // Only handle disruption not caused by the user
                return;
            }

            if (cause == DisconnectCause.ExceptionOnConnect && this.connectCancellation != null) {
                // Disconnect will be handled by ConnectAsync()
                return;
            }

            this.UnexpectedlyDisconnected?.Invoke(cause.ToString());
        }

        /// <summary>
        /// Notification when the <see cref="SessionRunner"/> terminated. Can be used to handle errors.
        /// </summary>
        /// <param name="cause">Shutdown cause.</param>
        /// <param name="runner">Session runner object.</param>
        private void OnSessionRunnerShutdown(ShutdownCause cause, SessionRunner runner) {
            if (cause == ShutdownCause.Ok) {
                return;
            }

            Debug.LogError($"[{nameof(PhotonService)}]: OnSessionRunnerShutdown, Reason = {cause}");

            this.UnexpectedlyDisconnected?.Invoke(cause.ToString());
        }

        private void OnErrorInfo(OnErrorInfoMsg errorInfoMsg) {
            Debug.LogError($"[{nameof(PhotonService)}]: OnErrorInfo, Error = {errorInfoMsg.errorInfo}");
        }

        private void OnClientStateChanged(ClientState oldState, ClientState newState) {
            this.message.Raise(new OnStateChanged { State = newState });
        }

        private static string GetProgressFromClientState(ClientState state) {
            return state switch {
                ClientState.PeerCreated => "PEER_CREATED",
                ClientState.Authenticating => "AUTHENTICATING",
                ClientState.Authenticated => "AUTHENTICATED",
                ClientState.JoiningLobby => "JOINING_LOBBY",
                ClientState.JoinedLobby => "JOINED_LOBBY",
                ClientState.DisconnectingFromMasterServer => "DISCONNECTING_FROM_MASTER_SERVER",
                ClientState.ConnectingToGameServer => "CONNECTING_TO_GAME_SERVER",
                ClientState.ConnectedToGameServer => "CONNECTED_TO_GAME_SERVER",
                ClientState.Joining => "JOINING",
                ClientState.Joined => "JOINED",
                ClientState.Leaving => "LEAVING",
                ClientState.DisconnectingFromGameServer => "DISCONNECTING_FROM_GAME_SERVER",
                ClientState.ConnectingToMasterServer => "CONNECTING_TO_MASTER_SERVER",
                ClientState.Disconnecting => "DISCONNECTING",
                ClientState.Disconnected => "DISCONNECTED",
                ClientState.ConnectedToMasterServer => "CONNECTED_TO_MASTER_SERVER",
                ClientState.ConnectingToNameServer => "CONNECTING_TO_NAME_SERVER",
                ClientState.ConnectedToNameServer => "CONNECTED_TO_NAME_SERVER",
                ClientState.DisconnectingFromNameServer => "DISCONNECTING_FROM_NAME_SERVER",
                ClientState.ConnectWithFallbackProtocol => "CONNECT_WITH_FALLBACK_PROTOCOL",
                _ => "UNKNOWN_PEER_STATE",
            };
        }

        private static float? GetProgress01FromClientState(ClientState state) {
            return state switch {
                ClientState.ConnectingToNameServer => 0.10f,
                ClientState.ConnectedToNameServer => 0.20f,
                ClientState.ConnectingToMasterServer => 0.30f,
                ClientState.ConnectedToMasterServer => 0.40f,
                ClientState.ConnectingToGameServer => 0.50f,
                ClientState.ConnectedToGameServer => 0.60f,
                ClientState.Joining => 0.70f,
                ClientState.Joined => 0.80f,
                _ => null,
            };
        }

        private class OnStateChanged {
            public ClientState State;
        }
    }
}