namespace Game.UI.Widgets.ItemInfo {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using GameInventory;
    using Multicast;
    using Quantum;
    using Quantum.Commands;
    using Services.Photon;
    using UniMob;
    using UniMob.UI;
    using UniMob.UI.Widgets;
    using Views.ItemInfo;

    public class ItemInfoWidget : StatefulWidget {
        public Action OnClose { get; set; }

        public WidgetPosition.Position Position { get; set; } = WidgetPosition.Position.Center;

        public bool IsTakeButtonVisible { get; set; }

        public ItemAsset ItemAsset          { get; set; }
        public EntityRef ItemEntityOptional { get; set; }
    }

    public class ItemInfoState : ViewState<ItemInfoWidget>, IItemInfoState {
        [Inject] private GameInventoryApi gameInventoryApi;
        [Inject] private PhotonService    photonService;

        private readonly StateHolder statsHolder;

        public ItemInfoState() {
            this.statsHolder = this.CreateChild(this.BuildStats);
        }

        public override void InitState() {
            base.InitState();

            App.Events.Listen(this.StateLifetime, (ApplicationUpdateEvent evt) => {
                var f = this.photonService.PredictedFrame;

                var itemAsset = this.ItemAsset;
                var itemRef   = this.ItemEntity;
                var exist     = f != null && f.Exists(this.ItemEntity);
                var item      = f != null && f.TryGet(itemRef, out Item outItem) ? outItem : default;

                this.Weight          = exist ? Item.GetItemWeight(f, itemRef).AsFloat : itemAsset.weight.AsFloat;
                this.UsagesRemaining = exist ? Item.GetRemainingUsages(f, itemRef) : itemAsset.MaxUsages;

                this.CanTake = this.IsTakeVisible && exist && (
                    this.gameInventoryApi.IsEnoughSpaceTetris(itemRef, out _, RotationType.Find) ||
                    this.gameInventoryApi.TryFindSlotForItem(itemRef, out _, out _)
                );

                this.Rotated = exist && item.Rotated;

                this.WeaponStats = exist && f.TryGet(itemRef, out Weapon weapon) ? weapon.CurrentStats : default;
            });
        }

        [Atom] private WeaponStats WeaponStats { get; set; }

        public override WidgetViewReference View => UiConstants.Views.ItemInfo.Screen;

        private ItemAsset ItemAsset  => this.Widget.ItemAsset;
        private EntityRef ItemEntity => this.Widget.ItemEntityOptional;

        public string ItemKey    => this.ItemAsset.ItemKey;
        public string ItemIcon   => this.ItemAsset.IconLarge;
        public string ItemRarity => EnumNames<ERarityType>.GetName(this.ItemAsset.rarity);
        public int    Quality    => this.ItemAsset.Def.Quality;


        public bool IsTakeVisible => this.Widget.IsTakeButtonVisible;

        [Atom] public float Weight          { get; set; }
        [Atom] public bool  CanTake         { get; set; }
        [Atom] public bool  Rotated         { get; set; }
        [Atom] public int   UsagesRemaining { get; set; }

        public IState Stats => this.statsHolder.Value;

        public WidgetPosition.Position WidgetPosition => this.Widget.Position;

        public void Close() {
            this.Widget.OnClose?.Invoke();
        }

        public void Take() {
            if (!this.gameInventoryApi.IsEnoughSpaceTetris(this.ItemEntity, out var place, RotationType.Find)
                && !this.gameInventoryApi.TryFindSlotForItem(this.ItemEntity, out _, out _)) {
                return;
            }

            this.photonService.Runner?.Game.SendCommand(new SwapTetrisCommand {
                ItemEntity        = this.ItemEntity,
                SmartAssignToSlot = true,
                IndexI            = place.I,
                IndexJ            = place.J,
                Rotated           = place.Rotated,
            });

            this.Close();
        }

        public void Rotate() {
            this.photonService.Runner?.Game.SendCommand(new RotateItemCommand() {
                ItemEntity = this.ItemEntity,
            });
        }

