namespace InfimaGames.LowPolyShooterPack {
    using System;
    using System.Collections.Generic;
    using Multicast.Pools;
    using Sirenix.OdinInspector;
    using UnityEngine;
    using Random = UnityEngine.Random;

    public class Projectile : MonoBehaviour {
        [SerializeField, ChildGameObjectsOnly]
        [LabelText("Bullet [?]")]
        [PropertyTooltip("Дочерний объект пули; скрывается по таймеру во время полёта.")]
        public GameObject bullet;

        [SerializeField] public float speed    = 100f;
        [SerializeField] public float lifetime = 0.05f;

        [Serializable]
        private class LayerImpactConfig {
            [Tooltip("Слои, для которых применяется этот набор эффектов.")]
            public LayerMask layers = ~0;
            [Tooltip("Префаб декали (дырки) для этих слоёв.")]
            public GameObject decalPrefab;
            [Tooltip("Префаб эффекта попадания (дым/частицы) для этих слоёв.")]
            public GameObject impactPrefab;
            [Tooltip("Случайное смещение от поверхности при установке декали.")]
            public Vector2 decalOffsetRange = new(0.01f, 0.03f);
            [Tooltip("Масштаб декали.")]
            public float decalScale = 1f;
            [Tooltip("Масштаб эффекта попадания.")]
            public float impactScale = 1f;
            [Tooltip("Случайный поворот декали вокруг нормали.")]
            public bool randomizeDecalRotation = true;
            [Tooltip("Привязывать декаль к объекту попадания.")]
            public bool parentDecalToHit = true;
        }

        [TitleGroup("Impact VFX by Layer")]
        [SerializeField, Tooltip("Таблица: слой -> эффекты и размеры.")]
        private List<LayerImpactConfig> layerImpactConfigs = new();

        private LayerMask aggregatedHitMask;

        [NonSerialized] private Vector3 hitPoint;
        [NonSerialized] private Vector3 origin;
        [NonSerialized] private Vector3? hitNormal;
        [NonSerialized] private float remainingLifetime;
        [NonSerialized] private float bulletLifetime;
        [NonSerialized] private bool impactSpawned;

        private void Awake() {
            aggregatedHitMask = 0;
            if (layerImpactConfigs != null) {
                foreach (var cfg in layerImpactConfigs) {
                    if (cfg == null) {
                        continue;
                    }
                    aggregatedHitMask |= cfg.layers;
                }
            }
        }

        public void Setup(Vector3 origin, Vector3 hitPoint, Vector3? hitNormal) {
            this.origin            = origin;
            this.hitPoint          = hitPoint;
            this.hitNormal         = hitNormal;
            this.remainingLifetime = this.lifetime;
            this.bulletLifetime    = Mathf.Min(this.lifetime, Vector3.Distance(this.transform.position, hitPoint) / Mathf.Max(this.speed, 0.1f));
            this.impactSpawned     = false;

            if (this.bullet) {
                this.bullet.SetActive(true);
            }
        }

        private void OnDrawGizmos() {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, this.hitPoint);
        }

        private void Update() {
            this.transform.position = Vector3.MoveTowards(this.transform.position, this.hitPoint, this.speed * Time.deltaTime);

            this.remainingLifetime -= Time.deltaTime;

            if (this.bulletLifetime > 0f) {
                this.bulletLifetime -= Time.deltaTime;

                if (this.bulletLifetime <= 0f && this.bullet) {
                    this.bullet.SetActive(false);
                }
            }

            var reachedHitPoint = (this.transform.position - this.hitPoint).sqrMagnitude <= 0.0001f;
            if (reachedHitPoint && !this.impactSpawned) {
                this.SpawnImpact();
            }

            if (this.remainingLifetime <= 0f) {
                this.SpawnImpact();
                GameObjectPool.Destroy(this.gameObject);
            }
        }

        private void SpawnImpact() {
            if (this.impactSpawned) {
                return;
            }
            this.impactSpawned = true;

            var direction    = this.hitPoint - this.origin;
            var hasDirection = direction.sqrMagnitude > 0.0001f;
            var targetNormal = this.hitNormal ?? Vector3.zero;
            Transform hitParent = null;

            var sampleOrigin = hasDirection ? this.origin : this.transform.position;
            var maxDistance  = hasDirection ? direction.magnitude + 0.05f : 0.05f;

            if (aggregatedHitMask.value == 0) {
                return;
            }

            LayerImpactConfig config = null;

            if (hasDirection && Physics.Raycast(sampleOrigin, direction.normalized, out var hitInfo, maxDistance, aggregatedHitMask, QueryTriggerInteraction.Ignore)) {
                this.hitPoint = hitInfo.point;
                targetNormal  = hitInfo.normal;
                hitParent     = hitInfo.collider.transform;
                config        = this.ResolveConfig(hitParent.gameObject.layer);
                if (config == null) {
                    return;
                }
            }
            else {
                return;
            }

            if (targetNormal == Vector3.zero && hasDirection) {
                targetNormal = -direction.normalized;
            }
            if (targetNormal == Vector3.zero) {
                targetNormal = Vector3.up;
            }

            var activeDecalPrefab       = config.decalPrefab;
            var activeImpactPrefab      = config.impactPrefab;
            var activeOffsetRange       = config.decalOffsetRange;
            var activeDecalScale        = config.decalScale;
            var activeImpactScale       = config.impactScale;
            var activeRandomizeDecalRot = config.randomizeDecalRotation;
            var activeParentDecalToHit  = config.parentDecalToHit;

            if (activeDecalPrefab != null) {
                var offset      = Random.Range(activeOffsetRange.x, activeOffsetRange.y);
                var decalObject = GameObjectPool.Instantiate(activeDecalPrefab, this.hitPoint + targetNormal * offset, Quaternion.LookRotation(targetNormal));

                if (activeRandomizeDecalRot) {
                    decalObject.transform.rotation = Quaternion.AngleAxis(Random.Range(0f, 360f), targetNormal) * decalObject.transform.rotation;
                }

                decalObject.transform.localScale *= activeDecalScale;

                if (activeParentDecalToHit && hitParent != null) {
                    decalObject.transform.SetParent(hitParent, true);
                }
            }

            if (activeImpactPrefab != null) {
                var impactObject = GameObjectPool.Instantiate(activeImpactPrefab, this.hitPoint, Quaternion.LookRotation(targetNormal));
                if (!Mathf.Approximately(activeImpactScale, 1f)) {
                    impactObject.transform.localScale *= activeImpactScale;
                }

                if (hitParent != null) {
                    impactObject.transform.SetParent(hitParent, true);
                }
            }
        }

        private LayerImpactConfig ResolveConfig(int layer) {
            if (layerImpactConfigs == null) {
                return null;
            }

            foreach (var cfg in layerImpactConfigs) {
                if (cfg == null) {
                    continue;
                }

                if ((cfg.layers.value & (1 << layer)) != 0) {
                    return cfg;
                }
            }

            return null;
        }
    }
}