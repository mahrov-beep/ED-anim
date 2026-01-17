namespace Game.UI.Controllers.Photon {
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Cysharp.Threading.Tasks;
    using Domain.GameModes;
    using Domain.Party;
    using global::Photon.Client;
    using global::Photon.Realtime;
    using Multicast;
    using Multicast.Analytics;
    using Quantum;
    using Services.Photon;
    using Shared.UserProfile.Data;
    using UniMob;
    using UnityEngine.Serialization;

    [Serializable, RequireFieldsInit]
    public struct PhotonJoinGameControllerArgs : IResultControllerArgs<ConnectResult> {
        public PhotonGameConnectArgs connectionArgs;
        public string                gameModeKeyOverride;
        public int?                  maxPlayersOverride;
    }

    public class PhotonJoinGameController : ResultController<PhotonJoinGameControllerArgs, ConnectResult> {
        [Inject] private PhotonService  photonService;
        [Inject] private GameModesModel gameModesModel;
        [Inject] private SdUserProfile  sdUserProfile;
        [Inject] private PartyModel     partyModel;

        protected override async UniTask<ConnectResult> Execute(Context context) {
            var progressAtom   = Atom.Value(0f);
            var messageAtom    = Atom.Value(string.Empty);
            var parametersAtom = Atom.Value(string.Empty);

            //void OnCancel() => this.photonService.CancelConnect();

            var gameModeModel = this.gameModesModel.SelectedGameMode;
            if (!string.IsNullOrEmpty(this.Args.gameModeKeyOverride)) {
                var overrideModel = this.gameModesModel.VisibleGameModes.FirstOrDefault(m => string.Equals(m.Def.key, this.Args.gameModeKeyOverride, StringComparison.OrdinalIgnoreCase));
                if (overrideModel != null) {
                    gameModeModel = overrideModel;
                }
            }
            var maxPlayers = this.Args.maxPlayersOverride ?? gameModeModel.ModeQuantumAsset.maxPlayers;

            var setupArgs = new PhotonGameSetupArgs {
                // Используем окружение (Environment) для того
                // чтобы сессии из редактора не пересекались с настоящими игроками
                // В релизе Env=prod/staging, в редакторе можно задать
                // в текстовом поле в верхнем правом меню редактора
                /*AppVersion = PhotonService.HasDevAppVersion
                    ? PhotonService.DevAppVersionPref
                    // если в бой зайдут игроки с разных версий билда, то высока вероятность
                    // что там разные сборки кода квантума и игра упадет с frameDiff error,
                    // так что создаем отдельное лобби на каждый билд игры
                    : $"{App.ServerAccessTokenInfo.Environment}-{Application.buildGUID}",*/

                // в релизе нужно будет добавить авторизацию через сервер,
                // а пока просто передаем айдишник
                AuthValues = new AuthenticationValues(App.ServerAccessTokenInfo.UserId.ToString()),

                MaxPlayerCount = maxPlayers,

                // здесь можно указать параметры для запуска игры.
                // в релизе эти параметры будет валидировать сервер (через photon webhook)
                RuntimeConfig = new RuntimeConfig {
                    GameModeAsset    = gameModeModel.ModeQuantumAsset,
                    Map              = gameModeModel.ModeQuantumAsset.map,
                    SystemsConfig    = gameModeModel.ModeQuantumAsset.systemsConfig,
                    SimulationConfig = gameModeModel.ModeQuantumAsset.simulationConfig,
                },

                // здесь можно указать параметры для запуска игры.
                // в релизе эти параметры будет валидировать сервер (через photon webhook)
                RuntimePlayer = new RuntimePlayer {
                    NickName      = this.sdUserProfile.NickName.Value,
                    Loadout       = this.sdUserProfile.Loadouts.GetSelectedLoadoutClone(),
                    StorageWidth  = this.sdUserProfile.StorageWidth.Value,
                    StorageHeight = this.sdUserProfile.StorageHeight.Value,
                    PartyKeyHash  = ComputePartyKeyHash(),
                },

                // можно передавать только параметры которые помечены как "разрешённые"
                // в админке (на сайте) фотона
                // пока это только "mode", но в теории для конфиругигрования игры достаточно этого параметра,
                // а все остальные данные можно получить из GameModes DDE.
                // Если нужно хранить какие-то временные значения,
                // то для того можно использовать компоненты в Quantum ECS
                CustomRoomProperties = new PhotonHashtable {
                    [PhotonConstants.RoomProperties.GAME_MODE]     = gameModeModel.Key,
                    [PhotonConstants.RoomProperties.START_QUANTUM] = false,
                },
                CustomLobbyProperties = new[] {
                    PhotonConstants.RoomProperties.GAME_MODE,
                    PhotonConstants.RoomProperties.START_QUANTUM,
                },

                DeltaTimeType = SimulationUpdateTime.Default,
            };

            ConnectResult result;

            bool retrying = false;

            do {
                await using (await context.RunProgressScreenDisposable(progressAtom, messageAtom, parametersAtom, useSystemNavigator: true)) {
                    result = await this.photonService.ConnectAndJoinGameAsync(setupArgs, this.Args.connectionArgs,
                        new Progress<string>(p => messageAtom.Value = p),
                        new Progress<string>(p => parametersAtom.Value = p),
                        new Progress<float>(p => progressAtom.Value = p)
                    );
                }

                if (!result.Success) {
                    // do not retry if room not exist
                    if (result.DebugMessage == "Failed to join the room with error '32758'") {
                        return result;
                    }

                    App.Analytics.Send("photon_connect_failed", new AnalyticsArg("reason", result.FailReason.ToString()) {
                        new AnalyticsArg("cause", result.DisconnectCause.ToString()) {
                            new AnalyticsArg("message", result.DebugMessage),
                        },
                    });

                    if (result.FailReason != ConnectFailReason.ApplicationQuit) {

                        var alertPhotonFailedToConnectToServer = context
                            .GetNavigator(AppNavigatorType.System)
                            .AlertPhotonFailedToConnectToServer($"{result.FailReason} :: {result.DisconnectCause} :: {result.DebugMessage}");

                        retrying = (await Task.WhenAll(alertPhotonFailedToConnectToServer, WaitForResultCleanup()))[0];
                    }
                }
            }
            while (retrying);

            return result;

            async Task<bool> WaitForResultCleanup() {
                // ReSharper disable once AccessToModifiedClosure
                await (result.WaitForCleanup ?? Task.CompletedTask);
                return false;
            }
        }

        private ulong ComputePartyKeyHash() {
            var leader = this.partyModel.Leader.Value;
            var source = leader != Guid.Empty ? leader.ToString("N") : App.ServerAccessTokenInfo.UserId.ToString("N");

            return Fnv1a64(source);
        }

        private static ulong Fnv1a64(string s) {
            const ulong offset = 1469598103934665603UL;
            const ulong prime  = 1099511628211UL;
            ulong       hash   = offset;
            for (int i = 0; i < s.Length; i++) {
                hash ^= s[i];
                hash *= prime;
            }
            return hash;
        }
    }
}