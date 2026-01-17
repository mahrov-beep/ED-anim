using Scellecs.Morpeh;
using UnityEngine;
using UnityEngine.SceneManagement;

[DefaultExecutionOrder(-100000)]
public class BootloaderChecker : MonoBehaviour {
#if UNITY_EDITOR
    [UnityEditor.InitializeOnLoad]
    static class BootloaderCheckerEditor {
        static BootloaderCheckerEditor() {
            UnityEditor.EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
            UnityEditor.EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
            UnityEditor.EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        static void OnPlayModeStateChanged(UnityEditor.PlayModeStateChange state) {
            if (state == UnityEditor.PlayModeStateChange.ExitingPlayMode) {
                BootloaderChecker.isBootloaderLaunched = false;
            }
        }
    }
#endif

    static bool isBootloaderLaunched;

    [SerializeField] string bootloaderSceneName = "Bootloader";

    void Awake() {
        if (isBootloaderLaunched) {
            return;
        }

        var onBootLoader = SceneManager.GetActiveScene().name == bootloaderSceneName;
        if (onBootLoader) {
            isBootloaderLaunched = true;
        }
        else {
            SceneManager.LoadScene(bootloaderSceneName);
        }
    }
}