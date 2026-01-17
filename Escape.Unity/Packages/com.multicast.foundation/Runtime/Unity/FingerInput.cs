#if (UNITY_ANDROID || UNITY_IOS) && !UNITY_EDITOR
#define TOUCH_INPUT
#endif

namespace Multicast.Unity {
    using UnityEngine;
    using UnityEngine.EventSystems;

    public sealed class FingerInput {
        private readonly int  touchNum;
        private          int? fingerId;

        public FingerInput(int touchNum) {
            this.touchNum = touchNum;
        }

        public FingerState GetFingerState(out Vector3 mousePosition) {
#if TOUCH_INPUT
            var touchCount = Input.touchCount;
            if (touchCount > 0) {
                var eventSystem = EventSystem.current;

                var skipped = 0;
                for (var i = 0; i < touchCount; i++) {
                    var touch = Input.GetTouch(i);

                    if (this.fingerId != touch.fingerId) {
                        if (eventSystem.IsPointerOverGameObject(touch.fingerId)) {
                            continue;
                        }

                        if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled) {
                            continue;
                        }

                        if (skipped++ != this.touchNum) {
                            continue;
                        }

                        this.fingerId = touch.fingerId;
                    }

                    mousePosition = touch.position;

                    switch (touch.phase) {
                        case TouchPhase.Began:
                            return FingerState.Down;

                        case TouchPhase.Ended:
                            this.fingerId = null;
                            return FingerState.Up;

                        case TouchPhase.Canceled:
                            this.fingerId = null;
                            return FingerState.None;

                        case TouchPhase.Moved:
                        case TouchPhase.Stationary:
                            return FingerState.Pressed;
                    }
                }
            }

            if (this.fingerId.HasValue) {
                this.fingerId = null;
            }

            mousePosition = Vector3.zero;
            return FingerState.None;
#else
            if (this.touchNum == 0) {
                if (!this.fingerId.HasValue) {
                    var eventSystem = EventSystem.current;
                    if (!ReferenceEquals(eventSystem, null) && eventSystem.IsPointerOverGameObject()) {
                        mousePosition = Vector3.zero;
                        return FingerState.None;
                    }

                    this.fingerId = 0;
                }

                mousePosition = Input.mousePosition;

                if (Input.GetMouseButtonDown(0)) {
                    return FingerState.Down;
                }

                if (Input.GetMouseButtonUp(0)) {
                    return FingerState.Up;
                }

                if (Input.GetMouseButton(0)) {
                    return FingerState.Pressed;
                }
            }

            this.fingerId = null;
            mousePosition = Vector3.zero;
            return FingerState.None;
#endif
        }
    }

    public enum FingerState {
        None,
        Down,
        Pressed,
        Up,
    }
}