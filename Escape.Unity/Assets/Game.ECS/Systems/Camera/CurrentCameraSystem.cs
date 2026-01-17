namespace Game.ECS.Systems.Camera {
    using Components.Camera;
    using Multicast;
    using Scellecs.Morpeh;
    using Unity.Cinemachine;
    using UnityEngine;

    public class CurrentCameraSystem : SystemBase {
        [Inject] private Stash<CinemachineVirtualCameraComponent> camComponent;

        private Filter cameraFilter;
        private SingletonFilter<CinemachineBrainComponent> brainFilter;

        public override void OnAwake() {
            this.cameraFilter = this.World.Filter
                .With<CinemachineVirtualCameraComponent>()
                .Build();

            this.brainFilter = this.World.Filter.Singleton<CinemachineBrainComponent>();
        }

        public override void OnUpdate(float deltaTime) {
        }

        public bool TryGetCurrentCameraEntity(out Entity cameraEntity) {
            cameraEntity = null;

            ref var brainComponent = ref this.brainFilter.Instance;

            var activeCam = brainComponent.brain.ActiveVirtualCamera as CinemachineCamera;
            if (activeCam == null) {
                return false;
            }

            foreach (var entity in this.cameraFilter) {
                ref var cam = ref this.camComponent.Get(entity);
                if (cam.camera == activeCam) {
                    cameraEntity = entity;
                    return true;
                }
            }

            return false;
        }

        public bool TryGetCurrentCinemachineCamera(out CinemachineCamera vCam) {
            vCam = null;

            if (!this.TryGetCurrentCameraEntity(out var cameraEntity)) {
                return false;
            }

            ref var cam = ref this.camComponent.Get(cameraEntity);

            vCam = cam.camera;
            return true;
        }

        public bool TryGetCurrentCameraExtension<T>(out T extension) where T : Component {
            extension = null;

            if (!this.TryGetCurrentCameraEntity(out var cameraEntity)) {
                return false;
            }

            ref var cam = ref this.camComponent.Get(cameraEntity);

            if (!cam.extensionsMap.TryGetValue(typeof(T), out var extensionUntyped)) {
                return false;
            }

            if (extensionUntyped is not T extensionTypes) {
                return false;
            }

            extension = extensionTypes;
            return true;
        }
    }
}