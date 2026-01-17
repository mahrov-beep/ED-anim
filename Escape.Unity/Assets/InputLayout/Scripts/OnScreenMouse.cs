using UnityEngine;
using UnityEngine.InputSystem;

namespace InputLayout.Scripts {

#if UNITY_EDITOR
    [UnityEditor.InitializeOnLoad]
#endif
    public class OnScreenMouse : Mouse {
        static OnScreenMouse() {
            InputSystem.RegisterLayout<OnScreenMouse>("OnScreenMouse");
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void InitializeInPlayer() { }
    }
}