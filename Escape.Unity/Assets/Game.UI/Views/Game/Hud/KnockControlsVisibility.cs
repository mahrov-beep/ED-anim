namespace Game.UI.Views.Game.Hud {
    using Multicast;
    using UniMob.UI;
    using UnityEngine;

    /// <summary>
    /// AutoView that toggles visibility and interaction of a canvas group when the local player is knocked.
    /// </summary>
    [RequireComponent(typeof(CanvasGroup))]
    public sealed class KnockControlsVisibility : AutoView<IKnockControlsViewState> {
        [SerializeField] private CanvasGroup canvasGroup;

        bool? lastHidden;

        protected override void Awake() {
            base.Awake();
            canvasGroup ??= GetComponent<CanvasGroup>();
        }

        protected override void Render() {
            base.Render();

            if (canvasGroup == null || this.State == null) {
                return;
            }

            UpdateVisibility(this.State.HideInput);
        }

        void UpdateVisibility(bool shouldHide) {
            if (lastHidden.HasValue && lastHidden.Value == shouldHide) {
                return;
            }

            lastHidden = shouldHide;
            canvasGroup.alpha = shouldHide ? 0f : 1f;
            canvasGroup.interactable = !shouldHide;
            canvasGroup.blocksRaycasts = !shouldHide;
        }
    }

    public interface IKnockControlsViewState : IViewState {
        bool HideInput { get; }
    }
}
