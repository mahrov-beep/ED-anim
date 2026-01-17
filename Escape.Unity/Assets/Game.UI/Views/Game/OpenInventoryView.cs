namespace Game.UI.Views.Game {
    using Multicast;
    using TMPro;
    using UniMob.UI;
    using UnityEngine;
    using UnityEngine.UI;

    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class OpenInventoryView : AutoView<IOpenInventoryIndicatorState> {
        [System.Serializable]
        private struct FillColorStep {
            [Range(0f, 1f)] public float progressValue;
            public Color                 color;
        }

        [SerializeField] private Image           fillImage;
        [SerializeField] private Image           bgfillImage;
        [SerializeField] private Image           bgImage;
        [SerializeField] private FillColorStep[] fillColorSteps = {
            new FillColorStep { progressValue = 0.5f, color = Color.white },
            new FillColorStep { progressValue = 0.75f, color = Color.yellow },
            new FillColorStep { progressValue = 1f, color = Color.red },
        };
        [SerializeField] private TextMeshProUGUI weightText;
        [SerializeField] private string          weightFormat = "{0}/{1}";

        protected override void Render() {
            base.Render();

            if (this.State == null) {
                return;
            }

            if (this.fillImage != null) {
                var fillNormalized = Mathf.Clamp01(this.State.InventoryFillNormalized);
                var targetColor    = this.EvaluateFillColor(fillNormalized);

                this.fillImage.fillAmount = fillNormalized;
                this.ApplyColorPreservingAlpha(this.fillImage, targetColor);
                this.ApplyColorPreservingAlpha(this.bgfillImage, targetColor);
                this.ApplyColorPreservingAlpha(this.bgImage, targetColor);
            }

            if (this.weightText != null) {
                var current = this.State.InventoryCurrentWeight;
                var limit   = this.State.InventoryWeightLimit;

                if (limit <= 0f) {
                    this.weightText.text = current.ToString("0");
                }
                else {
                    this.weightText.text = string.Format(this.weightFormat, Mathf.RoundToInt(current), Mathf.RoundToInt(limit));
                }
            }
        }

        private Color EvaluateFillColor(float normalizedValue) {
            if (this.fillColorSteps == null || this.fillColorSteps.Length == 0) {
                return this.fillImage != null ? this.fillImage.color : Color.white;
            }

            var color = this.fillColorSteps[this.fillColorSteps.Length - 1].color;

            for (var i = 0; i < this.fillColorSteps.Length; i++) {
                var step = this.fillColorSteps[i];

                if (normalizedValue <= step.progressValue) {
                    color = step.color;
                    break;
                }
            }

            return color;
        }

        private void ApplyColorPreservingAlpha(Image targetImage, Color strokeColor) {
            if (targetImage == null) {
                return;
            }

            var current = targetImage.color;
            strokeColor.a      = current.a;
            targetImage.color  = strokeColor;
        }
    }

    public interface IOpenInventoryIndicatorState : IViewState {
        float InventoryFillNormalized { get; }
        float InventoryCurrentWeight  { get; }
        float InventoryWeightLimit    { get; }
    }
}
