namespace Game.UI.Views {
    using System;
    using UnityEngine;
    using UnityEngine.Events;
    using UnityEngine.EventSystems;
    using UnityEngine.UI;

    [Serializable]
    public class HoldToActionButton : MonoBehaviour, IPointerDownHandler, IInitializePotentialDragHandler, IPointerUpHandler, IBeginDragHandler, IDragHandler, IEndDragHandler {
        public float minDuration = 0.05f;
        public float holdDuration;
        public float doubleClickMaxDelay = 0.3f;

        public bool canHold;

        public UnityEvent fastClick;
        public UnityEvent doubleClick;
        public UnityEvent holdEnded;
        public UnityEvent holdStarted;

        public Image holdImage;
        public Slider holdSlider;

        private float? holdRemainingTime;
        private float? lastClickTime;
        private bool pendingSingleClick;
        private float singleClickTimer;

        void Update() {
            if (this.pendingSingleClick) {
                this.singleClickTimer -= Time.unscaledDeltaTime;
                
                if (this.singleClickTimer <= 0) {
                    this.pendingSingleClick = false;
                    this.fastClick.Invoke();
                }
            }

            if (!this.holdRemainingTime.HasValue || !this.canHold) {
                if (this.holdSlider) {
                    this.holdSlider.value = 0f;
                }

                if (this.holdImage) {
                    this.holdImage.fillAmount = 0f;
                }

                return;
            }

            this.holdRemainingTime = this.holdRemainingTime.Value - Time.unscaledDeltaTime;
            if (this.holdSlider) {
                this.holdSlider.value     = 1f - (this.holdRemainingTime.Value + this.minDuration) / Mathf.Max(0.001f, this.holdDuration);
            }

            if (this.holdImage) {
                this.holdImage.fillAmount = 1f - (this.holdRemainingTime.Value + this.minDuration) / Mathf.Max(0.001f, this.holdDuration);
            }

            if (this.holdRemainingTime.Value > 0) {
                return;
            }

            this.holdRemainingTime = null;
            this.pendingSingleClick = false;

            this.holdEnded?.Invoke();
        }

        public void OnPointerDown(PointerEventData eventData) {
            ExecuteEvents.ExecuteHierarchy(this.transform.parent.gameObject, eventData, ExecuteEvents.pointerDownHandler);

            this.holdRemainingTime = this.holdDuration + this.minDuration;
            
            this.holdStarted?.Invoke();
        }

        public void OnPointerUp(PointerEventData eventData) {
            ExecuteEvents.ExecuteHierarchy(this.transform.parent.gameObject, eventData, ExecuteEvents.pointerUpHandler);

            if (this.holdRemainingTime.HasValue) {
                var currentTime = Time.unscaledTime;
                var isDoubleClick = this.lastClickTime.HasValue && (currentTime - this.lastClickTime.Value) < this.doubleClickMaxDelay;

                if (isDoubleClick) {
                    this.pendingSingleClick = false;
                    this.lastClickTime = null;
                    this.doubleClick?.Invoke();
                }
                else {
                    this.lastClickTime = currentTime;
                    this.pendingSingleClick = true;
                    this.singleClickTimer = this.doubleClickMaxDelay;
                }
            }

            this.holdRemainingTime = null;
        }


        public void OnInitializePotentialDrag(PointerEventData eventData) {
            ExecuteEvents.ExecuteHierarchy(this.transform.parent.gameObject, eventData, ExecuteEvents.initializePotentialDrag);
        }

        public void CancelHold() {
            if (!this.holdRemainingTime.HasValue) {
                return;
            }

            this.holdRemainingTime = null;

            if (this.holdSlider) {
                this.holdSlider.value = 0f;
            }

            if (this.holdImage) {
                this.holdImage.fillAmount = 0f;
            }
        }

        public void OnDrag(PointerEventData eventData) {
            ExecuteEvents.ExecuteHierarchy(this.transform.parent.gameObject, eventData, ExecuteEvents.dragHandler);

            this.holdRemainingTime = null;
            this.pendingSingleClick = false;
        }

        public void OnBeginDrag(PointerEventData eventData) {
            ExecuteEvents.ExecuteHierarchy(this.transform.parent.gameObject, eventData, ExecuteEvents.beginDragHandler);
            
            this.pendingSingleClick = false;
        }

        public void OnEndDrag(PointerEventData eventData) {
            ExecuteEvents.ExecuteHierarchy(this.transform.parent.gameObject, eventData, ExecuteEvents.endDragHandler);
        }
    }
}