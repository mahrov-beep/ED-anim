namespace Game.ECS.Systems.FloatingText {
    using System;
    using Components.Camera;
    using Core;
    using Multicast;
    using Player;
    using Quantum;
    using Scellecs.Morpeh;
    using Services.Photon;
    using Unit;
    using UnityEngine;
    using static Quantum.EDamageType;
    using SystemBase = Scellecs.Morpeh.SystemBase;

    public class FloatingTextSystem : SystemBase {
        [Inject] private PhotonService           photonService;
        [Inject] private LocalPlayerSystem       localPlayerSystem;
        [Inject] private QuantumEntityViewSystem quantumEntityViewSystem;
        [Inject] private FloatingTextConfig      floatingConfig;

        [Inject] private Stash<CinemachineBrainComponent> brainStash;

        private SingletonFilter<CinemachineBrainComponent> camera;

        private IDisposable onUnitDamage, onUnitHeal;

        public override void OnAwake() {
            camera = World.FilterSingleton<CinemachineBrainComponent>();

            onUnitHeal   = QuantumEvent.SubscribeManual<EventUnitHeal>(OnUnitHeal);
            onUnitDamage = QuantumEvent.SubscribeManual<EventUnitDamage>(OnUnitDamage);
        }

        public override void Dispose() {
            onUnitDamage.Dispose();
            onUnitHeal.Dispose();
        }

        public override void OnUpdate(float deltaTime) { }

        private void OnUnitHeal(EventUnitHeal callback) {
            if (localPlayerSystem.HasNotLocalEntityRef(out var localRef)) {
                return;
            }

            if (callback.targetRef == localRef) {
                return;
            }

            if (!IsEffectVisible(callback.targetRef)) {
                return;
            }

            if (!quantumEntityViewSystem.TryGetEntityView(callback.targetRef, out var view)) {
                return;
            }

            if (!camera.IsValid) {
                return;
            }

            var healNumber = floatingConfig.Heal.Spawn(
                            view.Transform.position + floatingConfig.OffsetHealFloating,
                            view.Transform);

            healNumber.faceCameraView = false;
            healNumber.transform.rotation = camera.Instance.Transform.rotation;
        }

        private void OnUnitDamage(EventUnitDamage callback) {
            if (localPlayerSystem.HasNotLocalEntityRef(out var localRef)) {
                return;
            }

            var attack = callback.attack;

            if (attack.SourceUnitRef != localRef) {
                return;
            }

            if (!IsEffectVisible(callback.targetRef)) {
                return;
            }

            if (!quantumEntityViewSystem.TryGetEntityView(callback.targetRef, out var view)) {
                return;
            }

            if (!camera.IsValid) {
                return;
            }

            var isCrit = (bool)callback.isCrit;

            var config = floatingConfig;
            var prefab = attack.DamageType switch {
                            Bullet => isCrit ? config.BulletDamageCritical : config.BulletDamage,
                            Melee => isCrit ? config.ExplosionDamageCritical : config.MeleeDamage,
                            Explosion => isCrit ? config.ExplosionDamageCritical : config.ExplosionDamage,
                            Fire => config.FireDamage,
                            // None impossible by odin validator
                            None => config.BulletDamage,
                            _ => config.BulletDamage,
            };

            var floating = prefab.Spawn();
            floating.faceCameraView = false;
            floating.transform.rotation = camera.Instance.Transform.rotation;
            floating.SetPosition(view.Transform.position + config.OffsetDamageFloating);
            floating.number = attack.Damage.AsFloat;
            floating.SetFollowedTarget(view.Transform);
        }

        private bool IsEffectVisible(EntityRef targetUnitRef) {
            if (!photonService.TryGetPredicted(out var f)) {
                return false;
            }

            if (localPlayerSystem.HasNotLocalEntityRef(out var localRef)) {
                return false;
            }

            if (targetUnitRef == localRef) {
                return true;
            }

            return LineOfSightHelper.HasLineSightFast(f, localRef, targetUnitRef);
        }
    }
}
