namespace Game.UI.Widgets.ItemInfo {
    using System;
    using System.Globalization;
    using Multicast;
    using Quantum;
    using UniMob.UI;
    using Views.ItemInfo;

    [RequireFieldsInit]
    public class ItemInfoWeaponStatWidget : StatefulWidget {
        public string               StatType;
        public ERarityType          StatRarity;
        public FPBoostedValue?      Value;
        public FPBoostedMultiplier? Multiplier;
    }

    public class ItemInfoWeaponStatState : ViewState<ItemInfoWeaponStatWidget>, IItemInfoStatState {
        public override WidgetViewReference View => UiConstants.Views.ItemInfo.Stat;

        public string StatRarity => EnumNames<ERarityType>.GetName(this.Widget.StatRarity);
        public string StatKey    => this.Widget.StatType;
        public string StatValue  => StringifyStat(this.Widget.Value, this.Widget.Multiplier);

        string StringifyStat(FPBoostedValue? value, FPBoostedMultiplier? multiplier) {
            if (value is { } v) {
                var currentValue      = Math.Round(v.AsFloat, 2);
                var baseValue         = Math.Round(v.BaseValue.AsFloat, 2);
                var multiplierPercent = Math.Round(v.AdditiveMultiplierMinus1.AsFloat * 100, 1);
                if (multiplierPercent == 0) {
                    return $"{currentValue.ToString(CultureInfo.InvariantCulture)}";
                }

                //var sign = multiplierPercent >= 0 ? "+" : "";
                //return $"{currentValue} <color=#888>({baseValue} {sign}{multiplierPercent}% by {v.BoostCount} att)</color>";
                //return $"`{currentValue}";
                var additiveValue     = currentValue - baseValue;
                var additiveValueSign = additiveValue > 0 ? "+" : "";
                return $"{baseValue.ToString(CultureInfo.InvariantCulture)} ({additiveValueSign}{additiveValue.ToString(CultureInfo.InvariantCulture)})";
            }

            if (multiplier is { } m) {
                var multiplierPercent = Math.Round(m.AdditiveMultiplierMinus1.AsFloat * 100, 1);

                if (multiplierPercent == 0) {
                    return $"-";
                }

                var sign = multiplierPercent >= 0 ? "+" : "";
                //return $"{sign}{multiplierPercent}% <color=#888>(by {m.BoostCount} att)</color>";
                return $"{sign}{multiplierPercent.ToString(CultureInfo.InvariantCulture)}%";
            }

            return "";
        }
    }
}