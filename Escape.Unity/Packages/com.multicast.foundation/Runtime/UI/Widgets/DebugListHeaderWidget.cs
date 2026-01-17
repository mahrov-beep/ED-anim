namespace Multicast.UI.Widgets {
    using UniMob.UI;
    using UniMob.UI.Widgets;

    public class DebugListHeaderWidget : StatefulWidget {
        public string Title { get; set; } = string.Empty;

        public DebugListHeaderWidget() {
        }

        public DebugListHeaderWidget(string title) {
            this.Title = title;
        }

        public override State CreateState() => new DebugListHeaderState();
    }

    internal class DebugListHeaderState : HocState<DebugListHeaderWidget> {
        public override Widget Build(BuildContext context) {
            return new UniMobText {
                Value              = this.Widget.Title,
                FontSize           = 30,
                Size               = WidgetSize.FixedHeight(40),
                MainAxisAlignment  = MainAxisAlignment.End,
                CrossAxisAlignment = CrossAxisAlignment.Center,
            };
        }
    }
}