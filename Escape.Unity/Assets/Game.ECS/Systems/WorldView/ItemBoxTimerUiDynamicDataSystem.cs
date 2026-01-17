namespace Game.ECS.Systems.WorldView {
    using System;
    using Components.ItemBox;
    using Scripts;
    using Core;
    using Game.Services.Photon;
    using Multicast;
    using Scellecs.Morpeh;
    using UnityEngine.Pool;
    using SystemBase = Scellecs.Morpeh.SystemBase;

    public class ItemBoxTimerUiDynamicDataSystem : SystemBase {
        [Inject] private PhotonService    photonService;
        [Inject] private LifetimeSystem   lifetimeSystem;
        [Inject] private UiDynamicContext uiDynamicContext;

        [Inject] private Stash<ItemBoxComponent> itemBoxComponent;

        private ObjectPool<ItemBoxTimerUiDynamicData>            uiDynamicDataPool;
        private SystemStateProcessor<ItemBoxTimerStateComponent> processor;

        public override void OnAwake() {
            this.uiDynamicDataPool = new ObjectPool<ItemBoxTimerUiDynamicData>(
                () => new ItemBoxTimerUiDynamicData(this.lifetimeSystem.SceneLifetime),
                actionOnGet: data => this.uiDynamicContext.Add(data),
                actionOnRelease: data => this.uiDynamicContext.Remove(data));

            this.processor = this.World.Filter
                .With<ItemBoxComponent>()
                .With<ItemBoxTimerMarker>()
                .ToSystemStateProcessor(this.InitItemBoxTimer, this.CleanupItemBoxTimer);
        }

        public override void Dispose() {
            this.processor.Dispose();

            base.Dispose();
        }

        public override void OnUpdate(float deltaTime) {
            if (this.photonService.PredictedFrame is not { } f) {
                return;
            }

            this.processor.Process();

            foreach (var entity in this.processor.Entities) {
                ref var itemBox = ref this.itemBoxComponent.Get(entity);
                ref var state   = ref this.processor.States.Get(entity);

                state.Data.WorldPos = itemBox.position;
                state.Data.Progress = 1f - itemBox.timer / itemBox.time;
                state.Data.Timer    = (int)itemBox.timer;
            }
        }

        private ItemBoxTimerStateComponent InitItemBoxTimer(Entity entity) {
            var data = this.uiDynamicDataPool.Get();

            return new ItemBoxTimerStateComponent {
                Data = data,
            };
        }

        private void CleanupItemBoxTimer(ref ItemBoxTimerStateComponent state) {
            this.uiDynamicDataPool.Release(state.Data);
        }

        [Serializable, RequireFieldsInit]
        private struct ItemBoxTimerStateComponent : ISystemStateComponent {
            public ItemBoxTimerUiDynamicData Data;
        }
    }
}