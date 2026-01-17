namespace InfimaGames.LowPolyShooterPack {
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.Pool;

    public abstract class AttachmentCollection<T> : MonoBehaviour where T : MonoBehaviour {
        private readonly Dictionary<GameObject, T> prefabToBehaviourMapping = new();
        private readonly Dictionary<string, T>     nameToBehaviourMapping   = new();

        private void Awake() {
            using (ListPool<T>.Get(out var elements)) {
                this.GetComponentsInChildren(includeInactive: true, elements);

                foreach (var el in elements) {
                    this.nameToBehaviourMapping[el.name] = el;

                    el.gameObject.SetActive(false);
                }
            }
        }

        public T GetByPrefab(GameObject prefab) {
            if (this.prefabToBehaviourMapping.TryGetValue(prefab, out var behaviour)) {
                return behaviour;
            }

            return this.prefabToBehaviourMapping[prefab] = this.GetByName(prefab.name);
        }

        public T GetByName(string elName) {
            return this.nameToBehaviourMapping.GetValueOrDefault(elName, null);
        }
    }
}