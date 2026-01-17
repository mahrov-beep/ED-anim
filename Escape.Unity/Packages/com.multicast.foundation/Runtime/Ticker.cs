namespace Multicast {
    using UniMob;
    using UnityEngine;

    public class Ticker : MonoBehaviour {
        private static readonly MutableAtom<int> FrameTicker = Atom.Value(0);

        private static Ticker instance;

        public static void TickEveryFrame() => FrameTicker.Get();

        [RuntimeInitializeOnLoadMethod]
        private static void CreateTicker() {
            if (instance != null) {
                return;
            }

            var go = new GameObject(nameof(Ticker));

            instance = go.AddComponent<Ticker>();

            DontDestroyOnLoad(go);
            DontDestroyOnLoad(instance);
        }

        private void Update() {
            FrameTicker.Invalidate();
        }

        private void OnDestroy() {
            instance = null;
        }
    }
}