namespace Multicast {
    using System;
    using CodeWriter.ViewBinding;
    using Numerics;
    using UnityEngine;
    using UnityEngine.Scripting;

    [Serializable, Preserve, ExposedViewEntry]
    public sealed class ViewVariableCost : ViewVariable<Cost, ViewVariableCost> {
        public override string TypeDisplayName => "Cost";

        public FormattingArgs    formatting;
        public ViewContextBase[] extraContexts;

        [Preserve]
        public ViewVariableCost() {
        }

        public override void AppendValueTo(ref ValueTextBuilder builder) {
            ToString(ref builder, this.Value, this.formatting, this.extraContexts);
        }

#if UNITY_EDITOR
        public override void DoGUI(Rect position, GUIContent label,
            UnityEditor.SerializedProperty property, string variableName) {
            UnityEditor.EditorGUI.LabelField(position, label.text, "Not implemented", GUI.skin.label);
        }

        public override void DoRuntimeGUI(Rect position, GUIContent label, string variableName) {
            UnityEditor.EditorGUI.LabelField(position, label.text, "Not implemented", GUI.skin.label);
        }
#endif

        public static void ToString(ref ValueTextBuilder textBuilder, Cost cost, FormattingArgs formatting, ViewContextBase[] extraContexts) {
            extraContexts ??= Array.Empty<ViewContextBase>();

            if (cost.CurrenciesCount == 0) {
                textBuilder.AppendFormat(formatting.emptyCostPlaceholder, extraContexts);
                return;
            }

            var isFirst = true;

            foreach (var (currencyKey, amount) in cost) {
                if (!isFirst) {
                    textBuilder.AppendFormat(formatting.partsSeparator ?? FormattingArgs.DEFAULT_PARTS_SEPARATOR, extraContexts);
                }

                isFirst = false;

                if (formatting.withLeftIcon) {
                    textBuilder.Append("<sprite name=");
                    textBuilder.Append(currencyKey);
                    textBuilder.Append(">");
                    textBuilder.AppendFormat(formatting.currencyIconSeparator ?? FormattingArgs.DEFAULT_CURRENCY_ICON_SEPARATOR, extraContexts);
                }

                if (formatting.withBalance) {
                    ViewVariableBigDouble.ToString(ref textBuilder, Cost.BalanceProvider(currencyKey));
                    textBuilder.AppendFormat(formatting.balanceSeparator ?? FormattingArgs.DEFAULT_BALANCE_SEPARATOR, extraContexts);
                }

                textBuilder.Append(formatting.amountPrefix);
                ViewVariableBigDouble.ToString(ref textBuilder, amount);
                textBuilder.Append(formatting.amountSuffix);

                if (!formatting.withLeftIcon) {
                    textBuilder.AppendFormat(formatting.currencyIconSeparator ?? FormattingArgs.DEFAULT_CURRENCY_ICON_SEPARATOR, extraContexts);
                    textBuilder.Append("<sprite name=");
                    textBuilder.Append(currencyKey);
                    textBuilder.Append(">");
                }
            }
        }

        [Serializable]
        public struct FormattingArgs {
            public const string DEFAULT_PARTS_SEPARATOR         = "<space=10>";
            public const string DEFAULT_CURRENCY_ICON_SEPARATOR = "";
            public const string DEFAULT_BALANCE_SEPARATOR       = "/";

            public bool withBalance;
            public bool withLeftIcon;

            public string partsSeparator;
            public string currencyIconSeparator;
            public string balanceSeparator;
            public string amountPrefix;
            public string amountSuffix;

            public string emptyCostPlaceholder;
        }
    }
}