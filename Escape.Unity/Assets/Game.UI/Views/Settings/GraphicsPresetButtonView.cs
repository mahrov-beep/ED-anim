namespace Game.UI.Views.Settings {
    using Sirenix.OdinInspector;
    using TMPro;
    using UniMob.UI;
    using UnityEngine;
    using UnityEngine.UI;
    using Multicast;
    using UI.Widgets.Settings;

    public class GraphicsPresetButtonView : AutoView<IGraphicsOptionButtonState> {
        [SerializeField, Required] private TMP_Text title;
        [SerializeField, Required] private Button   button;
        [SerializeField]            private GameObject selectedIndicator;
        [SerializeField]            private GameObject recommendedLabel;

        protected override void Awake() {
            base.Awake();
            this.button.onClick.AddListener(this.OnClick);
        }

        protected override void OnDestroy() {
            base.OnDestroy();
            this.button.onClick.RemoveListener(this.OnClick);
        }

        protected override void Render() {
            base.Render();

            if (this.title != null) {
                this.title.text = this.State.Title;
            }

            if (this.selectedIndicator != null) {
                this.selectedIndicator.SetActive(this.State.IsSelected);
            }

            if (this.recommendedLabel != null) {
                this.recommendedLabel.SetActive(this.State.IsRecommended);
            }

            if (this.button != null) {
                this.button.interactable = !this.State.IsSelected;
            }
        }

        private void OnClick() {
            this.State.Select();
        }
    }

    public interface IGraphicsPresetButtonState : IGraphicsOptionButtonState {
        int    QualityIndex { get; }
        string RawName      { get; }
    }
}
