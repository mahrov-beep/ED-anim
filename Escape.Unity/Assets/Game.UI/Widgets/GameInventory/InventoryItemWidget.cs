namespace Game.UI.Widgets.GameInventory {
    using System;
    using UniMob.UI;
    using Views.GameInventory;

    public enum InventoryItemFilter {
        All         = 0,
        Armor       = 1,
        Weapon      = 2,
        Consumables = 3,
    }
    
    [RequireFieldsInit]
    public class InventoryItemFilterWidget : StatefulWidget {
        public bool IsRight    { get; set; }
        public bool IsSelected { get; set; }

        public InventoryItemFilter FilterKey { get; set; }

        public Action OnClick { get; set; }
    }

    public class InventoryItemFilterState : ViewState<InventoryItemFilterWidget>, IInventoryItemFilterState {
        public override WidgetViewReference View => UiConstants.Views.GameInventory.InventoryItemFilter;

        public bool IsRight    => this.Widget.IsRight;
        public bool IsSelected => this.Widget.IsSelected;

        public string FilterKey => this.Widget.FilterKey.ToString();
        
        public void OnClick() {
            this.Widget.OnClick?.Invoke();
        }
    }
}