namespace Game.UI.Widgets.Items {
    using UniMob.UI;
    using Views.items;

    [RequireFieldsInit]
    public class EmptyItemAttachmentMarkerWidget : StatefulWidget {
    }

    public class EmptyItemAttachmentMarkerState : ViewState<EmptyItemAttachmentMarkerWidget>, IEmptyItemAttachmentMarkerState {
        public override WidgetViewReference View => UiConstants.Views.Items.EmptyAttachmentMarker;
    }
}