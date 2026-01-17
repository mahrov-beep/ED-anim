namespace Multicast.UI.Views {
    using UniMob.UI;
    using Sirenix.OdinInspector;
    using TMPro;
    using UnityEngine;
    using UnityEngine.UI;

    public class DebugListItemView : View<IDebugListItemState> {
        [SerializeField, Required] private TMP_Text primaryText;
        [SerializeField, Required] private TMP_Text secondaryText;

        [SerializeField, Required] private Button button;

        protected override void Awake() {
            base.Awake();

            this.button.Click(() => this.State.OnClick);
        }

        protected override void Render() {
            this.primaryText.text  = this.State.PrimaryText;
            this.primaryText.color = this.State.PrimaryTextColor;

            this.secondaryText.text  = this.State.SecondaryText;
            this.secondaryText.color = this.State.SecondaryTextColor;
        }
    }

    public interface IDebugListItemState : IViewState {
        string PrimaryText   { get; }
        string SecondaryText { get; }

        Color PrimaryTextColor   { get; }
        Color SecondaryTextColor { get; }

        void OnClick();
    }
}