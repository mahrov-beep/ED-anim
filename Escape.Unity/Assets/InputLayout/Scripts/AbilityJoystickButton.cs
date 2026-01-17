namespace InputLayout.Scripts {
    using UnityEngine;
    using UnityEngine.EventSystems;
    using UnityEngine.InputSystem.Layouts;

    public sealed class AbilityJoystickButton : ScreenControl {
        [SerializeField] private RectTransform dragArea;
        [SerializeField] private RectTransform handle;         
        [SerializeField] private float handleRange = 120f;

        [InputControl(layout = "Button")]
        [SerializeField] private string _controlPath = "<Keyboard>/q";
        [SerializeField] private AbilityJoystickLookControl lookControl;
        private RectTransform normalizationRect;
        private RectTransform areaRect;
        private Vector2 defaultHandlePos;
        private Vector2 pointerDownLocal;
        private bool isPointerActive;
        private int? lastDragFrame;

        protected override string controlPathInternal {
            get => _controlPath;
            set => _controlPath = value;
        }

        void Awake() {
            EnsureLookControl();
            areaRect = dragArea != null ? dragArea : GetComponent<RectTransform>();
            defaultHandlePos = handle != null ? handle.anchoredPosition : Vector2.zero;
            EnsureNormalizationRect();
        }       

        void Update() {
            if (lookControl == null) {
                return;
            }

            if (lastDragFrame.HasValue && Time.frameCount > lastDragFrame.Value + 1) {
                lookControl.ResetDelta();
            }
        }

        public override void OnPointerDown(PointerEventData eventData) {
            if (eventData == null || (handle != null && !handle.gameObject.activeInHierarchy)) {
                return;
            }

            SendValueToControl(1f);
            lastDragFrame = Time.frameCount;
            lookControl?.ResetDelta();

            if (areaRect == null) {
                pointerDownLocal = Vector2.zero;
                isPointerActive = true;
            }
            else {
                if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                            areaRect,
                            eventData.position,
                            eventData.pressEventCamera,
                            out pointerDownLocal)) {
                    pointerDownLocal = Vector2.zero;
                }

                isPointerActive = true;
            }

            if (handle != null) {
                handle.anchoredPosition = defaultHandlePos;
            }
        }

        public override void OnDrag(PointerEventData eventData) {
            if (!isPointerActive || eventData == null || (handle != null && !handle.gameObject.activeInHierarchy)) {
                return;
            }

            lastDragFrame = Time.frameCount;

            if (areaRect != null) {
                if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                            areaRect,
                            eventData.position,
                            eventData.pressEventCamera,
                            out var currentLocal)) {
                    var delta = currentLocal - pointerDownLocal;
                    var clamped = Vector2.ClampMagnitude(delta, handleRange);
                    if (handle != null) {
                        handle.anchoredPosition = defaultHandlePos + clamped;
                    }
                }
            }

            if (lookControl != null) {
                var rect = normalizationRect != null ? normalizationRect : areaRect;
                if (rect != null) {
                    var size = rect.rect.size;

                    if (!Mathf.Approximately(size.x, 0f) && !Mathf.Approximately(size.y, 0f)) {
                        var normalizedDelta = new Vector2(eventData.delta.x / size.x, eventData.delta.y / size.y);
                        lookControl.SetDelta(normalizedDelta);
                    }
                }
            }
        }

        public override void OnPointerUp(PointerEventData eventData) {
            if (!isPointerActive || (handle != null && !handle.gameObject.activeInHierarchy)) {
                return;
            }

            isPointerActive = false;
            lastDragFrame = null;

            if (handle != null) {
                handle.anchoredPosition = defaultHandlePos;
            }

            SendValueToControl(0f);
            lookControl?.ResetDelta();
        }

        protected override void OnDisable() {
            isPointerActive = false;
            lastDragFrame = null;
            SendValueToControl(0f);
            lookControl?.ResetDelta();

            if (handle != null) {
                handle.anchoredPosition = defaultHandlePos;
            }
        }

        void EnsureLookControl() {
            if (lookControl != null) {
                return;
            }

            lookControl = GetComponentInChildren<AbilityJoystickLookControl>(true);
            if (lookControl != null) {
                return;
            }

            var child = new GameObject("AbilityJoystickLookControl");
            child.transform.SetParent(transform, false);
            lookControl = child.AddComponent<AbilityJoystickLookControl>();
        }

        void EnsureNormalizationRect() {
            normalizationRect = GameObject.Find("LookDelta SwipeZone").GetComponent<RectTransform>(); //HARD
            if (normalizationRect != null) {
                return;
            }

            normalizationRect = dragArea != null ? dragArea : GetComponentInParent<RectTransform>();
            if (normalizationRect == null) {
                normalizationRect = GetComponent<RectTransform>();
            }
        }
    }
}
