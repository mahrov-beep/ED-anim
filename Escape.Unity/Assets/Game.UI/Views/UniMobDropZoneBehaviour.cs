namespace Game.UI.Views {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using JetBrains.Annotations;
    using UnityEngine;
    using UnityEngine.Events;
    using UnityEngine.UI;

    [RequireComponent(typeof(Image))]
    public class UniMobDropZoneBehaviour : MonoBehaviour {
        [SerializeField] private GameObject acceptHighlightingObject;
        [SerializeField] private GameObject hoverHighlightingObject;
        [SerializeField] private GameObject hoverFailHighlightingObject;

        private readonly List<object> activeDragAndDropPayloads = new List<object>();
        private readonly List<object> hoverDragAndDropPayloads  = new List<object>();

        [PublicAPI]
        public readonly UnityEvent<object> OnAccept = new UnityEvent<object>();
        
        [PublicAPI]
        public readonly UnityEvent<object> OnHoverEndEvent = new UnityEvent<object>();
        [PublicAPI]
        public readonly UnityEvent<object> OnDragAndDropEndEvent = new UnityEvent<object>();

        [PublicAPI]
        public Action<List<object>, bool> CustomHighlightDelegate; // args: List<object> payloads, bool isAccepted

        [PublicAPI]
        public Func<object, bool> IsPayloadAcceptableDelegate;
        
        public bool CanDrop      { get; set; } = true;
        public bool CanAccept    { get; set; } = false;
        public bool CanNotAccept { get; set; } = false;

        protected virtual void OnEnable() {
            this.activeDragAndDropPayloads.Clear();
            this.hoverDragAndDropPayloads.Clear();

            this.RefreshAcceptState();
            this.RefreshHoverState();

            UniMobDragAndDrop.RegisterDropZone(this);
        }

        protected virtual void OnDisable() {
            this.activeDragAndDropPayloads.Clear();
            this.hoverDragAndDropPayloads.Clear();

            UniMobDragAndDrop.UnregisterDropZone(this);
        }

        [PublicAPI]
        public bool IsPayloadAcceptable(object payload) {
            return this.IsPayloadAcceptableDelegate == null || this.IsPayloadAcceptableDelegate(payload);
        }
        
        internal static void NotifyHasValidZone(object payload) {
        }

        internal bool AcceptPayload(object payload) {
            if (!this.IsPayloadAcceptable(payload)) {
                return false;
            }

            this.OnAccept.Invoke(payload);
            return true;
        }

        internal void OnHoverBegin(object payload) {
            this.hoverDragAndDropPayloads.Add(payload);
            this.RefreshHoverState();
        }

        internal void OnHoverEnd(object payload) {
            this.hoverDragAndDropPayloads.Remove(payload);
            this.RefreshHoverState();
            
            this.OnHoverEndEvent.Invoke(payload);
        }

        internal void OnDragAndDropBegin(object payload) {
            this.activeDragAndDropPayloads.Add(payload);
            this.RefreshAcceptState();
        }

        internal void OnDragAndDropEnd(object payload) {
            this.activeDragAndDropPayloads.Remove(payload);
            this.RefreshAcceptState();
            
            this.OnDragAndDropEndEvent.Invoke(payload);
        }

        private void RefreshAcceptState() {
            if (this.acceptHighlightingObject != null) {
                var canAcceptDragAndDrop = this.activeDragAndDropPayloads.Count > 0 &&
                                           this.activeDragAndDropPayloads.All(it => this.IsPayloadAcceptable(it));

                var isAccepted = canAcceptDragAndDrop && this.CanDrop;
                this.acceptHighlightingObject.SetActive(isAccepted);
                this.CustomHighlightDelegate?.Invoke(this.activeDragAndDropPayloads, isAccepted);
            }
        }

        public void RefreshHoverHighlight() {
            if (this.hoverHighlightingObject != null) {
                this.hoverHighlightingObject.SetActive(this.CanAccept);
            }

            if (this.hoverFailHighlightingObject != null) {
                this.hoverFailHighlightingObject.SetActive(this.CanNotAccept);
            }
        }

        private void RefreshHoverState() {
            if (this.hoverHighlightingObject != null) {
                var hovered = this.hoverDragAndDropPayloads.Count > 0 &&
                              this.hoverDragAndDropPayloads.Any(it => this.IsPayloadAcceptable(it));

                this.hoverHighlightingObject.SetActive(hovered && this.CanDrop);

                if (this.hoverFailHighlightingObject) {
                    this.hoverFailHighlightingObject.SetActive(hovered && !this.CanDrop);
                }
            }
        }
    }
}