namespace Multicast.Unity {
    using UnityEngine;

    public sealed class DontDestroyOnLoad : MonoBehaviour {
        private void Awake() {
            DontDestroyOnLoad(this.gameObject);
        }
    }
}