#if UNITY_EDITOR
namespace Multicast.FeatureToggles {
    using UnityEditor;

    public static class FeatureTogglesEditorMenu {
        public const string MENU_ITEM = "Game/Show FeatureToggles Selector on Start";

        [MenuItem(MENU_ITEM, true)]
        public static bool ValidateShowFeatureSelectorOnce() {
            Menu.SetChecked(MENU_ITEM, FeatureTogglesUI.ShowFeatureTogglesSelectorOnce);
            return true;
        }

        [MenuItem(MENU_ITEM, priority = 100)]
        public static void SetShowFeatureSelectorOnce() {
            FeatureTogglesUI.ShowFeatureTogglesSelectorOnce = true;
        }
    }
}
#endif