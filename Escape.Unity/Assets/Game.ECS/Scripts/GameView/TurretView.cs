namespace _Project.Scripts {
    using Quantum;
    using UnityEngine;
    public class TurretView : QuantumEntityViewComponent {
        [SerializeField] GameObject _rotationExclude;

        public override void OnActivate(Frame frame) {
            _rotationExclude.transform.SetParent(null);
        }
        public override void OnDeactivate() {
            Destroy(_rotationExclude);
        }
    }
}