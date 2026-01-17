namespace Game.UI.Views {
    using Multicast;
    using Sirenix.OdinInspector;
    using UniMob.UI;
    using UnityEngine;
    using static UnityEngine.Mathf;

    public class VignetteView : AutoView<IVignetteViewState> {
        [Required]
        [SerializeField] private CanvasGroup vignette;

        [Range(0, 100)]
        [SerializeField] private float hpCriticalPercent = 20f;

        [Range(0, 1)]
        [SerializeField] private float alphaAtThreshold;

        [SerializeField] private AnimationCurve curve = AnimationCurve.Linear(0, 0, 1, 1);

        [Button]
        protected override void Render() {
            base.Render();
            var normalized = State.Health / State.MaxHealth;
            var threshold  = hpCriticalPercent * 0.01f;

            if (normalized > threshold) {
                vignette.alpha = 0f;
                return;
            }

            var t = InverseLerp(threshold, 0f, normalized);
            vignette.alpha = Lerp(alphaAtThreshold, 1f, curve.Evaluate(t));
        }
    }

    public interface IVignetteViewState : IViewState {
        float Health    { get; }
        float MaxHealth { get; }
    }

}