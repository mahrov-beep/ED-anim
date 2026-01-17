namespace Game.UI.Widgets.Game.Hud {
    using Domain.GameInventory;
    using Multicast;
    using UniMob.UI;
    using UniMob.UI.Widgets;
    using Views.Game;
    using UnityEngine;

    [RequireFieldsInit]
    public class OpenInventoryIndicatorWidget : StatefulWidget {
    }

    public class OpenInventoryIndicatorState : ViewState<OpenInventoryIndicatorWidget>, IOpenInventoryIndicatorState {
        [Inject] private GameInventoryModel gameInventoryModel;

        public override WidgetViewReference View => default;

        public float InventoryFillNormalized {
            get {
                var limit = this.InventoryWeightLimit;
                if (limit <= 0f) {
                    return InventoryCurrentWeight > 0f ? 1f : 0f;
                }

                return Mathf.Clamp01(InventoryCurrentWeight / limit);
            }
        }

        public float InventoryCurrentWeight => this.gameInventoryModel.CurrentItemWeight;

        public float InventoryWeightLimit => this.gameInventoryModel.LimitItemsWeight;
    }
}
