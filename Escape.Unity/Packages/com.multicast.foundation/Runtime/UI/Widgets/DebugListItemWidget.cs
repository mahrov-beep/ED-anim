namespace Multicast.UI.Widgets {
    using System;
    using UniMob.UI;
    using UnityEngine;
    using Views;

    public class DebugListItemWidget : StatefulWidget {
        public string PrimaryText   { get; set; } = string.Empty;
        public string SecondaryText { get; set; } = string.Empty;

        public Color PrimaryTextColor   { get; set; } = Color.black;
        public Color SecondaryTextColor { get; set; } = Color.black;

        public Action OnClick { get; set; }

        public override State CreateState() => new DebugListItemState();
    }

    internal class DebugListItemState : ViewState<DebugListItemWidget>, IDebugListItemState {
        public override WidgetViewReference View => WidgetViewReference.Resource("Views/DebugListItem View");

        public string PrimaryText   => this.Widget.PrimaryText;
        public string SecondaryText => this.Widget.SecondaryText;

        public Color PrimaryTextColor   => this.Widget.PrimaryTextColor;
        public Color SecondaryTextColor => this.Widget.SecondaryTextColor;

        public void OnClick() => this.Widget.OnClick?.Invoke();
    }
}