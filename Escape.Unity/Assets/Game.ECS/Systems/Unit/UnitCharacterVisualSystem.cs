namespace Game.ECS.Systems.Unit {
    using Components.Unit;
    using Domain;
    using Multicast;
    using Player;
    using Quantum;
    using Scellecs.Morpeh;
    using Services.Photon;
    using SoundEffects;
    using SystemBase = Scellecs.Morpeh.SystemBase;

    public class UnitCharacterVisualSystem : SystemBase {
        [Inject] private PhotonService photonService;

        [Inject] private Stash<UnitComponent>        unitComponent;
        [Inject] private Stash<LocalCharacterMarker> unitLocalMarker;

        private Filter unitFilter;

        public override void OnAwake() {
            this.unitFilter = this.World.Filter
                .With<UnitComponent>()
                .Build();
        }

        public override void OnUpdate(float deltaTime) {
            if (this.photonService.PredictedFrame is not { } f) {
                return;
            }

            var hasLocalChanging = false;

            foreach (var entity in this.unitFilter) {
                ref var unit = ref this.unitComponent.Get(entity);

                var isLocal = unitLocalMarker.Has(entity);

                foreach (var visual in unit.visuals) {
                    if (visual == null) {
                        continue;
                    }

                    var unitEntity = unit.EntityRef;

                    if (!f.TryGet(unitEntity, out CharacterLoadout loadout)) {
                        continue;
                    }

                    foreach (var slot in CharacterLoadoutSlotsExtension.AllValidSlots) {
                        var itemEntity = loadout.ItemAtSlot(slot);

                        if (itemEntity == EntityRef.None) {
                            if (isLocal) {
                                if (visual.IsNotEmpty(EnumNames<CharacterLoadoutSlots>.GetName(slot))) {
                                    hasLocalChanging = true;
                                }
                            }

                            visual.SetVisual(EnumNames<CharacterLoadoutSlots>.GetName(slot), null);

                            continue;
                        }

                        var item      = f.Get<Item>(itemEntity);
                        var itemAsset = f.FindAsset(item.Asset);

                        if (isLocal) {
                            if (!visual.IsExisting(EnumNames<CharacterLoadoutSlots>.GetName(slot), itemAsset.ItemKey)) {
                                hasLocalChanging = true;
                            }
                        }

                        visual.SetVisual(EnumNames<CharacterLoadoutSlots>.GetName(slot), itemAsset.ItemKey);
                    }
                }
            }

            if (hasLocalChanging) {
                App.Get<ISoundEffectService>()?.PlayOneShot(CoreConstants.SoundEffectKeys.Equip);
            }
        }
    }
}