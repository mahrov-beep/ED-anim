namespace Multicast.Modules.UserData {
    using System;
    using UnityEngine;

    internal class UserDataEvents : MonoBehaviour {
        public Action OnApplicationPaused { get; set; }

        private void OnApplicationPause(bool paused) {
            this.OnApplicationPaused?.Invoke();
        }

        private void OnApplicationQuit() {
            this.OnApplicationPaused?.Invoke();
        }
    }
}