namespace Game.UI.Widgets.GameInventory.Simulation {
    using System.Linq;
    using Domain.GameInventory;
    using Domain.Safe;
    using Multicast;
    using Quantum;
    using UniMob.UI;
    using UniMob.UI.Widgets;

    [RequireFieldsInit]
    public class SimulationGameInventoryLoadoutHocWidget : StatefulWidget {
        public bool NoDragging;
        public int  UpdatedFrame { get; set; }
    }

    public class SimulationGameInventoryLoadoutHocState : HocState<SimulationGameInventoryLoadoutHocWidget> {
        [Inject] private GameInventoryModel gameInventoryModel;
        [Inject] private SafeModel          safeModel;

        public override Widget Build(BuildContext context) {
            var safe = this.BuildSafe();
            var slots = LoadoutSlotExtensions.VisibleSlots
                .Select(slotType => this.gameInventoryModel.TryGetSlotItem(slotType, out var model)
                    ? this.BuildSlotItem(model)
                    : this.BuildEmptySlotItem(slotType))
                .ToList();
            
            return new ScrollGridFlow() {
                MaxCrossAxisExtent = 600 + 20,
                Children = {
                    slots,
                    safe,
                },
            };
        }

        private Widget BuildEmptySlotItem(CharacterLoadoutSlots slotType) {
            return new GameInventoryEmptySlotItemWidget {
                Key      = Key.Of(EnumNames<CharacterLoadoutSlots>.GetName(slotType)),
                SlotType = slotType,
            };
        }

        private Widget BuildSlotItem(GameInventorySlotItemModel model) {
            return new SimulationGameInventorySlotItemWidget {
                Key        = Key.Of(model),
                Model      = model,
                NoDragging = this.Widget.NoDragging,
            };
        }

        private Widget BuildEmpty() {
            return new Empty();
        }

        private Widget BuildSafe() {
            var hasSafe = this.gameInventoryModel.TryGetSlotItem(CharacterLoadoutSlots.Safe, out var model);
            
            var safeSlot = hasSafe
                ? this.BuildSlotItem(model)
                : this.BuildEmptySlotItem(CharacterLoadoutSlots.Safe);

            var safeGrid = hasSafe
                ? new GameInventoryTetrisWidget {
                    UpdatedFrame          = this.Widget.UpdatedFrame,
                    NoDraggingInInventory = this.Widget.NoDragging,
                    Width                 = this.safeModel.Width,
                    Height                = this.safeModel.Height,
                    Items                 = this.safeModel.EnumerateItems(),
                    Source                = TetrisSource.Safe,
                    InShop                = false,
                    MaxHeight             = 10,
                }
                : this.BuildEmpty();

            return new Container() {
                Size = WidgetSize.Fixed(600 + 20, hasSafe ? 400 : 200),
                Child = new Row() {
                    Size               = WidgetSize.Fixed(600 + 20, hasSafe ? 400 : 200),
                    CrossAxisAlignment = CrossAxisAlignment.Center,
                    MainAxisAlignment  = MainAxisAlignment.Start,
                    Children = {
                        safeSlot,
                        safeGrid,
                    },
                },
            };
        }
    }
}