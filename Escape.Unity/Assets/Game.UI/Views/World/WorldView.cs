namespace Game.UI.Views.World {
    using System;
    using ECS.Components.WorldView;
    using JetBrains.Annotations;
    using Scellecs.Morpeh;
    using UnityEngine;

    [RequireComponent(typeof(RectTransform))]
    public sealed class WorldView : MonoBehaviour {
        private Stash<WorldViewComponent> worldViewStash;
        private Entity                    entity;

        private void OnDisable() {
            this.entity?.Dispose();
        }

        [PublicAPI]
        public void SetTarget(Func<Vector3> func, bool hideWhenOffscreen = false) {
            if (this.worldViewStash == null) {
                this.worldViewStash = World.Default.GetStash<WorldViewComponent>();
            }

            if (func != null) {
                if (this.entity.IsNullOrDisposed()) {
                    this.entity = World.Default.CreateEntity();
                }

                var canvas = this.GetComponentInParent<Canvas>().rootCanvas;

                var canvasGroup = hideWhenOffscreen ? this.GetComponent<CanvasGroup>() : null;
                if (hideWhenOffscreen && canvasGroup == null) {
                    canvasGroup = this.gameObject.AddComponent<CanvasGroup>();
                    canvasGroup.blocksRaycasts = false;
                    canvasGroup.interactable = false;  
                }

                this.worldViewStash.Set(this.entity, new WorldViewComponent {
                    transform  = (RectTransform) this.transform,
                    targetFunc = func,
                    canvas     = canvas,
                    canvasRect = (RectTransform) canvas.transform,
                    hideWhenOffscreen = hideWhenOffscreen,
                    canvasGroup = canvasGroup,
                });
            }
            else if (!this.entity.IsNullOrDisposed()) {
                this.worldViewStash.Remove(this.entity);
            }
        }
    }
}