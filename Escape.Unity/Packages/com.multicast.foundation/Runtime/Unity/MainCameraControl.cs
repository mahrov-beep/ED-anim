namespace Multicast.Unity {
    using System.Collections.Generic;
    using UnityEngine;

    public static class MainCameraControl {
        private static readonly List<object> Disablers = new List<object>();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Init() {
            if (Disablers.Count > 0) {
                Debug.LogError("MainCameraControl.Disables is not empty");
                Disablers.Clear();
            }
        }

        private static Camera           camera;
        private static int              cullingMask;
        private static CameraClearFlags clearFlags;
        private static Color            backgroundColor;

        public static void AddDisabler(object disabler) {
            if (Disablers.Contains(disabler)) {
                return;
            }

            if (Disablers.Count == 0 && (camera = Camera.main) != null) {
                cullingMask     = camera.cullingMask;
                clearFlags      = camera.clearFlags;
                backgroundColor = camera.backgroundColor;

                camera.cullingMask     = 0;
                camera.clearFlags      = CameraClearFlags.SolidColor;
                camera.backgroundColor = Color.black;
            }

            Disablers.Add(disabler);
        }

        public static void RemoveDisabler(object disabler) {
            if (!Disablers.Contains(disabler)) {
                return;
            }

            Disablers.Remove(disabler);

            if (Disablers.Count == 0 && camera != null) {
                camera.cullingMask     = cullingMask;
                camera.clearFlags      = clearFlags;
                camera.backgroundColor = backgroundColor;

                camera = null;
            }
        }
    }
}