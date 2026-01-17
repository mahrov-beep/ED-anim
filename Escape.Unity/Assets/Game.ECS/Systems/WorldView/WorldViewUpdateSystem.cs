namespace Game.ECS.Systems.WorldView {
    using Components.Camera;
    using ECS.Components.WorldView;
    using Multicast;
    using Scellecs.Morpeh;
    using UnityEngine;

    public class WorldViewUpdateSystem : SystemBase {
        [Inject] private Stash<WorldViewComponent>        worldViewStash;
        [Inject] private Stash<CinemachineBrainComponent> cameraStash;

        private Filter worldViewFilter;
        
        private Filter cameraFilter;

        public override void OnAwake() {
            this.worldViewFilter = this.World.Filter
                .With<WorldViewComponent>()
                .Build();

            this.cameraFilter = World.Filter
                .With<CinemachineBrainComponent>()
                .Build();
        }

        public override void OnUpdate(float deltaTime) {
            var cameraEntity = this.cameraFilter.FirstOrDefault();
            
            if (cameraEntity == default) {
                return;
            }

            var cameraComponent = this.cameraStash.Get(cameraEntity);

            var cam = cameraComponent.camera;

            if (cam == null) {
                return;
            }

            foreach (var entity in this.worldViewFilter) {
                ref var worldView = ref this.worldViewStash.Get(entity);

                var worldPosition  = worldView.targetFunc.Invoke();
                var canvasSize     = worldView.canvasRect.sizeDelta;
                var halfCanvasSize = canvasSize * 0.5f;

                var viewportPosition = cam.WorldToViewportPoint(worldPosition);
                if (worldView.hideWhenOffscreen) {
                    var isVisible = viewportPosition.z > 0f
                                    && viewportPosition.x >= 0f && viewportPosition.x <= 1f
                                    && viewportPosition.y >= 0f && viewportPosition.y <= 1f;

                    var cG = worldView.canvasGroup;
                    if(cG != null) {
                        cG.alpha = isVisible ? 1f : 0f;                       
                    }         
                    
                    if (!isVisible) {
                        continue;
                    }
                }
                var inCanvasPosition = Vector2.Scale(viewportPosition, canvasSize);

                worldView.transform.anchoredPosition = inCanvasPosition - halfCanvasSize;
            }
        }
    }
}