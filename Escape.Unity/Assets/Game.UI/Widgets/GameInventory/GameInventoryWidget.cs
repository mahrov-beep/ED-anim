namespace Game.UI.Widgets.GameInventory {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Domain;
    using Domain.GameInventory;
    using Game;
    using Multicast;
    using Quantum;
    using Quantum.Commands;
    using Services.Photon;
    using Simulation;
    using SoundEffects;
    using UniMob;
    using UniMob.UI;
    using UniMob.UI.Widgets;
    using Views;
    using Views.GameInventory;

    [RequireFieldsInit]
    public class GameInventoryWidget : StatefulWidget {
        public Action OnClose;
        public Action OnIncrementLoadout;
        public Action OnDecrementLoadout;

        public bool ShowItemsThrowZone;
        public bool NoDraggingInInventory;
        
        public bool IgnoreNearby;
        
        public bool HasSomeLoadouts;
        
        public int LoadoutIndex;
        public int LoadoutCount;
    }

    public class GameInventoryState : ViewState<GameInventoryWidget>, IGameInventoryState {
        [Inject] private GameInventoryModel   gameInventoryModel;
        [Inject] private PhotonService        photonService;
        [Inject] private GameInventoryApi     gameInventoryApi;
        [Inject] private GameNearbyItemsModel gameNearbyItemsModel;

        private readonly StateHolder slotItemState;
        private readonly StateHolder trashItemState;
        private readonly StateHolder filtersState;
        private readonly StateHolder nearbyItemsState;
        
        [Atom] private bool IgnoreNearby => this.Widget.IgnoreNearby;

        [Atom] private InventoryItemFilter Filter { get; set; } = InventoryItemFilter.All;

        public static readonly Dictionary<InventoryItemFilter, HashSet<ItemTypes>> FilterTypes = new Dictionary<InventoryItemFilter, HashSet<ItemTypes>>() {
            { InventoryItemFilter.Armor, new HashSet<ItemTypes> { ItemTypes.Armor, ItemTypes.Helmet, ItemTypes.Backpack, ItemTypes.Headphones } },
            { InventoryItemFilter.Consumables, new HashSet<ItemTypes> 
                { ItemTypes.Ability, ItemTypes.Booster, ItemTypes.Perk, ItemTypes.AmmoBox, ItemTypes.HealBox, ItemTypes.RebirthTicket, ItemTypes.WeaponAttachment, ItemTypes.Stuff } },
            { InventoryItemFilter.Weapon, new HashSet<ItemTypes> { ItemTypes.Weapon, ItemTypes.Grenade } },
        };

        public override WidgetViewReference View => UiConstants.Views.GameInventory.Screen;

        public GameInventoryState() {
            this.slotItemState    = this.CreateChild(this.BuildSlotItems);
            this.trashItemState   = this.CreateChild(this.BuildTrashItems);
            this.nearbyItemsState = this.CreateChild(this.BuildNearbyItemsList);
            this.filtersState     = this.CreateChild(x => BuildFilters(x, this.Filter, this.OnFilterClick));
        }
        
        public bool ShowItemsThrowZone => this.Widget.ShowItemsThrowZone;

        public bool HasSomeLoadouts => this.Widget.HasSomeLoadouts;

        public float  CurrentItemsWeight => this.gameInventoryModel.CurrentItemWeight;
        public float  LimitItemsWeight   => this.gameInventoryModel.LimitItemsWeight;
        public int    LoadoutQuality     => this.gameInventoryModel.LoadoutQuality;

        public int LoadoutCount        => this.Widget.LoadoutCount;
        public int CurrentLoadoutIndex => this.Widget.LoadoutIndex;
        
        public IState SlotItems          => this.slotItemState.Value;
        public IState TrashItems         => this.trashItemState.Value;

        public IState Filters     => this.filtersState.Value;
        public IState NearbyItems => this.nearbyItemsState.Value;

        public bool IsEnoughSpace(DragAndDropPayloadItem payload) {
            return this.gameInventoryApi.IsEnoughSpace(payload);
        }
        
        public void Close() {
            this.Widget.OnClose?.Invoke();
            
            if (this.IgnoreNearby) {
                return;
            }
            
            if (this.gameNearbyItemsModel.OpenedNearbyItemEntity == EntityRef.None) {
                return;
            }

            this.photonService.Runner?.Game.SendCommand(new CloseItemBoxCommand());
        }
        
        public void OnIncrementLoadout() {
            this.Widget.OnIncrementLoadout?.Invoke();
        }
        
        public void OnDecrementLoadout() {
            this.Widget.OnDecrementLoadout?.Invoke();
        }

        public bool CanThrowItem(DragAndDropPayloadItem payload) {
            return this.gameInventoryApi.CanThrowItem(payload);
        }

        public void OnThrowItem(DragAndDropPayloadItem payload) {
            this.gameInventoryApi.ThrowItem(payload);
            
            App.Get<ISoundEffectService>()?.PlayOneShot(CoreConstants.SoundEffectKeys.THROW_ITEM);
        }

        private Widget BuildSlotItems(BuildContext context) {
            return new SimulationGameInventoryLoadoutHocWidget {
                NoDragging = this.Widget.NoDraggingInInventory,
            };
        }

        private Widget BuildTrashItems(BuildContext context) {
            return new GameInventoryTetrisWidget {
                UpdatedFrame = this.gameInventoryModel.UpdatedFrame,
                Items = this.gameInventoryModel.EnumerateTrashItems()
                    .ToList(),
                Width                 = this.gameInventoryModel.LoadoutWidth,
                Height                = this.gameInventoryModel.LoadoutHeight,
                NoDraggingInInventory = this.Widget.NoDraggingInInventory,
                Source                = TetrisSource.Inventory,
                InShop                = false,
                MaxHeight             = 10,
            };
        }

        private Widget BuildNearbyItemsList(BuildContext context) {
            if (this.IgnoreNearby) {
                return new Empty();
            }

            var hasNearbyItemBox  = this.gameNearbyItemsModel.NearbyItemBox.Entity != EntityRef.None;
            var hasNearbyBackpack = this.gameNearbyItemsModel.NearbyBackpack.Entity != EntityRef.None;

            return new ScrollGridFlow {
                MaxCrossAxisCount = 1,
                MainAxisAlignment = MainAxisAlignment.Start,
                CrossAxisAlignment = CrossAxisAlignment.Center,
                Children = {
                    (when: hasNearbyItemBox, () => this.BuildNearbyItemBoxItem()),
                    (when: hasNearbyBackpack, () => this.BuildNearbyBackpackItem()),
                },
            };
        }

        private Widget BuildNearbyItemBoxItem() {
            return new GameNearbyItemsWidget {
                ItemBoxModel = this.gameNearbyItemsModel.NearbyItemBox,
            };
        }

        private Widget BuildNearbyBackpackItem() {
            return new GameNearbyItemsWidget {
                ItemBoxModel = this.gameNearbyItemsModel.NearbyBackpack,
            };
        }

        private bool FilterItems(GameInventoryTrashItemModel arg) {
            if (this.Filter == InventoryItemFilter.All) {
                return true;
            }
            
            var item = photonService.PredictedFrame!.Get<Item>(arg.ItemEntity);

            var asset = photonService.PredictedFrame!.FindAsset(item.Asset);

            return FilterItems(this.Filter, asset);
        }
        
        public static bool FilterItems(InventoryItemFilter filter, ItemAsset itemAsset) {
            if (filter == InventoryItemFilter.All) {
                return true;
            }
            
            return FilterTypes[filter].Contains(itemAsset.ItemType);
        }

        public static Widget BuildFilters(BuildContext context, InventoryItemFilter filter, Action<InventoryItemFilter> onClick) {
            return new Column {
                MainAxisAlignment  = MainAxisAlignment.Start,
                CrossAxisAlignment = CrossAxisAlignment.Start,
                Size               = WidgetSize.Stretched,
                Children = {
                    BuildFilter(filter, InventoryItemFilter.All, false, onClick),
                    BuildFilter(filter, InventoryItemFilter.Armor, false, onClick),
                    BuildFilter(filter, InventoryItemFilter.Weapon, false, onClick),
                    BuildFilter(filter, InventoryItemFilter.Consumables, false, onClick),
                },
            };
        }
        
        public static Widget BuildFilter(InventoryItemFilter currentFilter, InventoryItemFilter filter, bool isRight, Action<InventoryItemFilter> onClick) {
            return new InventoryItemFilterWidget {
                FilterKey  = filter,
                IsRight    = isRight,
                IsSelected = filter == currentFilter,
                OnClick    = () => onClick(filter),
            };
        }
        
        private void OnFilterClick(InventoryItemFilter filter) {
            if (this.Filter == filter) {
                return;
            }

            this.Filter = filter;
        }
    }
}