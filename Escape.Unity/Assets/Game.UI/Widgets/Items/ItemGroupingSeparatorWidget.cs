namespace Game.UI.Widgets.Items {
    using Multicast;
    using Quantum;
    using UniMob.UI;
    using Views.items;

    [RequireFieldsInit]
    public class ItemGroupingSeparatorWidget : StatefulWidget {
        public ItemAssetGrouping Grouping;

        public float? Width { get; set; }
    }

    public class ItemGroupingSeparatorState : ViewState<ItemGroupingSeparatorWidget>, IItemGroupingSeparatorState {
        public override WidgetViewReference View => UiConstants.Views.Items.GroupingSeparator;

        public string Grouping => EnumNames<ItemAssetGrouping>.GetName(this.Widget.Grouping);

        public override WidgetSize CalculateSize() {
            var size = base.CalculateSize();

            if (this.Widget.Width is { } width) {
                size = new WidgetSize(width, size.MinHeight, width, size.MaxHeight);
            }

            return size;
        }
    }
}