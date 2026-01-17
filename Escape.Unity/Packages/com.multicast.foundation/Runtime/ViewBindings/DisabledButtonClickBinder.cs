namespace Multicast {
    using System;
    using CodeWriter.ViewBinding;
    using TriInspector;
    using UniMob;
    using UnityEngine;
    using UnityEngine.EventSystems;
    using UnityEngine.UI;

    [DrawWithTriInspector]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Button))]
    public class DisabledButtonClickBinder : ViewBindingBehaviour, IPointerClickHandler {
        [Required]
        [SerializeField]
        private Button button;

        [SerializeField]
        private ViewEventVoid onClick;

        private event Action PointerClicked;

        protected override void Setup(Lifetime lifetime) {
            base.Setup(lifetime);

            lifetime.Bracket<Action>(
                e => this.PointerClicked += e,
                e => this.PointerClicked -= e,
                this.onClick.Invoke);
        }

        public void OnPointerClick(PointerEventData eventData) {
            if (eventData.button != PointerEventData.InputButton.Left) {
                return;
            }

            if (!this.button.IsActive()) {
                return;
            }

            if (this.button.IsInteractable()) {
                return;
            }

            this.PointerClicked?.Invoke();
        }

#if UNITY_EDITOR
        protected override void Reset() {
            base.Reset();

            this.button = this.GetComponent<Button>();
        }
#endif
    }
}