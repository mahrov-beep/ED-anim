#if UNITY_EDITOR
namespace Multicast.Numerics {
    using JetBrains.Annotations;
    using Sirenix.OdinInspector.Editor;
    using Sirenix.Utilities.Editor;
    using UnityEditor;
    using UnityEngine;

    [UsedImplicitly]
    internal class GameTimeDrawer : OdinValueDrawer<GameTime> {
        protected override void DrawPropertyLayout(GUIContent label) {
            var t = this.ValueEntry.SmartValue.AsDateTime;

            GUILayout.BeginHorizontal();

            if (label != null) {
                EditorGUILayout.PrefixLabel(label);
            }

            GUIHelper.PushGUIEnabled(false);
            GUIHelper.PushLabelWidth(10);
            EditorGUILayout.TextField("D", t.ToString("dd/MM/yyyy"));
            EditorGUILayout.TextField("T", t.ToString("hh:mm:ss"));
            GUIHelper.PopLabelWidth();
            GUIHelper.PopGUIEnabled();

            GUILayout.EndHorizontal();
        }
    }
}
#endif