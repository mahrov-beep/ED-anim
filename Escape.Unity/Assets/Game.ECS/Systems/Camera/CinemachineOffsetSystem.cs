namespace Game.ECS.Systems.Camera {
    using Components.Camera;
    using Components.Unit;
    using Core;
    using Multicast;
    using Player;
    using Quantum;
    using Scellecs.Morpeh;
    using Services.Photon;
    using UnityEngine;
    using SystemBase = Scellecs.Morpeh.SystemBase;

    public class CinemachineOffsetSystem : SystemBase {
        [Inject] private PhotonService           photonService;
        [Inject] private LocalPlayerSystem       localPlayerSystem;
        [Inject] private QuantumEntityViewSystem quantumEntityViewSystem;

        [Inject] private Stash<CinemachineCameraOffsetComponent> cameraOffsetComponent;
        [Inject] private Stash<UnitComponent>                    unitComponent;

        private Filter cameraFilter;

        public override void OnAwake() {
            this.cameraFilter = this.World.Filter
                .With<CinemachineVirtualCameraComponent>()
                .With<CinemachineCameraOffsetComponent>()
                .Build();
        }

        public override unsafe void OnUpdate(float deltaTime) {
            if (this.photonService.PredictedFrame is not { } f) {
                return;
            }

            if (localPlayerSystem.HasNotLocalEntityRef(out var localRef)) {
                return;
            }

            if (!f.TryGetPointer(localRef, out Unit* unit)) {
                return;
            }

            foreach (var entity in this.cameraFilter) {
                ref var cameraOffset = ref this.cameraOffsetComponent.Get(entity);

                var targetVector = unit->CurrentSpeed.AsFloat > 0 ? cameraOffset.movementOffset : cameraOffset.idleOffset;

                EAttributeType.PercentBoost_CameraOffset
                    .UNSAFE_ApplyPercentMultiplierOn(ref targetVector.z, f, localRef);

                var weaponConfig = unit->GetActiveWeaponConfig(f);
                if (weaponConfig != null) {
                    var weaponOffsetZ = weaponConfig.cameraOffsetZ.AsFloat;

                    EAttributeType.PercentBoost_WeaponCameraOffset
                        .UNSAFE_ApplyPercentMultiplierOn(ref weaponOffsetZ, f, localRef);

                    targetVector.z -= weaponOffsetZ;
                }

                cameraOffset.cameraOffset.Offset = Vector3.Lerp(
                    cameraOffset.cameraOffset.Offset,
                    targetVector,
                    Time.deltaTime * cameraOffset.speed);
            }
        }
    }
}