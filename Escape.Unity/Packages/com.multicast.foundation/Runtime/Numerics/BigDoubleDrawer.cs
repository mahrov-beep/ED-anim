#if UNITY_EDITOR
namespace Multicast.Numerics {
    using JetBrains.Annotations;
    using Sirenix.OdinInspector.Editor;
    using UnityEditor;
    using UnityEngine;

    [UsedImplicitly]
    public class BigDoubleDrawer : OdinValueDrawer<BigDouble> {
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

            this.ValueEntry.SmartValue = DrawValue(position, this.ValueEntry.SmartValue);

            GUI.Label(position, BigString.ToString(this.ValueEntry.SmartValue), this.rightLabel);
        }

        public static BigDouble DrawValue(Rect rect, BigDouble value) {
            var newValueString = EditorGUI.TextField(rect, value.ToString());

            try {
                return BigDouble.Parse(newValueString);
            }
            catch {
                return BigDouble.Zero;
            }
        }
    }
}

#endif