namespace Game.ECS.Systems.Unit {
    using Components.ItemBox;
    using Multicast;
    using Quantum;
    using Scellecs.Morpeh;
    using Services.Photon;
    using SystemBase = Scellecs.Morpeh.SystemBase;

    public class ItemBoxTimerSystem : SystemBase {
        [Inject] private PhotonService             photonService;
        [Inject] private Stash<ItemBoxComponent>   itemBoxStash;
        [Inject] private Stash<ItemBoxTimerMarker> itemBoxTimerStash;

        private Filter itemBoxFilter;

        public override void OnAwake() {
            this.itemBoxFilter = this.World.Filter.With<ItemBoxComponent>().Build();
        }
        
        public override void OnUpdate(float deltaTime) {
            if (this.photonService.PredictedFrame is not { } f) {
                return;
            }
            
            foreach (var entity in this.itemBoxFilter) {
                ref var itemBoxComponent = ref this.itemBoxStash.Get(entity);

                if (!f.TryGet<ItemBox>(itemBoxComponent.quantumEntityView.EntityRef, out var itemBox)) {
                    continue;
                }
                
                if (!f.TryGet<Transform3D>(itemBoxComponent.quantumEntityView.EntityRef, out var transform3D)) {
                    continue;
                }
                
                if (!f.TryGet<TimerItemBoxMarker>(itemBoxComponent.quantumEntityView.EntityRef, out var timerMarker)) {
                    if (this.itemBoxTimerStash.Has(entity)) {
                        this.itemBoxTimerStash.Remove(entity);
                    }
                    
                    continue;
                }
                
                if (!this.itemBoxTimerStash.Has(entity)) {
                    this.itemBoxTimerStash.Add(entity);
                }
                
                itemBoxComponent.time  = itemBox.TimeToOpen.AsFloat;
                itemBoxComponent.timer = itemBox.TimerToOpen.AsFloat;
                itemBoxComponent.position  = transform3D.Position.ToUnityVector3();
            }
        }
    }
}