namespace Game.ECS.Systems.Grenade {
    using System.Collections.Generic;
    using Components.Unit;
    using Core;
    using Multicast;
    using Quantum;
    using Scellecs.Morpeh;
    using Services.Photon;
    using UnityEngine;
    using SystemBase = Scellecs.Morpeh.SystemBase;

    public unsafe class GrenadeTrackingSystem : SystemBase {
        [Inject] private readonly PhotonService photonService;
        [Inject] private readonly QuantumEntityViewSystem quantumEntityViewSystem;
        [Inject] private readonly Stash<GrenadeComponent> grenadeStash;
        [Inject] private readonly Stash<GrenadeMarker> grenadeMarkerStash;

        private Filter grenadeFilter;
        private readonly Dictionary<EntityRef, Entity> trackedGrenades = new(16);

        private const float DEFAULT_EXPLOSION_RADIUS = 4f;

        public override void OnAwake() {
            grenadeFilter = World.Filter
                .With<GrenadeComponent>()
                .Build();
        }

        public override void Dispose() {
            trackedGrenades.Clear();
            grenadeStash.RemoveAll();
            grenadeMarkerStash.RemoveAll();
        }

        public override void OnUpdate(float deltaTime) {
            if (!photonService.TryGetPredicted(out var frame)) {
                return;
            }

            UpdateExistingGrenades(frame);
            TrackNewGrenadeProjectiles(frame);
            TrackActiveGrenadeZones(frame);
        }

        #region Update Existing

        private void UpdateExistingGrenades(Frame frame) {
            foreach (var entity in grenadeFilter) {
                ref var grenadeComponent = ref grenadeStash.Get(entity);
                var entityRef = grenadeComponent.EntityRef;

                if (!IsGrenadeViewValid(entityRef, ref grenadeComponent)) {
                    RemoveGrenadeEntity(entity, entityRef);
                    continue;
                }

                UpdateGrenadePosition(frame, ref grenadeComponent, entityRef);
                EnsureViewAssigned(entityRef, ref grenadeComponent);
            }
        }

        private bool IsGrenadeViewValid(EntityRef entityRef, ref GrenadeComponent component) {
            if (component.quantumEntityView && component.quantumEntityView.gameObject) {
                return true;
            }

            if (quantumEntityViewSystem.TryGetEntityView(entityRef, out var view) && view) {
                component.quantumEntityView = view;
                return true;
            }

            return false;
        }

        private void UpdateGrenadePosition(Frame frame, ref GrenadeComponent component, EntityRef entityRef) {
            if (frame.TryGetPointer(entityRef, out Transform3D* transform)) {
                component.PositionView = transform->Position.ToUnityVector3();
            }             
            else if (component.quantumEntityView && component.quantumEntityView.transform) {
                component.PositionView = component.quantumEntityView.transform.position;
            }
        }

        private void EnsureViewAssigned(EntityRef entityRef, ref GrenadeComponent component) {
            if (!component.quantumEntityView) {
                if (quantumEntityViewSystem.TryGetEntityView(entityRef, out var view)) {
                    component.quantumEntityView = view;
                }
            }
        }

        #endregion

        #region Track Projectiles

        private void TrackNewGrenadeProjectiles(Frame frame) {
            foreach (var (unitRef, unit) in frame.GetComponentIterator<Unit>()) {
                var projectileRef = unit.ActiveAbilityInfo.AbilityEffectRef;

                if (!projectileRef.IsValid) {
                    continue;
                }               

                if (!ShouldTrackProjectile(frame, projectileRef, unit.AbilityRef, unitRef)) {
                    continue;
                }

                var explosionRadius = TryGetExplosionRadiusFromAbility(frame, unit.AbilityRef);
                TryCreateGrenadeIndicator(frame, projectileRef, explosionRadius); 
            }
        }

