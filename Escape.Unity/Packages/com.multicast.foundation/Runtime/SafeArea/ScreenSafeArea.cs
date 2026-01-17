namespace Multicast.SafeArea {
    using UnityEngine;
#if UNITY_EDITOR
    using UnityEditor;

#endif

    public static class ScreenSafeArea {
        public static BannerPlacementType BannerPlacement = BannerPlacementType.None;

        public static Rect FullArea => new Rect(0, 0, Screen.width, Screen.height);

        public static Rect SafeArea => GetSafeArea();

        private static Rect GetSafeArea() {
            if (Application.isPlaying) {
                return Screen.safeArea;
            }

            var area = new Rect(0, 0, Screen.width, Screen.height);

#if UNITY_EDITOR
            if (Application.isEditor && SimulatedDevice != SimDevice.None) {
                var nsa        = new Rect(0, 0, Screen.width, Screen.height);
                var isPortrait = Screen.height > Screen.width;

                switch (SimulatedDevice) {
                    case SimDevice.iPhoneX:
                        nsa = isPortrait ? iPhoneXSafeArea[0] : iPhoneXSafeArea[1];
                        break;
                }

                area = new Rect(Screen.width * nsa.x, Screen.height * nsa.y, Screen.width * nsa.width,
                    Screen.height * nsa.height);
            }
#endif

            return area;
        }

#if UNITY_EDITOR
        public enum SimDevice {
            None    = 0,
            iPhoneX = 1,
        }

        private static SimDevice SimulatedDevice {
            get => (SimDevice) EditorPrefs.GetInt("ScreenSafeArea.simulatedDevice", (int) SimDevice.None);
            set => EditorPrefs.SetInt("ScreenSafeArea.simulatedDevice", (int) value);
        }

        private static readonly Rect[] iPhoneXSafeArea = {
            new Rect(0f, 102f / 2436f, 1f, 2202f / 2436f),                     // Portrait
            new Rect(132f / 2436f, 63f / 1125f, 2172f / 2436f, 1062f / 1125f), // Landscape
        };

        public static bool CheckSimulated(string path, SimDevice simDevice) {
            Menu.SetChecked(path, simDevice == SimulatedDevice);
            return true;
        }

        public static void SetSimulated(SimDevice simDevice) {
            SimulatedDevice = simDevice;
        }

        private const string NONE_MENU_ITEM    = "Edit/Screen SafeArea Emulation/None";
        private const string IPHONEX_MENU_ITEM = "Edit/Screen SafeArea Emulation/iPhone X";

        [MenuItem(NONE_MENU_ITEM, true)]
        private static bool CheckNone() => CheckSimulated(NONE_MENU_ITEM, SimDevice.None);

        [MenuItem(NONE_MENU_ITEM)]
        private static void SimNone() => SetSimulated(SimDevice.None);

        [MenuItem(IPHONEX_MENU_ITEM, true)]
        private static bool CheckIPhone() => CheckSimulated(IPHONEX_MENU_ITEM, SimDevice.iPhoneX);

        [MenuItem(IPHONEX_MENU_ITEM)]
        private static void SimIPhone() => SetSimulated(SimDevice.iPhoneX);
#endif
    }

    public enum BannerPlacementType {
        None   = 0,
        Top    = 1,
        Bottom = 2,
    }
}