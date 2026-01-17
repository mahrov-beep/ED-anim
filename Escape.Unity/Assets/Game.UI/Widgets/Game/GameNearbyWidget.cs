namespace Game.UI.Widgets.Game {
    using Domain.GameInventory;
    using ECS.Systems.GameInventory;
    using Multicast;
    using Quantum;
    using UniMob.UI;
    using UniMob.UI.Widgets;
    using UnityEngine;

    [RequireFieldsInit]
    public class GameNearbyWidget : StatefulWidget {
    }

    public class GameNearbyState : HocState<GameNearbyWidget> {
        [Inject] private GameNearbyInteractiveZoneModel interactiveZoneModel;

        public override Widget Build(BuildContext context) {
            return new VerticalSplitBox {
                FirstChild = this.BuildNearbyInteractiveZone(),
                SecondChild = new ScrollGridFlow {
                    MaxCrossAxisExtent = 1010,
                    Children = {
                        this.BuildNearbyItemsList(),
                    },
                },
            };
        }

        private Widget BuildNearbyInteractiveZone() {
            if (this.interactiveZoneModel.NearbyInteractiveZone == EntityRef.None) {
                return new Container {
                    Size = WidgetSize.FixedHeight(0),
                };
            }

            return new NearbyInteractiveZoneWidget();
        }

        private Widget BuildNearbyItemsList() {
            return new NearbyItemsBoxWidget();
        }
    }
}