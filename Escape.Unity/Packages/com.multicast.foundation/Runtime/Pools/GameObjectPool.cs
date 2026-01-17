namespace Multicast.Pools {
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using JetBrains.Annotations;
    using UnityEngine;
    using Debug = UnityEngine.Debug;
    using Object = UnityEngine.Object;

    public static class GameObjectPool {
        private static readonly Dictionary<int, Pool> Pools = new Dictionary<int, Pool>();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Initialize() {
            Pools.Clear();
        }

        public static Pool GetPool([NotNull] GameObject prefab) {
            if (prefab == null) {
                throw new ArgumentNullException(nameof(prefab));
            }

            var prefabID = prefab.GetInstanceID();

            Pools.TryGetValue(prefabID, out var pool);

            if (pool != null) {
                return pool;
            }

            pool = new GameObject("Pool").AddComponent<Pool>();
            pool.Init(prefab);
            Pools[prefabID] = pool;

            return pool;
        }

        public static GameObject Instantiate([NotNull] GameObject prefab, Vector3 position, Quaternion rotation, Transform parent = null) {
            if (prefab == null) {
                throw new ArgumentNullException(nameof(prefab));
            }

            var prefabID = prefab.GetInstanceID();
            var pool     = GetPool(prefab);
            var obj      = pool.Get(position, rotation, parent);

            var poolID = obj.GetComponent<PoolID>() ?? obj.AddComponent<PoolID>();
            poolID.PrefabInstanceID = prefabID;

            return obj;
        }

        public static void Destroy(GameObject obj, bool deactivate = true) {
            if (obj == null) {
                Debug.LogWarning("[GameObjectPool] object is null");
                return;
            }

            var poolID = obj.GetComponent<PoolID>();
            if (poolID == null) {
                Debug.LogError("[GameObjectPool] PoolID component not attached", obj);
                Object.Destroy(obj);
                return;
            }

            if (poolID.PrefabInstanceID == 0 || poolID.ObjectDestroyed) {
                return;
            }

            if (!Pools.TryGetValue(poolID.PrefabInstanceID, out var pool)) {
                Debug.LogError("[GameObjectPool] pool for object not exists", obj);
                Object.Destroy(obj);
                return;
            }

            poolID.PrefabInstanceID = 0;
            pool.Return(obj, deactivate);
        }

        public sealed class Pool : MonoBehaviour {
            private readonly Stack<GameObject> stack = new Stack<GameObject>();

            private GameObject prefab;
            private bool       poolDestroyed;

            private void OnDestroy() {
                this.poolDestroyed = true;
            }

            public void Init([NotNull] GameObject instancePrefab) {
                if (instancePrefab == null) {
                    throw new ArgumentNullException(nameof(instancePrefab));
                }

                this.prefab = instancePrefab;

                this.EditorUpdateName();
            }

            public GameObject Get(Vector3 position, Quaternion rotation, Transform parent) {
                if (this.poolDestroyed) {
                    throw new InvalidOperationException("Cannot get object from destroyed pool");
                }

                GameObject obj = null;

                while (this.stack.Count > 0) {
                    obj = this.stack.Pop();

                    if (obj == null) {
                        continue;
                    }

                    obj.transform.SetParent(parent);
                    obj.transform.SetPositionAndRotation(position, rotation);
                    break;
                }

                if (obj == null) {
                    obj = Object.Instantiate(this.prefab, position, rotation, parent);
                }

                if (!obj.activeSelf) {
                    obj.SetActive(true);
                }

                this.EditorUpdateName();

                return obj;
            }

            public void Return(GameObject obj, bool deactivate) {
                if (this.poolDestroyed) {
                    return;
                }

                if (obj == null) {
                    return;
                }

                if (deactivate) {
                    obj.SetActive(false);
                }

                obj.transform.SetParent(this.transform);

                this.stack.Push(obj);

                this.EditorUpdateName();
            }

            [Conditional("UNITY_EDITOR")]
            private void EditorUpdateName() {
#if UNITY_EDITOR
                this.name = $"{this.prefab.name} Pool ({this.stack.Count})";
#endif
            }
        }
    }
}