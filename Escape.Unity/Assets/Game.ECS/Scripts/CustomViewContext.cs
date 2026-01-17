namespace _Project.Scripts.GameView {
    using JetBrains.Annotations;
    using Quantum;
    using UnityEngine;

    public class CustomViewContext : MonoBehaviour, IQuantumViewContext {
        public QuantumEntityViewUpdater QuantumEntityViewUpdater;

        [CanBeNull] public CharacterView LocalView;
    }
}