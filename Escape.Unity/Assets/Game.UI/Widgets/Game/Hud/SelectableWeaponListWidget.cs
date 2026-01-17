namespace Game.UI.Widgets.Game {
    using Domain.GameInventory;
    using Hud;
    using Multicast;
    using Quantum;
    using UniMob.UI;
    using UniMob.UI.Widgets;

    [RequireFieldsInit]
    public class SelectableWeaponListWidget : StatefulWidget {
    }

    public class SelectableWeaponListState : HocState<SelectableWeaponListWidget> {
        [Inject] private GameInventoryModel gameInventoryModel;

        public override Widget Build(BuildContext context) {
            this.gameInventoryModel.TryGetSlotItem(CharacterLoadoutSlots.PrimaryWeapon, out var primary);
            this.gameInventoryModel.TryGetSlotItem(CharacterLoadoutSlots.SecondaryWeapon, out var secondary);
            // this.gameInventoryModel.TryGetSlotItem(CharacterLoadoutSlots.MeleeWeapon, out var melee);

            return new Column {
                CrossAxisAlignment = CrossAxisAlignment.Center,
                Children = {
                    new Row {
                        MainAxisAlignment  = MainAxisAlignment.Center,
                        CrossAxisAlignment = CrossAxisAlignment.Center,
                        Children = {
                            this.BuildInventory(primary),
                            this.BuildInventory(secondary),
                            // this.BuildInventory(melee),
                        },
                    },
                },
            };
        }

        private Widget BuildInventory(GameInventorySlotItemModel model) {
            if (model == null) {
                return new Empty();
            }

            var weaponModel = this.gameInventoryModel.GetWeaponModel(model.SlotType)!;

            return new SelectableWeaponWidget {
                Model       = model,
                WeaponModel = weaponModel,
            };
        }
    }
}