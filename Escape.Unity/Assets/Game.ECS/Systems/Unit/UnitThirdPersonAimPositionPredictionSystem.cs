namespace Game.ECS.Systems.Unit {
    using System;
    using Camera;
    using Core;
    using Multicast;
    using Player;
    using Quantum;
    using Services.Photon;
    using Unity.Cinemachine;
    using UnityEngine;
    using Object = UnityEngine.Object;
    using SystemBase = Scellecs.Morpeh.SystemBase;

    public class UnitThirdPersonAimPositionPredictionSystem : SystemBase {
        [Inject] private PhotonService           photonService;
        [Inject] private LocalPlayerSystem       localPlayerSystem;
        [Inject] private QuantumEntityViewSystem quantumEntityViewSystem;
        [Inject] private CurrentCameraSystem     currentCameraSystem;
        
        [NonSerialized] private EntityRef? targetEntity;
        [NonSerialized] private Vector3    damping;
        [NonSerialized] private Vector3    dampingVelocity;

        public override void OnAwake() {
        }

        public override void OnUpdate(float deltaTime) {
            if (this.photonService.PredictedFrame is not { } f) {
                return;
            }

            if (f.GameModeAiming is not ThirdPersonAimingAsset aiming) {
                return;
            }

            if (localPlayerSystem.HasNotLocalEntityRef(out var localRef)) {
                return;
            }

            if (!f.TryGet(localRef, out Unit unit)) {
                return;
            }

            if (!f.TryGet(localRef, out UnitAim unitAim)) {
                return;
            }

            if (!this.quantumEntityViewSystem.TryGetEntityView(unitAim.AimEntity, out var aimEntityView)) {
                return;
            }

            if (!this.currentCameraSystem.TryGetCurrentCameraExtension(out CinemachineThirdPersonAimQuantum thirdPersonAimQuantum)) {
                return;
            }

            aimEntityView.ViewFlags |= QuantumEntityViewFlags.DisableUpdatePosition;

            var forwardAimTargetPos = thirdPersonAimQuantum.AimTarget;

            Vector3 targetAimTargetPos;

            if (aiming.autoAim &&
                unit.HasTarget &&
                unit.GetActiveWeaponConfig(f) is { } weaponConfig &&
                this.quantumEntityViewSystem.TryGetEntityView(unit.Target, out var targetView)) {
                var targetPos = targetView.transform.position
                                + new Vector3(0, weaponConfig.shotTargetOffsetY.AsFloat, 0);

                if (unit.Target != this.targetEntity) {
                    this.damping      = aimEntityView.transform.position - targetPos;
                    this.targetEntity = unit.Target;
                }

                targetAimTargetPos = targetPos;
            }
            else {
                if (this.targetEntity != EntityRef.None) {
                    this.damping      = aimEntityView.transform.position - forwardAimTargetPos;
                    this.targetEntity = EntityRef.None;
                }

                targetAimTargetPos = forwardAimTargetPos;
            }

            this.damping = Vector3.SmoothDamp(
                this.damping,
                Vector3.zero,
                ref this.dampingVelocity,
                0.05f
            );

            aimEntityView.transform.position = targetAimTargetPos + this.damping;
        }
    }
}