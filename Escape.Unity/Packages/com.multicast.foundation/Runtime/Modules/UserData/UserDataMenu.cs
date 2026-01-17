#if UNITY_EDITOR

namespace Multicast.Modules.UserData {
    using UnityEditor;
    using UnityEngine;

    public static class UserDataMenu {
        public const string SELECTOR_MENU_ITEM = "Game/Show UserData Selector on Start";

        [MenuItem(SELECTOR_MENU_ITEM, true)]
        public static bool ValidateShowFeatureSelectorOnce() {
            Menu.SetChecked(SELECTOR_MENU_ITEM, UserDataUI.ShowUserDataSelectorOnce);
            return true;
        }

        [MenuItem(SELECTOR_MENU_ITEM, priority = 110)]
        public static void SetShowFeatureSelectorOnce() {
            UserDataUI.ShowUserDataSelectorOnce = true;
        }

        [MenuItem("Game/Clear User Data", validate = true)]
        private static bool CanClearUserData() {
            return !Application.isPlaying;
        }

        [MenuItem("Game/Clear User Data", priority = 0)]
        private static void ClearUserData() {
            UserDataStatics.DeleteAllData();
        }
    }
}

#endif