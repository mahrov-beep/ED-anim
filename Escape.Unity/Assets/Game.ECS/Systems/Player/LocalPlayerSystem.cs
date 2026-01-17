namespace Game.ECS.Systems.Player {
    using System;
    using Components.Unit;
    using Core;
    using JetBrains.Annotations;
    using Multicast;
    using Quantum;
    using Scellecs.Morpeh;
    using Services.Photon;
    using UnityEngine;
    using SystemBase = Scellecs.Morpeh.SystemBase;

    public class LocalPlayerSystem : SystemBase {
        [Inject] private PhotonService photonService;

        [Inject] private Stash<UnitComponent>    stashUnitComponent;
        [Inject] private QuantumEntityViewSystem updater;

        private Filter unitFilter;

        public             EntityRef? LocalRef    { get; private set; }
        [CanBeNull] public Entity     LocalEntity { get; private set; }

        public override void OnAwake() {
            this.unitFilter = this.World.Filter
                            .With<UnitComponent>()
                            .Build();
        }

        public override void Dispose() {
            this.LocalRef = null;

            base.Dispose();
        }

        public override void OnUpdate(float deltaTime) {
            if (this.photonService.PredictedFrame is not { } f) {
                return;
            }

            if (this.LocalRef != null && !f.Exists(this.LocalRef.Value)) {
                this.LocalRef = null;
            }

            if (this.LocalRef == null) {
                foreach (var entity in this.unitFilter) {
                    var unit = this.stashUnitComponent.Get(entity);

                    if (f.TryGet(unit.EntityRef, out Unit u)) {
                        if (f.Context.IsLocalPlayer(u.PlayerRef)) {
                            this.LocalRef = unit.EntityRef;
                            LocalEntity   = entity;
                            OnSetLocalEntity(f, LocalRef.Value);
                        }
                    }
                }
            }
        }

        public bool HasNotLocalEntityRef(out EntityRef localRef) {
            localRef = EntityRef.None;
            if (LocalRef.HasValue == false) {
                return true;
            }

            localRef = LocalRef.Value;

            if (!photonService.TryGetPredicted(out var f)) {
                return true;
            }

            return false == f.Exists(localRef);
        }

        public bool IsLocalPlayerUnit(Entity unitEntity) {
            return stashUnitComponent.Has(unitEntity) &&
                            IsLocalPlayerUnit(in stashUnitComponent.Get(unitEntity));
        }

        public unsafe bool IsLocalPlayerUnit(in UnitComponent unitView) {
            if (!photonService.TryGetPredicted(out var f)) {
                return false;
            }

            var unit = f.GetPointer<Unit>(unitView.EntityRef);

            return f.Context.IsLocalPlayer(unit->PlayerRef);
        }

        private unsafe void OnSetLocalEntity(Frame f, EntityRef localRef) {
            var localUnit = f.GetPointer<Unit>(localRef);

            SetupExitZones(f, localUnit);
        }

        private unsafe void SetupExitZones(Frame f, Unit* localUnit) {
            foreach (var pair in f.GetComponentIterator<ExitZone>()) {
                var exitZoneRef = pair.Entity;

                if (exitZoneRef == localUnit->TargetExitZone) {
                    continue;
                }

                if (!updater.TryGetEntityView(exitZoneRef, out var view)) {
                    Debug.LogError($"ExitZone {exitZoneRef}: No view found");
                    continue;
                }

                view.gameObject.SetActive(false);
            }
        }
    }
}