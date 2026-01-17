namespace InputLayout.Scripts {
    using UnityEngine;
    using UnityEngine.EventSystems;
    using UnityEngine.InputSystem.Layouts;

    [RequireComponent(typeof(RectTransform))]
    [DefaultExecutionOrder(-50)]
    public class ScreenSwipeZone : ScreenControl, IPointerDownHandler, IPointerUpHandler, IDragHandler {
        [InputControl(layout = "Vector2")]
        [SerializeField]
        private string _controlPath;

        private int? lastDragFrame;

        protected override string controlPathInternal {
            get => _controlPath;
            set => _controlPath = value;
        }

        private void Update() {
            if (lastDragFrame.HasValue && Time.frameCount > lastDragFrame + 1) {
                SendValueToControl(Vector2.zero);
            }
        }

        public override void OnPointerUp(PointerEventData eventData) {
            lastDragFrame = null;
            SentDefaultValueToControl();
        }

        public override void OnPointerDown(PointerEventData eventData) {
            lastDragFrame = Time.frameCount;
            SendValueToControl(Vector2.zero);
        }

        public override void OnDrag(PointerEventData eventData) {
            var max   = ((RectTransform)transform).rect.size;
            var delta = eventData.delta;

            lastDragFrame = Time.frameCount;
            SendValueToControl(new Vector2(delta.x / max.x, delta.y / max.y));
        }
    }
}