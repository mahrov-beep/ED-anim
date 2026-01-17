#if UNITY_EDITOR
namespace Multicast.Numerics {
    using JetBrains.Annotations;
    using Sirenix.OdinInspector.Editor;
    using UnityEditor;
    using UnityEngine;

    [UsedImplicitly]
    public class FixedDoubleDrawer : OdinValueDrawer<FixedDouble> {
        protected override void DrawPropertyLayout(GUIContent label) {
            var value = (double) this.ValueEntry.SmartValue;

            EditorGUI.BeginChangeCheck();

            value = EditorGUILayout.DoubleField(label, value);

            if (EditorGUI.EndChangeCheck()) {
                this.ValueEntry.SmartValue = (FixedDouble) value;
            }
        }
    }
}
#endif