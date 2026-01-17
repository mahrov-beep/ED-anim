namespace _Project.Scripts.GameView {
    using Quantum;
    using UnityEngine;
    using UnityEngine.UI;
    public class AbilityReloadingView : QuantumSceneViewComponent {
        [SerializeField] private Image                  image;

        public override unsafe void OnUpdateView() {
            var filter = PredictedFrame.Filter<Unit>();

            while (filter.NextUnsafe(out var e, out var unit)) {
                if (!PredictedFrame.Context.IsLocalPlayer(unit->PlayerRef)) {
                    continue;
                }

                if (!PredictedFrame.Exists(unit->AbilityRef)) {
                    image.fillAmount = 0;
                    continue;
                }
                
                var ability = unit->GetAbility(PredictedFrame);
                if (ability->CooldownTimer.IsRunning) {
                    image.fillAmount = ability->CooldownTimer.NormalizedTimeLeft.AsFloat;
                }
            }
        }
    }
}