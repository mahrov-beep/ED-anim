namespace Game.ECS.Systems.Attack {
    using System;
    using System.Collections.Generic;
    using Components.WorldView;
    using Multicast;
    using Multicast.Pools;
    using Quantum;
    using Scellecs.Morpeh;
    using Services.Photon;
    using UnityEngine;
    using SystemBase = Scellecs.Morpeh.SystemBase;

    public class AttackVisualSystem : SystemBase {
        [Inject] private PhotonService photonService;
        [Inject] private Stash<WorldVFXRoot> vfxRootStash;

        private Filter vfxRootFilter;

        private IDisposable attackPerformedSubscribe;
        private IDisposable attackHitSubscribe;

        private struct VisualInfo {
            public float destroyTime;
            public float fadeStartTime;
            public float fadeDurationSeconds;
            public GameObject obj;
            public ParticleSystem[] particleSystems;
            public float[] originalEmissionRates;
        }

        private readonly List<VisualInfo> aliveVisual = new List<VisualInfo>(64);
        private const float DefaultFadeOutSeconds = 2.0f;

        public override void OnAwake() {
            this.vfxRootFilter = this.World.Filter
                .With<WorldVFXRoot>()
                .Build();

            this.aliveVisual.Clear();
            this.attackPerformedSubscribe = QuantumEvent.SubscribeManual<EventAttackPerformedSynced>(this.OnAttackPerformedVerified);
            this.attackHitSubscribe = QuantumEvent.SubscribeManual<EventAttackHitSynced>(this.OnAttackHitVerified);
        }

        public override void Dispose() {
            this.attackHitSubscribe.Dispose();
            this.attackPerformedSubscribe.Dispose();

            for (int i = 0; i < this.aliveVisual.Count; i++) {
                var v = this.aliveVisual[i];
                if (v.obj) {
                    GameObjectPool.Destroy(v.obj);
                }
            }

            this.aliveVisual.Clear();
        }

        public override void OnUpdate(float deltaTime) {
            var nowTime = Time.time;

            for (var i = 0; i < this.aliveVisual.Count; i++) {
                var info = this.aliveVisual[i];

                if (!info.obj) {
                    continue;
                }

                if (info.fadeDurationSeconds > 0f && nowTime >= info.fadeStartTime && info.particleSystems != null) {
                    var t = Mathf.Clamp01((nowTime - info.fadeStartTime) / info.fadeDurationSeconds);

                    for (int p = 0; p < info.particleSystems.Length; p++) {
                        var ps = info.particleSystems[p];
                        if (ps == null) continue;
                        var emission = ps.emission;
                        var original = (info.originalEmissionRates != null && p < info.originalEmissionRates.Length)
                            ? info.originalEmissionRates[p]
                            : emission.rateOverTimeMultiplier;
                        emission.rateOverTimeMultiplier = Mathf.Lerp(original, 0f, t);
                    }
                }

                if (nowTime > info.destroyTime) {
                    GameObjectPool.Destroy(info.obj);
                    this.aliveVisual[i] = default;
                }
            }

            this.aliveVisual.RemoveAll(static it => !it.obj);
        }

        private void OnAttackHitVerified(EventAttackHitSynced callback) {
            var attackData = callback.attackAsset;

            if (!attackData.attackHitFX) {
                return;
            }

            var hitFadeSec = attackData.attackHitFadeSeconds.AsFloat;
            var fade = attackData.attackHitSmoothFade
                ? (hitFadeSec > 0f ? hitFadeSec : DefaultFadeOutSeconds)
                : 0f;
            this.SpawnVisual(attackData.attackHitFX, attackData.attackHitFXLifetimeSec.AsFloat, callback.hitPoint.ToUnityVector3(), Quaternion.identity, fade);
        }

        private void OnAttackPerformedVerified(EventAttackPerformedSynced callback) {
            var attackData = callback.attackAsset;

            if (!attackData.attackPerformedFX) {
                return;
            }

            var performedFadeSec = attackData.attackPerformedFadeSeconds.AsFloat;
            var fade = attackData.attackPerformedSmoothFade
                ? (performedFadeSec > 0f ? performedFadeSec : DefaultFadeOutSeconds)
                : 0f;
            this.SpawnVisual(attackData.attackPerformedFX, attackData.attackPerformedFXLifetimeSec.AsFloat, callback.position.ToUnityVector3(), Quaternion.identity, fade);
        }

        private void SpawnVisual(GameObject prefab, float lifetimeSeconds, Vector3 position, Quaternion rotation, float fadeOutSeconds) {
            var root = this.vfxRootFilter.IsEmpty() == false
                ? this.vfxRootStash.Get(this.vfxRootFilter.First()).Root
                : null;

#if UNITY_EDITOR
            if (root == null) {
                Debug.LogWarning($"Не задан {nameof(WorldVFXRootProvider)} на сцене");
            }
#endif

            var obj = GameObjectPool.Instantiate(prefab, position, rotation, root);

            var psArray = obj.GetComponentsInChildren<ParticleSystem>(true);
            float[] originalEmission = null;

            float maxStartLifetime = 0f;
            if (psArray != null && psArray.Length > 0) {
                originalEmission = new float[psArray.Length];
                for (int i = 0; i < psArray.Length; i++) {
                    var ps = psArray[i];
                    var emission = ps.emission;
                    originalEmission[i] = emission.rateOverTimeMultiplier;

                    var main = ps.main;
                    var lifetimeCurve = main.startLifetime;
                    maxStartLifetime = Mathf.Max(maxStartLifetime, lifetimeCurve.constantMax);
                }
            }

            var now = Time.time;
            var fadeS = Mathf.Max(0f, fadeOutSeconds);
            var infoToAdd = new VisualInfo {
                obj = obj,
                particleSystems = psArray,
                originalEmissionRates = originalEmission,
                fadeStartTime = fadeS > 0f ? now + Mathf.Max(0f, lifetimeSeconds - fadeS) : float.MaxValue,
                fadeDurationSeconds = fadeS,
                destroyTime = now + lifetimeSeconds + maxStartLifetime
            };

            this.aliveVisual.Add(infoToAdd);
        }
    }
}