#if UNITY_EDITOR

namespace Multicast.Numerics {
    using JetBrains.Annotations;
    using Sirenix.OdinInspector.Editor;
    using UnityEditor;
    using UnityEngine;

    [UsedImplicitly]
    public class ProtectedIntDrawer : OdinValueDrawer<ProtectedInt> {
        protected override void DrawPropertyLayout(GUIContent label) {
            var position = EditorGUILayout.GetControlRect();

            if (label != null) {
                position = EditorGUI.PrefixLabel(position, label);
            }

            var value = this.ValueEntry.SmartValue;

            if (value.IsValid) {
                value = (ProtectedInt) EditorGUI.IntField(position, value.Value);
            }
            else {
                if (GUI.Button(position, "Fix")) {
                    value = new ProtectedInt(0);
                }
            }

            this.ValueEntry.SmartValue = value;
        }
    }
}

#endif