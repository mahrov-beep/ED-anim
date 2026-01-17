namespace Game.ECS.Systems.Camera {
    using Components.Camera;
    using Components.Unit;
    using Core;
    using Multicast;
    using Player;
    using Quantum;
    using Scellecs.Morpeh;
    using Services.Photon;
    using Unit;
    using Unity.Cinemachine;
    using SystemBase = Scellecs.Morpeh.SystemBase;

    public class CinemachineSetFollowSystem : SystemBase {
        [Inject] private PhotonService           photonService;
        [Inject] private QuantumEntityViewSystem quantumEntityViewSystem;
        [Inject] private LocalPlayerSystem       localPlayerSystem;
        [Inject] private CurrentCameraSystem     currentCameraSystem;

        [Inject] MapperUnitEntityRefToEntitySystem mapperUnitEntityRefToEntity;

        public override void OnAwake() { }

        public override void OnUpdate(float deltaTime) {
            if (!photonService.TryGetPredicted(out var f)) {
                return;
            }

            if (localPlayerSystem.HasNotLocalEntityRef(out var localRef)) {
                return;
            }

            if (!f.TryGet(localRef, out CharacterSpectatorCamera spectatorCamera)) {
                return;
            }

            if (!quantumEntityViewSystem.TryGetEntityView(spectatorCamera.CameraEntity, out var cameraEntityView)) {
                return;
            }

            if (!currentCameraSystem.TryGetCurrentCinemachineCamera(out var cam)) {
                return;
            }

            if (f.GameModeAiming is FirstPersonAimingAsset) {
                cam.Follow = null;

                return;
            }

            cam.Follow = cameraEntityView.transform;
        }
    }

}