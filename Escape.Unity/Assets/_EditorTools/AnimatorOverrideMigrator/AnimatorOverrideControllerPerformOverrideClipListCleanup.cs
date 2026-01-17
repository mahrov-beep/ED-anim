#if UNITY_EDITOR

namespace _EditorTools {
    using System.Reflection;
    using UnityEditor;
    using UnityEngine;

    public static class AnimatorOverrideControllerPerformOverrideClipListCleanup {
        [MenuItem("CONTEXT/AnimatorOverrideController/Cleanup OverrideClip List", false, 0)]
        public static void PerformCleanup(MenuCommand cmd) {
            var oc = (AnimatorOverrideController)cmd.context;

            typeof(AnimatorOverrideController)
                .GetMethod("PerformOverrideClipListCleanup", BindingFlags.Instance | BindingFlags.NonPublic)!
                .Invoke(oc, null);

            EditorUtility.SetDirty(oc);
            AssetDatabase.SaveAssetIfDirty(oc);
        }
    }
}

#endif