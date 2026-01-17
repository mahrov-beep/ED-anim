namespace Game.UI.Widgets {
    using System;
    using System.Text;
    using Common;
    using Domain.Game;
    using Domain.GameInventory;
    using Domain.Safe;
    using GameInventory;
    using Multicast;
    using Quantum;
    using UniMob.UI;
    using UniMob.UI.Widgets;
    using UnityEngine;

    [RequireFieldsInit]
    public class StatsWithInventoryWidget : StatefulWidget {
        public Action            OnClose;
        public IAnimation<float> Animation;
    }

    public class StatsWithInventoryState : HocState<StatsWithInventoryWidget> {
        private static readonly StringBuilder SharedStringBuilder = new StringBuilder();

        [Inject] private GameLocalCharacterModel localCharacterModel;
        [Inject] private GameInventoryModel      gameInventoryModel;
        [Inject] private SafeModel               safeModel;

        public override Widget Build(BuildContext context) {
            return new ZStack {
                Children = {
                    new LoadingScreenWidget {
                        View = UiConstants.Views.LoadingBgScreen,
                    },
                    new Builder(this.BuildStats),
                    new GameInventoryWidget {
                        OnClose               = this.Widget.OnClose,
                        ShowItemsThrowZone    = true,
                        NoDraggingInInventory = false,
                        IgnoreNearby          = false,
                        OnIncrementLoadout    = default,
                        OnDecrementLoadout    = default,
                        HasSomeLoadouts       = false,
                        LoadoutIndex          = 0,
                        LoadoutCount          = 0,
                    },
                },
            };
        }

        private Widget BuildStats(BuildContext context) {
            //var primary   = this.gameInventoryModel.PrimaryWeapon;
            //var secondary = this.gameInventoryModel.SecondaryWeapon;
            //var melee     = this.gameInventoryModel.MeleeWeapon;

            return new Column {
                Size               = WidgetSize.Stretched,
                MainAxisAlignment  = MainAxisAlignment.End,
                CrossAxisAlignment = CrossAxisAlignment.Start,
                Children = {
                    Text($"LOCAL PLAYER:\n{StringifyUnitStats(this.localCharacterModel.Stats)}"),
                    //Text($"PRIMARY WEAPON{IsSelectedLabel(primary)}:\n{StringifyWeaponStats(primary.Stats)}"),
                    //Text($"SECONDARY WEAPON{IsSelectedLabel(secondary)}:\n{StringifyWeaponStats(secondary.Stats)}"),
                    //Text($"MELEE WEAPON{IsSelectedLabel(melee)}:\n{StringifyWeaponStats(melee.Stats)}"),
                },
            };

            string IsSelectedLabel(GameWeaponModel weapon) {
                return weapon.IsSelected ? " (SELECTED)" : "";
            }

            string StringifyWeaponStats(WeaponStats weaponStats) {
                SharedStringBuilder.Clear();

                weaponStats.VisitStats((name, value, multiplier) => SharedStringBuilder
                    .Append(name)
                    .Append(": ")
                    .Append(StringifyStat(value, multiplier))
                    .AppendLine()
                );

                return SharedStringBuilder.ToString();
            }

            string StringifyUnitStats(UnitStats unitStats) {
                SharedStringBuilder.Clear();

                unitStats.VisitStats((name, value, multiplier) => SharedStringBuilder
                    .Append(name)
                    .Append(": ")
                    .Append(StringifyStat(value, multiplier))
                    .AppendLine()
                );

                return SharedStringBuilder.ToString();
            }

            string StringifyStat(FPBoostedValue? value, FPBoostedMultiplier? multiplier) {
                if (value is { } v) {
                    var currentValue      = Math.Round(v.AsFloat, 2);
                    var baseValue         = Math.Round(v.BaseValue.AsFloat, 2);
                    var additiveValue     = Math.Round(v.AdditiveValue.AsFloat, 2);
                    var multiplierPercent = Math.Round(v.AdditiveMultiplierMinus1.AsFloat * 100, 1);
                    if (additiveValue == 0 && multiplierPercent == 0) {
                        return $"<size=130%>{currentValue}</size>";
                    }

                    var additiveSign = additiveValue >= 0 ? "+" : "";
                    var percentSign  = multiplierPercent >= 0 ? "+" : "";
                    return $"<size=130%>{currentValue}</size>: {baseValue} {additiveSign}{additiveValue} {percentSign}{multiplierPercent}%";
                }

                if (multiplier is { } m) {
                    var multiplierPercent = Math.Round(m.AdditiveMultiplierMinus1.AsFloat * 100, 1);

                    if (multiplierPercent == 0) {
                        return $"-";
                    }

                    var sign = multiplierPercent >= 0 ? "+" : "";
                    return $"{sign}<size=130%>{multiplierPercent}%</size>";
                }

                return "";
            }

            Widget Text(string text) {
                return new PaddingBox(new RectPadding(10, 10, 10, 10)) {
                    Child = new Container {
                        BackgroundColor = new Color(0, 0, 0, 0.4f),
                        Child = new UniMobText {
                            FontSize = 36,
                            Value    = text,
                            Color    = Color.white,
                        },
                    },
                };
            }
        }
    }
}