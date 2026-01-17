namespace _Project.Scripts.GameView {
    using BrunoMikoski.AnimationSequencer;
    using Game.ECS.Scripts;
    using Game.ECS.Systems.GameInventory;
    using Quantum;
    using Sirenix.OdinInspector;
    using UnityEngine;

    /// <summary>
    /// Двери, сундуки, порталы
    /// </summary>
    public class OpenableViewComponent : QuantumEntityViewComponent, IOpenableView {

        [Required, SerializeField]
        private AnimationSequencerController open;

        [Required, SerializeField]
        private AnimationSequencerController close;

        public void Open(bool instant = false) {
            close.Kill();

            if (instant) {
                open.SetProgress(1, false);
            }
            else {
                open.Play();
            }
        }

        public void Close(bool instant = false) {
            open.Kill();

            if (instant) {
                close.SetProgress(1, false);
            }
            else {
                close.Play();
            }
        }
    }
}