        private bool ShouldTrackProjectile(Frame frame, EntityRef projectileRef, EntityRef abilityRef, EntityRef unitRef) {
            if (!projectileRef.IsValid || trackedGrenades.ContainsKey(projectileRef)) {
                return false;
            }

            if (!abilityRef.IsValid || !frame.Exists(abilityRef)) {               
                return false;
            }
            
            if (!frame.Has<Ability>(abilityRef)) {               
                return false;
            }

            var ability = frame.Get<Ability>(abilityRef);
            var abilityItem = frame.FindAsset<AbilityItemAsset>(ability.Config.Id);
            
            if (abilityItem == null) {             
                return false;
            }
          
            if (abilityItem is not GrenadeAbilityItemBase) {               
                return false;
            }

            var hasView = quantumEntityViewSystem.TryGetEntityView(projectileRef, out var view);    
            
            if (!hasView || !view) {               
                return false;
            }
           
            return true;
        }

        private float TryGetExplosionRadiusFromAbility(Frame frame, EntityRef abilityRef) {
            if (!frame.Has<Ability>(abilityRef)) {
                return DEFAULT_EXPLOSION_RADIUS;
            }

            var ability = frame.Get<Ability>(abilityRef);
            var abilityItem = frame.FindAsset<GrenadeAbilityItemBase>(ability.Config.Id);
            
            return DEFAULT_EXPLOSION_RADIUS;
        }

        #endregion

        #region Track Active Zones

        private void TrackActiveGrenadeZones(Frame frame) {
            foreach (var (attackRef, attack) in frame.GetComponentIterator<Attack>()) {
                if (trackedGrenades.ContainsKey(attackRef)) {
                    continue;
                }

                var attackData = frame.FindAsset<AttackData>(attack.AttackData.Id);
                if (attackData == null) {
                    continue;
                }
               
                if (attackData is not AttackData) {
                    continue;
                }               

                var explosionRadius = CalculateExplosionRadius(attackData);
                TryCreateGrenadeIndicator(frame, attackRef, explosionRadius);
            }
        }

        #endregion

        #region Entity Management

        private void TryCreateGrenadeIndicator(Frame frame, EntityRef quantumEntityRef, float explosionRadius) {
            if (!TryGetEntityData(frame, quantumEntityRef, out var position, out var view)) {               
                return;
            }

            CreateGrenadeEntity(quantumEntityRef, view, position, explosionRadius);
        }

        private bool TryGetEntityData(Frame frame, EntityRef entityRef, out Vector3 position, out QuantumEntityView view) {
            position = Vector3.zero;
            view = null;

            if (frame.TryGetPointer(entityRef, out Transform3D* transform)) {
                position = transform->Position.ToUnityVector3();                
                quantumEntityViewSystem.TryGetEntityView(entityRef, out view);  
                return true;
            }

            if (quantumEntityViewSystem.TryGetEntityView(entityRef, out view) && view) {
                position = view.transform.position;
                return true;
            }
            
            return false;
        }

        private void CreateGrenadeEntity(EntityRef quantumEntityRef, QuantumEntityView view, Vector3 position, float explosionRadius) {
            var entity = World.CreateEntity();

            ref var component = ref grenadeStash.Add(entity);
            component.EntityRef = quantumEntityRef;
            component.PositionView = position;
            component.quantumEntityView = view;
            component.ExplosionRadius = explosionRadius;

            grenadeMarkerStash.Add(entity);
            trackedGrenades[quantumEntityRef] = entity;
        }

        private void RemoveGrenadeEntity(Entity entity, EntityRef quantumEntityRef) {
            trackedGrenades.Remove(quantumEntityRef);
            World.RemoveEntity(entity);
        }

        #endregion

        #region Validation

        private static bool IsEntityValid(Frame frame, EntityRef entityRef) {
            return entityRef.IsValid && 
                   frame.Exists(entityRef) && 
                   frame.Has<Transform3D>(entityRef);
        }

        private static float CalculateExplosionRadius(AttackData attackData) {
            return attackData switch {
                AreaOfEffectAttackData aoeData => aoeData.radius.AsFloat,
                PersistentAreaOfEffectAttackData persistentAoeData => persistentAoeData.radius.AsFloat,
                _ => DEFAULT_EXPLOSION_RADIUS
            };
        }

        #endregion
    }
}

