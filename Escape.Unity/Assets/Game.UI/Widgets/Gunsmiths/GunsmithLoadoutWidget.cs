namespace Game.UI.Widgets.Gunsmiths {
    using System;
    using System.Linq;
    using Controllers.Features.Gunsmith;
    using Domain.Currencies;
    using Domain.Gunsmiths;
    using Domain.Storage;
    using GameInventory;
    using GameInventory.Snapshot;
    using Multicast;
    using Multicast.Numerics;
    using Quantum;
    using Shared;
    using Shared.Balance;
    using UniMob;
    using UniMob.UI;
    using Views.Gunsmiths;

    [RequireFieldsInit]
    public class GunsmithLoadoutWidget : StatefulWidget {
        public string GunsmithKey;
        public string GunsmithLoadoutGuid;
    }

    public class GunsmithLoadoutState : ViewState<GunsmithLoadoutWidget>, IGunsmithLoadoutState {
        [Inject] private GunsmithsModel  gunsmithsModel;
        [Inject] private CurrenciesModel currenciesModel;
        [Inject] private StorageModel    storageModel;
        [Inject] private GameDef         gameDef;

        private readonly StateHolder loadoutState;

        public GunsmithLoadoutState() {
            this.loadoutState = this.CreateChild(this.BuildLoadout);
        }

        [Atom] private GunsmithModel GunsmithModel => this.gunsmithsModel.Get(this.Widget.GunsmithKey);

        public override WidgetViewReference View => UiConstants.Views.Gunsmiths.Loadout;

        public string GunsmithLoadoutKey => this.GunsmithModel.Data.Loadouts.Get(this.Widget.GunsmithLoadoutGuid).GunsmithLoadoutKey.Value;

        [Atom] public int LoadoutQuality => ItemBalance.GetLoadoutQuality(this.gameDef, this.GunsmithModel.Data.Loadouts.Get(this.Widget.GunsmithLoadoutGuid).Loadout.Value);

        [Atom] public bool CanBuy => this.currenciesModel.HasEnough(this.BuyCost);

        [Atom] public Cost BuyCost {
            get {
                var gunsmithLoadoutDef = this.gameDef.GunsmithLoadouts.Get(this.GunsmithLoadoutKey);
                return new IntCost(gunsmithLoadoutDef.buyCost);
            }
        }

        public IState Loadout => this.loadoutState.Value;

        public void Buy() {
            GunsmithFeatureEvents.BuyLoadout.Raise(new GunsmithFeatureEvents.BuyLoadoutArgs {
                gunsmithKey         = this.GunsmithModel.Key,
                gunsmithLoadoutGuid = this.Widget.GunsmithLoadoutGuid,
            });
        }

        private Widget BuildLoadout(BuildContext context) {
            var loadout = this.GunsmithModel.Data.Loadouts.Get(this.Widget.GunsmithLoadoutGuid).Loadout.Value;

            var visibleSlots = new[] {
                CharacterLoadoutSlots.PrimaryWeapon,
                CharacterLoadoutSlots.Helmet,
                CharacterLoadoutSlots.Armor,
                CharacterLoadoutSlots.Backpack,
            };

            return new SnapshotGameInventoryLoadoutHocWidget {
                LoadoutSnapshot = loadout,
                VisibleSlots    = visibleSlots,
                ExtraItems = Enumerable.Empty<GameSnapshotLoadoutItem>()
                    .Concat(LoadoutSlotExtensions.VisibleSlots.Except(visibleSlots).Select(slotType => GetFromSlot(slotType)))
                    .Concat(loadout.TrashItems ?? Array.Empty<GameSnapshotLoadoutItem>())
                    .Where(it => it != null)
                    .ToList(),
            };

            GameSnapshotLoadoutItem GetFromSlot(CharacterLoadoutSlots slotType) {
                return loadout.SlotItems != null &&
                       slotType.ToInt() < loadout.SlotItems.Length &&
                       loadout.SlotItems[slotType.ToInt()] is { } item
                    ? item
                    : null;
            }
        }
    }
}