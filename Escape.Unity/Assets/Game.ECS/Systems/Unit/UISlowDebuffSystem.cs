namespace Game.ECS.Systems.Unit {
    using Core;
    using Player;
    using Game.Services.Photon;
    using Multicast;
    using Quantum;
    using UnityEngine;
    using SystemBase = Scellecs.Morpeh.SystemBase;

    public class UISlowDebuffSystem : SystemBase {
        [Inject] private PhotonService           photonService;
        [Inject] private LocalPlayerSystem       localPlayerSystem;
        [Inject] private QuantumEntityViewSystem entityViewUpdater;

        public SlowDebuff Debuff    { get; private set; }
        public Vector3    TargetPos { get; private set; }

        public override void OnAwake() { }

        public override unsafe void OnUpdate(float deltaTime) {

            Debuff = default;

            if (photonService.PredictedFrame is not { } f) {
                return;
            }

            if (localPlayerSystem.HasNotLocalEntityRef(out var localRef)) {
                return;
            }

            var unit = f.GetPointer<Unit>(localRef);
            if (!unit->HasTarget) {
                return;
            }

            var targetRef = unit->Target;

            var hasTargetView = entityViewUpdater.TryGetEntityView(targetRef,
                            out var targetView);

            if (!hasTargetView) {
                return;
            }

            Debuff    = f.Get<SlowDebuff>(targetRef);
            TargetPos = targetView.transform.position;
        }
    }
}