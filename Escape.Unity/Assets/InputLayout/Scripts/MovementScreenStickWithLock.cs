namespace InputLayout.Scripts {
    using Sirenix.OdinInspector;
    using UnityEngine;
    using UnityEngine.EventSystems;
    using UnityEngine.InputSystem;
    using UnityEngine.InputSystem.Layouts;
    using UnityEngine.Serialization;

    public class MovementScreenStickWithLock : ScreenControl, IPointerDownHandler, IPointerUpHandler, IDragHandler {
        [SerializeField, Required]
        private MovementScreenSprintButton sprintButton;
        
        [SerializeField]
        private float movementRange = 50f;

        [SerializeField, Range(1f, 180f)]
        private float angleToStartSprint = 30f;

        [FormerlySerializedAs("sprintIconCanvas")]
        [SerializeField]
        private CanvasGroup lockIconCanvas;

        [SerializeField]
        private float iconFadeDuration = 0.2f;

        [FormerlySerializedAs("sprintTarget")]
        [SerializeField]
        private RectTransform lockButton;

        [FormerlySerializedAs("sprintTargetCanvas")]
        [SerializeField]
        private CanvasGroup lockButtonGroup;

        [FormerlySerializedAs("sprintTargetOffset")]
        [SerializeField]
        private float lockTargetOffset = 90f;

        [FormerlySerializedAs("sprintTargetRadius")]
        [SerializeField]
        private float lockTargetRadius = 55f;

        [FormerlySerializedAs("sprintTargetAppearMag")]
        [SerializeField, Range(0f, 1f)]
        private float lockTargetAppearMag = 0.5f;

        [FormerlySerializedAs("sprintTargetHoldTime")]
        [SerializeField]
        private float lockTargetHoldTime = 0.3f;

        [InputControl(layout = "Vector2")]
        [SerializeField]
        private string _controlPath;

        private readonly Vector2 lockedMoveDirection = Vector2.up;

        private RectTransform parentRect;
        private RectTransform rectTransform;
        private Vector2       pointerDownPos; // в локале parentRect
        private Vector2       currentInput;
        private Vector3       startPos;
        private Vector2       lockButtonStartPos;
        private float         targetIconAlpha;
        private float         targetLockIconAlpha;
        private float         cosHalfAngle;
        private int           activePointerId = -1;
        private Camera        activePointerPressCamera;
        private int           activeTouchId = -1;

        private float   lockTargetHoldTimer;
        private bool    pointerIsDown;
        private Vector2 lastPointerLocalPos;

        private readonly Vector2 lockedInputDirection = Vector2.up;
        
        public bool IsLocked        { get; private set; }

        protected override string controlPathInternal {
            get => _controlPath;
            set => _controlPath = value;
        }

        void Start() {
            parentRect    = transform.parent.GetComponentInParent<RectTransform>();
            rectTransform = GetComponent<RectTransform>();
            startPos      = rectTransform.anchoredPosition;
            cosHalfAngle  = Mathf.Cos(Mathf.Deg2Rad * angleToStartSprint * 0.5f);
            if (lockButton != null) {
                lockButtonStartPos = lockButton.anchoredPosition;
            }

            if (lockIconCanvas != null) {
                lockIconCanvas.alpha = 0f;
                targetIconAlpha        = 0f;
            }
            if (lockButtonGroup != null) {
                lockButtonGroup.alpha = 0f;
                targetLockIconAlpha        = 0f;
            }
        }

        void Update() {
            if (lockIconCanvas != null && !Mathf.Approximately(lockIconCanvas.alpha, targetIconAlpha)) {
                float step = Time.unscaledDeltaTime / Mathf.Max(0.0001f, iconFadeDuration);
                lockIconCanvas.alpha = Mathf.MoveTowards(lockIconCanvas.alpha, targetIconAlpha, step);
            }
            if (lockButtonGroup != null && !Mathf.Approximately(lockButtonGroup.alpha, targetLockIconAlpha)) {
                float step = Time.unscaledDeltaTime / Mathf.Max(0.0001f, iconFadeDuration);
                lockButtonGroup.alpha = Mathf.MoveTowards(lockButtonGroup.alpha, targetLockIconAlpha, step);
            }

            if (IsLocked) {
                rectTransform.anchoredPosition = startPos;
                SendValueToControl(lockedInputDirection);
            }
            else {
                SendValueToControl(currentInput);
            }

            if (pointerIsDown && !IsLocked && activePointerId != -1 && Touchscreen.current != null) {
                if (activeTouchId == -1) {
                    activeTouchId = ResolveTouchIdByPointerId(activePointerId);
                }

                if (activeTouchId != -1 && !IsTouchPressed(activeTouchId)) {
                    ForcePointerUp();
                }
            }

            if (pointerIsDown && lockButtonGroup != null && lockButtonGroup.alpha > 0f && !IsLocked) {
                if (IsPointerOverSprintTarget(lastPointerLocalPos)) {
                    lockTargetHoldTimer += Time.unscaledDeltaTime;
                    if (lockTargetHoldTimer >= lockTargetHoldTime) {
                        LockDirection(currentInput);
                    }
                }
                else {
                    lockTargetHoldTimer = 0f;
                }
            }
        }

        public void SetIconTargetAlpha(float value) {
            targetIconAlpha = Mathf.Clamp01(value);
        }

        public override void OnPointerDown(PointerEventData eventData) {
            if (eventData == null) {
                return;
            }

            if (pointerIsDown && !IsLocked) {
                return;
            }

            if (activePointerId != -1 && activePointerId != eventData.pointerId) {
                return;
            }

            activePointerId           = eventData.pointerId;
            activePointerPressCamera = eventData.pressEventCamera;

            if (IsLocked) {
                Unlock(true);
            }

            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                parentRect,
                eventData.position,
                eventData.pressEventCamera,
                out pointerDownPos);

            lastPointerLocalPos = pointerDownPos;
            pointerIsDown       = true;
            lockTargetHoldTimer = 0f;
            currentInput        = Vector2.zero;

            if (Touchscreen.current != null) {
                activeTouchId = ResolveTouchIdByPointerId(eventData.pointerId);
                if (activeTouchId == -1) {
                    activeTouchId = ResolveTouchIdByNearestScreenPos(eventData.position);
                }
            }

            HideSprintTarget();
        }

        public override void OnDrag(PointerEventData eventData) {
            if (eventData == null) {
                return;
            }

            if (eventData.pointerId != activePointerId) {
                return;
            }

            if (IsLocked) {
                return;
            }

            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                parentRect,
                eventData.position,
                eventData.pressEventCamera,
                out var position);

            ApplyPointerLocalPosition(position);
        }

        public override void OnPointerUp(PointerEventData eventData) {
            if (Touchscreen.current != null && activeTouchId != -1) {
                if (IsTouchPressed(activeTouchId)) {
                    return;
                }

                ForcePointerUp();
                return;
            }

            if (eventData != null && activePointerId != -1 && eventData.pointerId != activePointerId) {
                return;
            }

            ForcePointerUp();
        }

        protected override void OnDisable() {
            ForcePointerUp();
            base.OnDisable();
        }

        private void ForcePointerUp() {
            activePointerId           = -1;
            activePointerPressCamera = null;
            activeTouchId             = -1;

            pointerIsDown       = false;
            lockTargetHoldTimer = 0f;
            HideSprintTarget();

            if (!IsLocked) {
                rectTransform.anchoredPosition = startPos;
                currentInput                   = Vector2.zero;
                SendValueToControl(Vector2.zero);
            }
        }

        private void ApplyPointerLocalPosition(Vector2 pointerLocalPos) {
            lastPointerLocalPos = pointerLocalPos;

            var delta = pointerLocalPos - pointerDownPos;
            delta = Vector2.ClampMagnitude(delta, movementRange);

            rectTransform.anchoredPosition = startPos + (Vector3)delta;
            currentInput                   = new Vector2(delta.x / movementRange, delta.y / movementRange);

            var  mag    = currentInput.magnitude;
            bool inCone = mag > 0.0001f && Vector2.Dot(currentInput.normalized, lockedMoveDirection) >= cosHalfAngle;

            if (inCone && mag >= lockTargetAppearMag) {
                ShowSprintTarget(pointerDownPos);
                if (!IsPointerOverSprintTarget(pointerLocalPos)) {
                    lockTargetHoldTimer = 0f;
                    SetIconTargetAlpha(0f);
                }
            }
            else {
                lockTargetHoldTimer = 0f;
                HideSprintTarget();
                SetIconTargetAlpha(0f);
            }
        }

        private int ResolveTouchIdByNearestScreenPos(Vector2 screenPos) {
            var touchscreen = Touchscreen.current;
            if (touchscreen == null) {
                return -1;
            }

            var bestDist = float.PositiveInfinity;
            var bestId   = -1;

            foreach (var touch in touchscreen.touches) {
                if (!touch.press.isPressed) {
                    continue;
                }

                var pos = touch.position.ReadValue();
                var d2  = (pos - screenPos).sqrMagnitude;
                if (d2 < bestDist) {
                    bestDist = d2;
                    bestId   = (int)touch.touchId.ReadValue();
                }
            }

            return bestId;
        }

        private int ResolveTouchIdByPointerId(int pointerId) {
            var touchscreen = Touchscreen.current;
            if (touchscreen == null) {
                return -1;
            }

            foreach (var touch in touchscreen.touches) {
                if (!touch.press.isPressed) {
                    continue;
                }

                var touchId = (int)touch.touchId.ReadValue();
                if (touchId == pointerId) {
                    return touchId;
                }
            }

            if (pointerId >= 0 && pointerId < touchscreen.touches.Count) {
                var touch = touchscreen.touches[pointerId];
                if (touch.press.isPressed) {
                    return (int)touch.touchId.ReadValue();
                }
            }

            return -1;
        }

        private bool IsTouchPressed(int touchId) {
            var touchscreen = Touchscreen.current;
            if (touchscreen == null) {
                return false;
            }

            foreach (var touch in touchscreen.touches) {
                if (!touch.press.isPressed) {
                    continue;
                }

                if ((int)touch.touchId.ReadValue() == touchId) {
                    return true;
                }
            }

            return false;
        }

        
        void LockDirection(Vector2 currentDir) {
            IsLocked = true;
            this.sprintButton.SetSprintActive(true);
            
            rectTransform.anchoredPosition = startPos;
            currentInput                   = Vector2.zero;

            HideSprintTarget();
            SetIconTargetAlpha(1f);
        }

        public void Unlock(bool resetStick) {
            IsLocked             = false;
            this.sprintButton.SetSprintActive(false);

            if (resetStick) {
                rectTransform.anchoredPosition = startPos;
                currentInput                   = Vector2.zero;
            }

            SetIconTargetAlpha(0f);
            HideSprintTarget();
        }

        void ShowSprintTarget(Vector2 baseLocalPos) {
            if (lockButton == null || lockButtonGroup == null) {
                return;
            }

            var lockY = baseLocalPos.y + lockTargetOffset;
            lockButton.anchoredPosition = new Vector2(lockButtonStartPos.x, lockY);
            targetLockIconAlpha             = 1f;
        }

        void HideSprintTarget() {
            if (lockButtonGroup == null) {
                return;
            }

            targetLockIconAlpha = 0f;
        }

        bool IsPointerOverSprintTarget(Vector2 pointerLocalPos) {
            if (lockButton == null) {
                return false;
            }

            var center = lockButton.anchoredPosition;
            var delta  = pointerLocalPos - center;

            var inCircle = delta.sqrMagnitude <= lockTargetRadius * lockTargetRadius;
            var aboveCap = delta.y >= 0f && Mathf.Abs(delta.x) <= lockTargetRadius;

            return inCircle || aboveCap;
        }
    }
}