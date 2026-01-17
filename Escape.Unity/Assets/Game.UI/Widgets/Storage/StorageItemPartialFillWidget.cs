namespace Game.UI.Widgets.Storage {
    using System;
    using UniMob.UI;
    using Views.Storage;

    [RequireFieldsInit]
    public class StorageItemPartialFillWidget : StatefulWidget {
        public int CurrentParts;
        public int RequiredParts;

        public bool Notify;

        public Action OnClick;
    }

    public class StorageItemPartialFillState : ViewState<StorageItemPartialFillWidget>, IStorageItemPartialFillState {
        public override WidgetViewReference View => UiConstants.Views.Storage.ItemPartialFill;

        public int CurrentParts  => this.Widget.CurrentParts;
        public int RequiredParts => this.Widget.RequiredParts;

        public bool Notify => this.Widget.Notify;

        public void OnClick() {
            this.Widget.OnClick?.Invoke();
        }
    }
}