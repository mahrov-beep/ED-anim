namespace Game.UI.Widgets.World {
    using ECS.Scripts;
    using UniMob.UI;
    using UnityEngine;
    using Views.World;

    public class ItemBoxTimerWidget : StatefulWidget {
        public ItemBoxTimerUiDynamicData Data { get; }

        public ItemBoxTimerWidget(ItemBoxTimerUiDynamicData data) {
            this.Data = data;
        }
    }

    public class ItemBoxTimerState : ViewState<ItemBoxTimerWidget>, IItemBoxTimerState {
        public override WidgetViewReference View => UiConstants.Views.World.ItemBoxTimerView;

        public Vector3 WorldPos => this.Widget.Data.WorldPos;
        public float   Progress => this.Widget.Data.Progress;
        public int     Timer    => this.Widget.Data.Timer;
    }
}