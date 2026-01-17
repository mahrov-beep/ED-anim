namespace InputLayout.Scripts {
    using UnityEngine;
    using UnityEngine.EventSystems;
    using UnityEngine.UI;

    public sealed class JoystickPositionBehaviour : MonoBehaviour, IDragHandler, IPointerDownHandler, IPointerUpHandler {
        public GameObject ControlsParent;

        [SerializeField]
        private RectTransform _joystick;

        private Vector3         defaultPos;
        private ScreenControl[] controls;
        private RectTransform   parentRect;

        private void Start() {
            defaultPos = _joystick.position;

            controls    = ControlsParent.GetComponents<ScreenControl>();
            parentRect  = transform.parent.GetComponentInParent<RectTransform>();
            
            var image = GetComponent<Image>();
            image.color = new Color(image.color.r, image.color.g, image.color.b, 0);
        }

        public void OnDrag(PointerEventData eventData) {
            foreach (var item in controls) {
                item.OnDrag(eventData);
            }
        }

        public void OnPointerDown(PointerEventData eventData) {
            if (eventData == null) {
                throw new System.ArgumentNullException(nameof(eventData));
            }

            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                            parentRect,
                            eventData.position,
                            eventData.pressEventCamera,
                            out var position);

            _joystick.localPosition = position;

            foreach (var item in controls) {
                item.OnPointerDown(eventData);
            }
        }

        public void OnPointerUp(PointerEventData eventData) {
            foreach (var item in controls) {
                item.OnPointerUp(eventData);
            }

            _joystick.position = defaultPos;
        }
    }
}