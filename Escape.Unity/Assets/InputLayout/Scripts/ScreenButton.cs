namespace InputLayout.Scripts {
    using UnityEngine;
    using UnityEngine.EventSystems;
    using UnityEngine.InputSystem.Layouts;

    public class ScreenButton : ScreenControl, IPointerDownHandler, IPointerUpHandler {
        public override void OnPointerUp(PointerEventData eventData) {
            SendValueToControl(0.0f);
        }

        public override void OnPointerDown(PointerEventData eventData) {
            SendValueToControl(1.0f);
        }

        public override void OnDrag(PointerEventData eventData) { }

        [InputControl(layout = "Button")][SerializeField]
        private string _controlPath;

        protected override string controlPathInternal {
            get => _controlPath;
            set => _controlPath = value;
        }
    }
}