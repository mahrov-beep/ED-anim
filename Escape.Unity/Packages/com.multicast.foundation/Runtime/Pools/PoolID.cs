namespace Multicast.Pools {
    using UnityEngine;

    public sealed class PoolID : MonoBehaviour {
        [SerializeField] private int prefabInstanceID;

        // ReSharper disable once ConvertToAutoProperty
        public int PrefabInstanceID {
            get => this.prefabInstanceID;
            set => this.prefabInstanceID = value;
        }

        public bool ObjectDestroyed { get; private set; }

        private void OnDestroy() {
            this.ObjectDestroyed = true;
        }
    }
}