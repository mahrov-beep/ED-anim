namespace Multicast {
    using System;
    using CodeWriter.ViewBinding;
    using Numerics;
    using UnityEngine;
    using UnityEngine.Scripting;

    [Serializable, Preserve, ExposedViewEntry]
    public sealed class ViewVariableBigDouble : ViewVariable<BigDouble, ViewVariableBigDouble> {
        public override string TypeDisplayName => "Big Double";

        [Preserve]
        public ViewVariableBigDouble() {
        }

        public override void AppendValueTo(ref ValueTextBuilder builder) {
            ToString(ref builder, this.Value);
        }

#if UNITY_EDITOR
        public override void DoGUI(Rect position, GUIContent label,
            UnityEditor.SerializedProperty property, string variableName) {
            var numeratorProp = property.FindPropertyRelative("numerator");
            var exponentProp  = property.FindPropertyRelative("exponent");

            var value = new BigDouble(numeratorProp.doubleValue, exponentProp.longValue);

            UnityEditor.EditorGUI.BeginChangeCheck();

            var newValueString = UnityEditor.EditorGUI.DelayedTextField(position, label, value.ToString());

            BigDouble newValue;
            try {
                newValue = BigDouble.Parse(newValueString);
            }
            catch {
                newValue = 0;
            }

            if (UnityEditor.EditorGUI.EndChangeCheck()) {
                numeratorProp.doubleValue = newValue.numerator;
                exponentProp.longValue    = newValue.exponent;
            }
        }

        public override void DoRuntimeGUI(Rect position, GUIContent label, string variableName) {
            UnityEditor.EditorGUI.LabelField(position, label.text, this.Value.ToString(), GUI.skin.label);
        }
#endif

        private static readonly int[] Exponents = {
            1,
            10,
            100,
        };

        public static void ToString(ref ValueTextBuilder textBuilder, BigDouble d) {
            if (d == BigDouble.Zero) {
                textBuilder.Append("0");
            }
            else if (d.exponent < 3) {
                var digits = Math.Min(2, 2 - (int) d.exponent);
                var result = Math.Round(d.numerator * Math.Pow(10, d.exponent), digits);

                textBuilder.Append((float) result, digits);
            }
            else {
                var expMod  = (int) d.exponent % 3;
                var expLeft = d.exponent - expMod;
                var digits  = 2 - expMod;
                var result  = Math.Round(d.numerator * Exponents[expMod], 2 - expMod);

                textBuilder.Append((float) result, digits, fixedPrecision: true);

                if (expLeft != 0L) {
                    textBuilder.Append(expLeft >= BigString.Names.Length * 3 ? BigString.GenerateCurrency(expLeft) : BigString.Names[expLeft / 3]);
                }
            }
        }
    }
}