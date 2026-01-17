namespace Game.UI.Controllers.Photon {
    using System;
    using System.Threading;
    using Cysharp.Threading.Tasks;
    using Domain;
    using global::Photon.Deterministic;
    using Multicast;
    using Multicast.Utilities;
    using Quantum;
    using Services.Photon;
    using Shared.UserProfile.Data;
    using UnityEngine.ResourceManagement.ResourceProviders;
    using UnityEngine.SceneManagement;

    [RequireFieldsInit]
    public struct PhotonLocalSimulationControllerArgs : IDisposableControllerArgs {
        public string UnitySceneName;
        public string GameModeAssetPath;

        public GameSnapshotLoadout Loadout;
        public GameSnapshotStorage Storage;
    }

    public class PhotonLocalSimulationController : DisposableController<PhotonLocalSimulationControllerArgs> {
        [Inject] private PhotonService photonService;
        [Inject] private SdUserProfile userProfile;

        private SceneInstance? scene;

        protected override async UniTask Activate(Context context) {
            await using (await context.RunProgressScreenDisposable("starting_local_simulation")) {
                this.scene = await AddressablesUtils.LoadSceneAsync(this.Args.UnitySceneName, LoadSceneMode.Additive);

                var gameModeAsset = QuantumUnityDB.Global.GetRequiredAsset<GameModeAsset>(this.Args.GameModeAssetPath);

                var setupArgs = new PhotonGameSetupArgs {
                    AuthValues = null,
                    RuntimeConfig = new RuntimeConfig {
                        GameModeAsset    = gameModeAsset,
                        Map              = gameModeAsset.map,
                        SystemsConfig    = gameModeAsset.systemsConfig,
                        SimulationConfig = gameModeAsset.simulationConfig,
                    },
                    RuntimePlayer = new RuntimePlayer {
                        NickName      = this.userProfile.NickName.Value,
                        Loadout       = this.Args.Loadout.DeepClone(), // pass clone because Quantum modifies data
                        Storage       = this.Args.Storage,
                        StorageWidth  = this.userProfile.StorageWidth.Value,
                        StorageHeight = this.userProfile.StorageHeight.Value,
                    },
                    MaxPlayerCount        = 1,
                    CustomLobbyProperties = Array.Empty<string>(),
                    CustomRoomProperties  = null,
                };

                // start local simulation for game results
                await this.photonService.StartGame(setupArgs,
                    mode: DeterministicGameMode.Local,
                    deterministicGuidSource: Guid.NewGuid().ToString(),
                    cancellationToken: CancellationToken.None
                );
            }
        }

        protected override async UniTask OnDisposeAsync(Context context) {
            if (this.scene.HasValue) {
                await AddressablesUtils.UnloadSceneAsync(this.scene.Value);
            }

            await this.photonService.DisconnectAsync(ConnectFailReason.UserRequest);
        }
    }
}