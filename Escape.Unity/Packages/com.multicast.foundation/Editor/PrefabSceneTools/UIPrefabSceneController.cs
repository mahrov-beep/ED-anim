#if UNITY_EDITOR

namespace Multicast.PrefabSceneTools {
    using System;
    using UnityEditor;
    using UnityEditor.SceneManagement;
    using UnityEngine;
    using UnityEngine.UI;

    internal static class UIPrefabSceneController {
        private const string SETTING_FILE = "UiPrefabSceneToolSettings.json";

        private const GameViewProxy.GameViewSizeType SIZE_TYPE = GameViewProxy.GameViewSizeType.FixedResolution;

        private static int LastSize {
            get => SessionState.GetInt("UIPrefabSceneController.lastSizeIndex", -1);
            set => SessionState.SetInt("UIPrefabSceneController.lastSizeIndex", value);
        }

        private static bool Last2dMode {
            get => SessionState.GetBool("UIPrefabSceneController.Last2dMode", false);
            set => SessionState.SetBool("UIPrefabSceneController.Last2dMode", value);
        }

        [InitializeOnLoadMethod]
        private static void Setup() {
            PrefabStage.prefabStageOpened  += OnPrefabStageOpened;
            PrefabStage.prefabStageClosing += OnPrefabStageClosing;
        }

        private static void OnPrefabStageClosing(PrefabStage obj) {
            try {
                var current = PrefabStageUtility.GetCurrentPrefabStage();
                if (current != null && current != obj) {
                    return;
                }

                if (LastSize != -1) {
                    RestoreGameViewSize();
                    EditorApplication.ExecuteMenuItem("Window/General/Scene");
                }
            }
            catch (Exception e) {
                Debug.LogError("UIEnvController: Failed to reset PrefabStage");
                Debug.LogException(e);
            }
        }

        private static void OnPrefabStageOpened(PrefabStage obj) {
            try {
                if (!(obj.prefabContentsRoot.transform is RectTransform)) {
                    RestoreGameViewSize();
                    SetScene2dMode(false);
                    return;
                }

                var settings = new SceneToolSettings();

                if (EditorGUIUtility.Load(SETTING_FILE) is TextAsset settingsAsset) {
                    JsonUtility.FromJsonOverwrite(settingsAsset.text, settings);
                }

                foreach (var rootGameObject in obj.scene.GetRootGameObjects()) {
                    if (rootGameObject.TryGetComponent(out CanvasScaler canvasScaler)) {
                        canvasScaler.referenceResolution = new Vector2(settings.width, settings.height);
                    }
                }

                if (LastSize == -1) {
                    LastSize   = GameViewProxy.GetSize();
                    Last2dMode = GetScene2dMode();
                }

                var name = $"{settings.width}x{settings.height}";
                var idx  = GameViewProxy.FindSize(GameViewProxy.GetCurrentViewSizeGroupType(), name);

                if (idx == -1) {
                    GameViewProxy.AddCustomSize(SIZE_TYPE, GameViewProxy.GetCurrentViewSizeGroupType(), settings.width, settings.height, name);
                    idx = GameViewProxy.FindSize(GameViewProxy.GetCurrentViewSizeGroupType(), name);
                }

                if (idx != -1) {
                    GameViewProxy.SetSize(idx);
                }

                EditorApplication.ExecuteMenuItem("Window/General/Scene");

                SetScene2dMode(true);
            }
            catch (Exception e) {
                Debug.LogError("UIEnvController: Failed to fix PrefabStage");
                Debug.LogException(e);
            }
        }

        private static void RestoreGameViewSize() {
            if (LastSize == -1) {
                return;
            }

            GameViewProxy.SetSize(LastSize);
            SetScene2dMode(Last2dMode);

            LastSize = -1;
        }

        private static bool GetScene2dMode() {
            foreach (var sceneViewUntyped in SceneView.sceneViews) {
                if (sceneViewUntyped is SceneView sceneView) {
                    return sceneView.in2DMode;
                }
            }

            return false;
        }

        private static void SetScene2dMode(bool in2d) {
            if (SceneView.sceneViews.Count > 0 && SceneView.sceneViews[0] is SceneView sceneView) {
                sceneView.in2DMode = in2d;
            }

            SceneView.RepaintAll();
        }

        [Serializable]
        private class SceneToolSettings {
            public int width  = 750;
            public int height = 1624;
        }
    }
}
#endif