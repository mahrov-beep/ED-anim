namespace _Project.Scripts.Scopes {
    using System;
    using UnityEngine;

    public sealed class GameObjectActiveScope : IDisposable {
        private readonly GameObject go;

        private readonly bool prev;

        public GameObjectActiveScope(GameObject gameObject, bool active = true) {
            go   = gameObject;
            prev = go.activeSelf;

            if (prev != active) {
                go.SetActive(active);
            }
        }

        public void Dispose() {
            if (go && go.activeSelf != prev) {
                go.SetActive(prev);
            }
        }
    }
}