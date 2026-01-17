#if UNITY_EDITOR && UNITY_EDITOR_WIN
namespace _EditorTools.LayoutSwitcherUtility.Scripts {
    using UnityEditor;
    using UnityEngine;
    [InitializeOnLoad]
    public static class LayoutSwitcher {
        static LayoutSettings settings;
        static EResizeWindow  _resizeOption;

        static LayoutSwitcher() {
            LoadSettings();
        }

        static void LoadSettings() {
            string[] guids = AssetDatabase.FindAssets("t:LayoutSettings");
            if (guids.Length > 0) {
                string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                settings = AssetDatabase.LoadAssetAtPath<LayoutSettings>(path);
            }
            else {
                settings = ScriptableObject.CreateInstance<LayoutSettings>();
                AssetDatabase.CreateAsset(settings, "Assets/Editor/LayoutSettings.asset");
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                Debug.LogWarning("LayoutSettings.asset не найден. Он был создан автоматически в папке Assets/Editor/. Пожалуйста, задайте имена макетов в инспекторе.");
            }
        }

        [MenuItem("Tools/Switch Layout/Layout 1 _F1")]
        static void SwitchToLayout1() {
            if (settings == null) {
                LoadSettings();
            }

            SwitchLayout(settings.layout1Name, settings.layout1ResizeOption);
        }

        [MenuItem("Tools/Switch Layout/Layout 2 _F2")]
        static void SwitchToLayout2() {
            if (settings == null) {
                LoadSettings();
            }

            SwitchLayout(settings.layout2Name, settings.layout2ResizeOption);
        }

        [MenuItem("Tools/Switch Layout/Layout 3 _F3")]
        static void SwitchToLayout3() {
            if (settings == null) {
                LoadSettings();
            }

            SwitchLayout(settings.layout3Name, settings.layout3ResizeOption);
        }

        [MenuItem("Tools/Switch Layout/Layout 4 _F4")]
        static void SwitchToLayout4() {
            if (settings == null) {
                LoadSettings();
            }

            SwitchLayout(settings.layout4Name, settings.layout4ResizeOption);
        }

        static void SwitchLayout(string layoutName, EResizeWindow resizeOption) {
            if (string.IsNullOrEmpty(layoutName)) {
                Debug.LogError("Имя макета не задано. Пожалуйста, укажите имя макета в настройках.");
                return;
            }

            bool result = EditorApplication.ExecuteMenuItem("Window/Layouts/" + layoutName);

            if (!result) {
                Debug.LogError("Не удалось переключиться на макет: " + layoutName + ". Убедитесь, что имя макета совпадает с пунктом в меню Window > Layouts.");
                return;
            }

#if UNITY_EDITOR && UNITY_EDITOR_WIN
            _resizeOption               =  resizeOption;
            EditorApplication.delayCall += OnDelayCall;
#endif
        }

#if UNITY_EDITOR && UNITY_EDITOR_WIN
        static void OnDelayCall() {
            EditorWindowResizer.ResizeEditorWindow(_resizeOption);
        }
#endif
    }
}
#endif