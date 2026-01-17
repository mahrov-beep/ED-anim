#if UNITY_EDITOR

namespace Multicast.PrefabSceneTools {
    using System;
    using System.Linq;
    using System.Reflection;
    using UnityEditor;
    using UnityEngine;

    // Add and Select Game View Resolution Programatically
    // https://answers.unity.com/questions/956123/add-and-select-game-view-resolution.html
    internal static class GameViewProxy {
        private static readonly Type GameViewWindowType = typeof(Editor).Assembly.GetType("UnityEditor.GameView");

        private static object     gameViewSizesInstance;
        private static MethodInfo getGroup;
        private static MethodInfo currentGroupType;

        [InitializeOnLoadMethod]
        private static void Init() {
            // gameViewSizesInstance  = ScriptableSingleton<GameViewSizes>.instance;
            var sizesType    = typeof(Editor).Assembly.GetType("UnityEditor.GameViewSizes");
            var singleType   = typeof(ScriptableSingleton<>).MakeGenericType(sizesType);
            var instanceProp = singleType.GetProperty("instance");
            getGroup              = sizesType.GetMethod("GetGroup");
            currentGroupType      = sizesType.GetProperty("currentGroupType")?.GetMethod;
            gameViewSizesInstance = instanceProp?.GetValue(null, null);
        }

        public enum GameViewSizeType {
            AspectRatio,
            FixedResolution,
        }

        public static GameViewSizeGroupType GetCurrentViewSizeGroupType() {
            return currentGroupType != null
                ? (GameViewSizeGroupType)currentGroupType.Invoke(gameViewSizesInstance, Array.Empty<object>())
                : GameViewSizeGroupType.Android;
        }

        public static int GetSize() {
            var flags     = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
            var indexProp = GameViewWindowType.GetProperty("selectedSizeIndex", flags);
            var gvWnd     = EditorWindow.GetWindow(GameViewWindowType);
            return (int) (indexProp?.GetValue(gvWnd, null)
                          ?? throw new InvalidOperationException("Failed to reflect UnityEditor.GameView"));
        }

        public static void SetSize(int index) {
            var flags     = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
            var indexProp = GameViewWindowType.GetProperty("selectedSizeIndex", flags);
            var gvWnd     = EditorWindow.GetWindow(GameViewWindowType);
            indexProp?.SetValue(gvWnd, index, null);

            var updateZoomMethod = GameViewWindowType.GetMethod("UpdateZoomAreaAndParent", flags);
            updateZoomMethod?.Invoke(gvWnd, new object[0]);

            var onResizedMethod = GameViewWindowType.GetMethod("OnResized", flags);
            onResizedMethod?.Invoke(gvWnd, new object[0]);

            SceneView.RepaintAll();
        }

        public static void AddCustomSize(GameViewSizeType viewSizeType, GameViewSizeGroupType sizeGroupType, int width, int height, string text) {
            // GameViewSizes group = gameViewSizesInstance.GetGroup(sizeGroupTyge);
            // group.AddCustomSize(new GameViewSize(viewSizeType, width, height, text);

            var group         = GetGroup(sizeGroupType);
            var addCustomSize = getGroup.ReturnType.GetMethod("AddCustomSize"); // or group.GetType().
            var gvsType       = typeof(Editor).Assembly.GetType("UnityEditor.GameViewSize");
            var ctor          = gvsType.GetConstructors().Single(c => c.GetParameters().Length == 4);
            var newSize       = ctor.Invoke(new object[] {(int) viewSizeType, width, height, text});

            addCustomSize?.Invoke(group, new[] {newSize});
        }

        public static int FindSize(GameViewSizeGroupType sizeGroupType, string text) {
            // GameViewSizes group = gameViewSizesInstance.GetGroup(sizeGroupType);
            // string[] texts = group.GetDisplayTexts();
            // for loop...

            var group           = GetGroup(sizeGroupType);
            var getDisplayTexts = group.GetType().GetMethod("GetDisplayTexts");
            var displayTexts    = getDisplayTexts?.Invoke(@group, null) as string[] ?? new string[0];

            for (var i = 0; i < displayTexts.Length; i++) {
                var display = displayTexts[i];
                // the text we get is "Name (W:H)" if the size has a name, or just "W:H" e.g. 16:9
                // so if we're querying a custom size text we substring to only get the name
                // You could see the outputs by just logging
                // Debug.Log(display);
                var pren = display.IndexOf('(');
                if (pren != -1) {
                    display = display.Substring(0, pren - 1); // -1 to remove the space that's before the prens. This is very implementation-depdenent
                }

                if (display == text) {
                    return i;
                }
            }

            return -1;
        }

        private static object GetGroup(GameViewSizeGroupType type) {
            return getGroup.Invoke(gameViewSizesInstance, new object[] {(int) type});
        }
    }
}

#endif