namespace Game.UI.Widgets.Storage {
    using UniMob.UI;
    using Views.Storage;

    [RequireFieldsInit]
    public class StorageItemBlockerWidget : StatefulWidget {
        public string BlockerText;
    }

    public class StorageItemBlockerState : ViewState<StorageItemBlockerWidget>, IStorageItemBlockerState {
        public override WidgetViewReference View => UiConstants.Views.Storage.ItemBlocker;

        public string BlockerText => this.Widget.BlockerText;
    }
}