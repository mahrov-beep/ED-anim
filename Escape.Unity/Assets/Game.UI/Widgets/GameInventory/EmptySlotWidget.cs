namespace Game.UI.Widgets.GameInventory {
    using UniMob.UI;
    using Views.GameInventory;

    [RequireFieldsInit]
    public class EmptySlotWidget : StatefulWidget {
    }

    public class EmptySlotState : ViewState<EmptySlotWidget>, IEmptySlotState {
        public override WidgetViewReference View => UiConstants.Views.GameInventory.EmptySlotItem;
    }
}