        private Widget BuildStats(BuildContext context) {
            var rarityEffects = this.ItemAsset.rarityEffects.IsValid &&
                                QuantumUnityDB.GetGlobalAsset(this.ItemAsset.rarityEffects).rarityEffects is { } rarityEffectsAsset &&
                                rarityEffectsAsset.FindIndex(it => it.rarity == this.ItemAsset.rarity) is var index && index != -1
                ? QuantumUnityDB.GetGlobalAsset(this.ItemAsset.rarityEffects).rarityEffects[index].itemEffects
                : new List<ItemAsset.PersistentItemEffect>();

            var effects = ItemAsset.PersistentItemEffect.Merge(this.ItemAsset.persistentEffects, rarityEffects);

            return new ScrollGridFlow {
                MaxCrossAxisCount = 2,
                Padding           = new RectPadding(20, 20, 20, 20),
                Children = {
                    this.BuildWeaponStats(),
                    this.BuildHealBoxStats(),
                    this.BuildUsageStats(),
                    this.BuildEffectOnUseStats(),

                    effects.Select(this.BuildStat),
                },
            };
        }

        private IEnumerable<Widget> BuildWeaponStats() {
            var stats = this.WeaponStats;

            if (EqualityComparer<WeaponStats>.Default.Equals(stats, default)) {
                if (this.ItemAsset is WeaponItemAsset weaponItemAsset) {
                    stats = weaponItemAsset.GetBaseStats();
                }
                else {
                    return Array.Empty<Widget>();
                }
            }

            var statWidgets = new List<Widget>();

            stats.VisitStats((type, value, multiplier) => {
                statWidgets.Add(new ItemInfoWeaponStatWidget {
                    Key        = Key.Of(type),
                    StatRarity = this.ItemAsset.rarity,
                    StatType   = type,
                    Value      = value,
                    Multiplier = multiplier,
                });
            });

            return statWidgets;
        }

        private IEnumerable<Widget> BuildHealBoxStats() {
            if (this.ItemAsset is not HealBoxItemAsset healBoxItemAsset) {
                return Array.Empty<Widget>();
            }

            return new[] {
                new ItemInfoHealStatWidget {
                    StatRarity = this.ItemAsset.rarity,
                    Heal       = healBoxItemAsset.heal,
                },
            };
        }

        private IEnumerable<Widget> BuildUsageStats() {
            if (this.ItemAsset.MaxUsages == 0) {
                return Array.Empty<Widget>();
            }

            return new[] {
                new ItemInfoUsedStatWidget {
                    StatRarity      = this.ItemAsset.rarity,
                    UsagesRemaining = this.UsagesRemaining,
                    MaxUsages       = this.ItemAsset.MaxUsages,
                },
            };
        }

        private IEnumerable<Widget> BuildEffectOnUseStats() {
            if (this.ItemAsset is not UsableItemAsset usableItemAsset) {
                return Array.Empty<Widget>();
            }

            return usableItemAsset.onUseEffects.Select(it => new ItemInfoStatWidget {
                Key        = Key.Of(it.AttributeType + "_onUse"),
                StatType   = it.AttributeType,
                StatRarity = this.ItemAsset.rarity,
                StatValue = it.Operation switch {
                    EModifierOperation.Add => it.Value.AsFloat,
                    EModifierOperation.Subtract => -it.Value.AsFloat,
                    _ => 0,
                },
                Duration = it.Duration,
            });
        }

        private Widget BuildStat(ItemAsset.PersistentItemEffect effect) {
            return new ItemInfoStatWidget {
                Key        = Key.Of(effect.attributeType),
                StatType   = effect.attributeType,
                StatRarity = this.ItemAsset.rarity,
                StatValue = effect.operation switch {
                    EModifierOperation.Add => effect.value.AsFloat,
                    EModifierOperation.Subtract => -effect.value.AsFloat,
                    _ => 0,
                },
                Duration = 0,
            };
        }
    }
}