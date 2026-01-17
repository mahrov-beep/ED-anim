#if UNITY_EDITOR
namespace Multicast.Numerics {
    using JetBrains.Annotations;
    using Sirenix.OdinInspector.Editor;
    using UnityEditor;
    using UnityEngine;
    using UnityEngine.Pool;

    [UsedImplicitly]
    public class CostDrawer : OdinValueDrawer<Cost> {
        protected override void DrawPropertyLayout(GUIContent label) {
            var value = this.ValueEntry.SmartValue;

            var position = EditorGUILayout.GetControlRect(true,
                (value.CurrenciesCount + 1) * EditorGUIUtility.singleLineHeight);

            if (label != null) {
                position = EditorGUI.PrefixLabel(position, label);
            }

            EditorGUI.BeginChangeCheck();

            using (ListPool<(string currencyKey, BigDouble amount)>.Get(out var tempValues)) {
                foreach (var (currencyKey, amount) in value) {
                    tempValues.Add((currencyKey, amount));
                }

                foreach (var (currencyKey, amount) in tempValues) {
                    var rect = new Rect(position) {
                        height = EditorGUIUtility.singleLineHeight,
                    };

                    var currencyKeyRect = new Rect(rect) {
                        xMax = rect.xMin + 120,
                    };
                    var removeRect = new Rect(rect) {
                        xMin = rect.xMax - 20,
                    };
                    var amountRect = new Rect(rect) {
                        xMin = currencyKeyRect.xMax + 1,
                        xMax = removeRect.xMin - 1,
                    };

                    position.yMin += rect.height;

                    var newCurrencyKey = GUI.TextField(currencyKeyRect, currencyKey);

                    if (newCurrencyKey != currencyKey) {
                        value.Clear(currencyKey);
                        value.Add(newCurrencyKey, amount);
                        GUIUtility.ExitGUI();
                    }

                    value[currencyKey] = BigDoubleDrawer.DrawValue(amountRect, amount);

                    if (GUI.Button(removeRect, "-")) {
                        value.Clear(currencyKey);
                        GUIUtility.ExitGUI();
                    }
                }

                var addRect = new Rect(position) {
                    xMin   = position.xMax - 20,
                    height = EditorGUIUtility.singleLineHeight,
                };

                if (GUI.Button(addRect, "+")) {
                    value.Add("", BigDouble.Zero);
                    GUIUtility.ExitGUI();
                }
            }

            if (EditorGUI.EndChangeCheck()) {
                this.ValueEntry.SmartValue = value;
            }
        }
    }
}
#endif