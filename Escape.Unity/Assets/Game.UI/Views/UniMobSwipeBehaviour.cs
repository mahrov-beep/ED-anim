namespace Game.UI.Views {
    using System;
    using JetBrains.Annotations;
    using UnityEngine;
    using UnityEngine.EventSystems;
    using UnityEngine.Pool;
    using UnityEngine.UI;

    public class UniMobSwipeBehaviour : MonoBehaviour, IInitializePotentialDragHandler, IBeginDragHandler, IDragHandler, IEndDragHandler {
        [SerializeField] private RectTransform rootObject;
        [SerializeField] private RectTransform dragObject;
        [SerializeField] private bool          routeEventsToParent;
        [SerializeField] private Vector2       minimalDistances;

        private Vector2  initialPosition;
        private Vector2  initialOffset;
        private bool     resetAfterDropRequired;
        private float    resetAfterDropSpeed;
        private object   activeDragAndDropPayload;
        private Vector2? targetPosition;
        private bool     isSubDragging;
        private bool     reActivationRequested;

        private UniMobDropZoneBehaviour hoveredZone;

        [PublicAPI]
        public Func<object> CreateDragAndDropPayloadDelegate;

        private void OnEnable() {
            this.SetInitial();
        }

        private void OnTransformParentChanged() {
            this.SetInitial();
        }

        private void Update() {
            if (this.reActivationRequested) {
                this.reActivationRequested = false;
                
                this.gameObject.SetActive(false);
                this.gameObject.SetActive(true);
            }
            
            if (this.resetAfterDropRequired) {
                this.resetAfterDropSpeed         += 30;
                this.resetAfterDropSpeed         *= 1.2f;
                this.dragObject.anchoredPosition =  Vector2.MoveTowards(this.dragObject.anchoredPosition, this.initialPosition, Time.smoothDeltaTime * this.resetAfterDropSpeed);

                if (Vector3.Distance(this.dragObject.anchoredPosition, this.initialPosition) < 10) {
                    this.SetInitial();
                }
            }

            if (this.targetPosition.HasValue) {
                this.dragObject.anchoredPosition = Vector2.Lerp(this.dragObject.anchoredPosition, this.targetPosition.Value, Time.deltaTime * 50f);
            }
        }

        private void SetInitial() {
            if (this.hoveredZone != null) {
                if (this.activeDragAndDropPayload != null) {
                    this.hoveredZone.OnHoverEnd(this.activeDragAndDropPayload);
                }

                this.hoveredZone = null;
            }

            if (this.activeDragAndDropPayload != null) {
                UniMobDragAndDrop.NotifyEndDrag(this.activeDragAndDropPayload);
                this.activeDragAndDropPayload = null;
            }

            this.resetAfterDropRequired      = false;
            this.resetAfterDropSpeed         = 1f;
            this.targetPosition              = null;
            this.dragObject.anchoredPosition = Vector2.zero;
            this.isSubDragging               = false;

            if (this.TryGetComponent(out GraphicRaycaster raycaster)) {
                Destroy(raycaster);
            }

            if (this.TryGetComponent(out Canvas canvas)) {
                Destroy(canvas);
                this.reActivationRequested = true;
            }
        }

        public void OnInitializePotentialDrag(PointerEventData eventData) {
            if (this.routeEventsToParent) {
                ExecuteEvents.ExecuteHierarchy(this.transform.parent.gameObject, eventData, ExecuteEvents.initializePotentialDrag);
            }
        }

        public void OnBeginDrag(PointerEventData eventData) {
            if (this.routeEventsToParent) {
                ExecuteEvents.ExecuteHierarchy(this.transform.parent.gameObject, eventData, ExecuteEvents.beginDragHandler);
            }

            RectTransformUtility.ScreenPointToLocalPointInRectangle(this.rootObject, eventData.position, eventData.pressEventCamera, out this.initialOffset);
            this.initialPosition = this.dragObject.anchoredPosition;
        }

        public void OnDrag(PointerEventData eventData) {
            if (this.routeEventsToParent && !this.isSubDragging) {
                ExecuteEvents.ExecuteHierarchy(this.transform.parent.gameObject, eventData, ExecuteEvents.dragHandler);
            }

            RectTransformUtility.ScreenPointToLocalPointInRectangle(this.rootObject, eventData.position, eventData.pressEventCamera, out var localOffset);

            var pos = localOffset - this.initialOffset;

            var isInMinZone = Mathf.Abs(pos.x) < this.minimalDistances.x && Mathf.Abs(pos.y) < this.minimalDistances.y;
            if (!isInMinZone && !this.isSubDragging) {
                this.BeginSubDragInternal(eventData);
            }

            if (this.isSubDragging) {
                this.targetPosition = pos;
            }

            if (this.activeDragAndDropPayload != null) {
                if (TryGetValidDropZoneAtPointer(eventData, this.activeDragAndDropPayload, out var newHoveredZone)) {
                    if (newHoveredZone != this.hoveredZone) {
                        if (this.hoveredZone != null) {
                            this.hoveredZone.OnHoverEnd(this.activeDragAndDropPayload);
                        }

                        this.hoveredZone = newHoveredZone;
                        this.hoveredZone.OnHoverBegin(this.activeDragAndDropPayload);
                        
                        UniMobDropZoneBehaviour.NotifyHasValidZone(this.activeDragAndDropPayload);
                    }
                }
                else if (this.hoveredZone != null) {
                    this.hoveredZone.OnHoverEnd(this.activeDragAndDropPayload);
                    this.hoveredZone = null;
                }
            }
        }

        public void OnEndDrag(PointerEventData eventData) {
            if (this.routeEventsToParent) {
                ExecuteEvents.ExecuteHierarchy(this.transform.parent.gameObject, eventData, ExecuteEvents.endDragHandler);
            }

            if (this.isSubDragging) {
                this.EndSubDragInternal(eventData);
            }

            this.targetPosition         = null;
            this.resetAfterDropRequired = true;
            this.resetAfterDropSpeed    = 1;
        }

        private void BeginSubDragInternal(PointerEventData eventData) {
            if (this.CreateDragAndDropPayloadDelegate == null) {
                return;
            }
            
            this.activeDragAndDropPayload = this.CreateDragAndDropPayloadDelegate();
            if (this.activeDragAndDropPayload == null) {
                return;
            }
            
            this.isSubDragging = true;

            var canvas = this.gameObject.AddComponent<Canvas>();
            canvas.overrideSorting = true;
            canvas.sortingOrder    = 999;

            this.gameObject.AddComponent<GraphicRaycaster>();

            UniMobDragAndDrop.NotifyBeginDrag(this.activeDragAndDropPayload);
        }

        private void EndSubDragInternal(PointerEventData eventData) {
            this.isSubDragging = false;

            if (this.activeDragAndDropPayload != null) {
                if (this.hoveredZone != null) {
                    this.hoveredZone.OnHoverEnd(this.activeDragAndDropPayload);
                    this.hoveredZone = null;
                }

                if (TryGetValidDropZoneAtPointer(eventData, this.activeDragAndDropPayload, out var zone)) {
                    zone.AcceptPayload(this.activeDragAndDropPayload);
                }

                UniMobDragAndDrop.NotifyEndDrag(this.activeDragAndDropPayload);
                this.activeDragAndDropPayload = null;
            }
        }

        private static bool TryGetValidDropZoneAtPointer(PointerEventData eventData, object payload, out UniMobDropZoneBehaviour result) {
            using (ListPool<RaycastResult>.Get(out var hits)) {
                EventSystem.current.RaycastAll(eventData, hits);

                hits.Sort(static (a, b) => a.distance.CompareTo(b.distance));

                foreach (var hit in hits) {
                    if (!hit.gameObject.TryGetComponent(out UniMobDropZoneBehaviour dropZoneBehaviour)) {
                        continue;
                    }

                    if (!dropZoneBehaviour.IsPayloadAcceptable(payload)) {
                        continue;
                    }

                    result = dropZoneBehaviour;
                    return true;
                }
            }

            result = null;
            return false;
        }
    }
}