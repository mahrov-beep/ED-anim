namespace Game.UI.Widgets.Game {
    using System.Collections.Generic;
    using Domain.GameInventory;
    using Multicast;
    using Quantum;
    using Services.Photon;
    using UniMob.UI;
    using UniMob.UI.Widgets;

    [RequireFieldsInit]
    public class NearbyItemsBoxWidget : StatefulWidget {
    }

    public class NearbyItemsBoxState : HocState<NearbyItemsBoxWidget> {
        [Inject] private PhotonService        photonService;
        [Inject] private GameNearbyItemsModel gameNearbyItemsModel;

        public override Widget Build(BuildContext context) {
            var widgets = new List<Widget>();

            if (this.gameNearbyItemsModel.NearbyItemBox.Entity != EntityRef.None) {
                widgets.Add(new GameNearbyItemBoxWidget {
                    ItemBoxModel = this.gameNearbyItemsModel.NearbyItemBox,
                });
            }

            if (this.gameNearbyItemsModel.NearbyBackpack.Entity != EntityRef.None) {
                widgets.Add(new GameNearbyItemBoxWidget {
                    ItemBoxModel = this.gameNearbyItemsModel.NearbyBackpack,
                });
            }

            return new Column {
                Children = {
                    widgets,
                },
            };
        }
    }
}