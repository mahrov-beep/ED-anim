namespace Game.ECS.Providers.Unit {
    using Components.Unit;
    using Game.ECS.Scripts.GameView;
    using global::Quantum;
    using QuantumUser.Unity;
    using Scellecs.Morpeh.Providers;
    using UnityEngine;

    [RequireComponent(typeof(QuantumEntityView))]
    public class UnitProvider : MonoProvider<UnitComponent> {
        private void Reset() {
            ref var data = ref GetData();
            data.quantumEntityView = this.GetComponent<QuantumEntityView>();
            data.healthBarOffset   = Vector3.up * 2;
            data.AimAssistTarget   = GetComponentInChildren<AimAssistTargetPoint>(true).gameObject;
            data.reconOutline      = data.quantumEntityView ? data.quantumEntityView.GetComponent<ReconOutline>() : null;
        }
    }
}
