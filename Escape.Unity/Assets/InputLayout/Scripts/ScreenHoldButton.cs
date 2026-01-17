namespace InputLayout.Scripts {
    using UnityEngine;
    using UnityEngine.EventSystems;
    using UnityEngine.InputSystem.Layouts;
    using UnityEngine.UI;
    
    public class ScreenHoldButton : ScreenControl,
        IPointerDownHandler, IPointerUpHandler {

        [SerializeField] private float holdDuration    = 0.5f;
        [SerializeField] private bool  activeAfterTimer = false;

        [SerializeField] private Image  holdImage;
        [SerializeField] private Slider holdSlider;

        [InputControl(layout = "Button")] [SerializeField]
        private string _controlPath;

        private float elapsed;
        private bool  holding;
        private bool  holdCompleted;

        protected override string controlPathInternal {
            get => _controlPath;
            set => _controlPath = value;
        }

        private void Update() {
            if (!holding) {
                ResetFill();
                return;
            }

            elapsed += Time.unscaledDeltaTime;

            UpdateFill();

            if (!holdCompleted && elapsed >= holdDuration) {
                holdCompleted = true;
                if (activeAfterTimer) {
                    SendValueToControl(0.0f); // auto-release after hold
                    CancelHold();
                }
            }
        }

        public override void OnPointerDown(PointerEventData eventData) {
            holding       = true;
            holdCompleted = false;
            elapsed       = 0f;

            SendValueToControl(1.0f);
        }

        public override void OnPointerUp(PointerEventData eventData) {
            if (holdCompleted && !activeAfterTimer) {
                SendValueToControl(0.0f);
            }

            CancelHold();
        }

        public override void OnDrag(PointerEventData eventData) {} 

        private void CancelHold() {
            holding       = false;
            holdCompleted = false;
            elapsed       = 0f;
            ResetFill();
        }

        private void ResetFill() {
            if (holdSlider) {
                holdSlider.value = 0f;
            }

            if (holdImage) {
                holdImage.fillAmount = 0f;
            }
        }

        private void UpdateFill() {
            var total = Mathf.Max(0.001f, holdDuration);
            var normalized = Mathf.Clamp01(elapsed / total);

            if (holdSlider) {
                holdSlider.value = normalized;
            }

            if (holdImage) {
                holdImage.fillAmount = normalized;
            }
        }
    }
}
