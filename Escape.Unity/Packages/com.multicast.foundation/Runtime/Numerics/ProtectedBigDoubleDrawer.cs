#if UNITY_EDITOR
namespace Multicast.Numerics {
    using JetBrains.Annotations;
    using Sirenix.OdinInspector.Editor;
    using UnityEditor;
    using UnityEngine;

    [UsedImplicitly]
    public class ProtectedBigDoubleDrawer : OdinValueDrawer<ProtectedBigDouble> {
        private GUIStyle rightLabel;

        protected override void DrawPropertyLayout(GUIContent label) {
            if (this.rightLabel == null) {
                this.rightLabel = new GUIStyle(EditorStyles.centeredGreyMiniLabel) {
                    alignment = TextAnchor.MiddleRight,
                    padding   = new RectOffset(0, 5, 0, 0)
                };
            }

            var position = EditorGUILayout.GetControlRect();

            if (label != null) {
                position = EditorGUI.PrefixLabel(position, label);
            }

            var value = this.ValueEntry.SmartValue.Value;

            var newValueString = EditorGUI.TextField(position, value.ToString());

            BigDouble newValue;
            try {
                newValue = BigDouble.Parse(newValueString);
            }
            catch {
                newValue = 0;
            }
            
            this.ValueEntry.SmartValue = new ProtectedBigDouble(newValue, false);

            GUI.Label(position, BigString.ToString(newValue), this.rightLabel);
        }
    }
}

#endif