namespace InputLayout.Scripts {
    using UnityEngine;
    using UnityEngine.EventSystems;
    using UnityEngine.InputSystem.Layouts;
    using UnityEngine.Serialization;

    public class ScreenStick : ScreenControl, IPointerDownHandler, IPointerUpHandler, IDragHandler {
        [SerializeField]
        private float movementRange = 50;

        [InputControl(layout = "Vector2")][SerializeField]
        private string _controlPath;

        private Vector3 startPos;
        private Vector2 pointerDownPos;

        private RectTransform parentRect;
        private RectTransform rectTransform;

        protected override string controlPathInternal {
            get => _controlPath;
            set => _controlPath = value;
        }

        private void Start() {
            parentRect    = transform.parent.GetComponentInParent<RectTransform>();
            rectTransform = GetComponent<RectTransform>();

            startPos = rectTransform.anchoredPosition;
        }

        public override void OnPointerDown(PointerEventData eventData) {
            if (eventData == null) {
                throw new System.ArgumentNullException(nameof(eventData));
            }

            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                            parentRect,
                            eventData.position,
                            eventData.pressEventCamera,
                            out pointerDownPos);
        }

        public override void OnDrag(PointerEventData eventData) {
            if (eventData == null) {
                throw new System.ArgumentNullException(nameof(eventData));
            }

            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                            parentRect,
                            eventData.position,
                            eventData.pressEventCamera,
                            out var position);

            var delta = position - pointerDownPos;

            delta = Vector2.ClampMagnitude(delta, movementRange);

            rectTransform.anchoredPosition = startPos + (Vector3)delta;

            var newPos = new Vector2(delta.x / movementRange, delta.y / movementRange);

            SendValueToControl(newPos);
        }

        public override void OnPointerUp(PointerEventData eventData) {
            rectTransform.anchoredPosition = startPos;

            SendValueToControl(Vector2.zero);
        }
    }
}