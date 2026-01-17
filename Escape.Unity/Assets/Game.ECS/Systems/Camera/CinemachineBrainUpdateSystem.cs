namespace Game.ECS.Systems.Camera {
    using Components.Camera;
    using Multicast;
    using Quantum;
    using Scellecs.Morpeh;
    using Services.Photon;
    using SystemBase = Scellecs.Morpeh.SystemBase;

    public class CinemachineBrainUpdateSystem : SystemBase {
        [Inject] private Stash<CinemachineBrainComponent> brainStash;
        [Inject] private PhotonService                    photonService;

        [Inject] private CurrentCameraSystem currentCameraSystem;

        private Filter brainFilter;

        public override void OnAwake() {
            this.brainFilter = this.World.Filter
                .With<CinemachineBrainComponent>()
                .Build();
        }

        public override void OnUpdate(float deltaTime) {
            if (!photonService.TryGetPredicted(out var f)) {
                return;
            }

            foreach (var entity in brainFilter) {
                ref var brain = ref brainStash.Get(entity);

                brain.brain.ManualUpdate();

                if (!currentCameraSystem.TryGetCurrentCinemachineCamera(out var currentCamera)) {
                    continue;
                }

                brain.Transform = currentCamera.transform;
            }
        }
    }
}