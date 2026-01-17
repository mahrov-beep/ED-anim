namespace InputLayout.Scripts {
    using UnityEngine.EventSystems;
    using UnityEngine.InputSystem.OnScreen;

    public abstract class ScreenControl : OnScreenControl, IPointerDownHandler, IPointerUpHandler, IDragHandler {
        public abstract void OnPointerUp(PointerEventData eventData);

        public abstract void OnPointerDown(PointerEventData eventData);

        public abstract void OnDrag(PointerEventData eventData);
    }
}