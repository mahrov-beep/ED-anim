namespace Multicast.UI.Widgets {
    using System.Collections.Generic;
    using Routes;
    using UniMob.UI;
    using UniMob.UI.Widgets;
    using UnityEngine;

    public class DebugListWidget : StatefulWidget {
        public string Title { get; }

        public DebugListWidget(string title) {
            this.Title = title;
        }

        public List<Widget> Items { get; set; } = new List<Widget>();

        public override State CreateState() => new DebugListState();
    }

    public class DebugListState : HocState<DebugListWidget> {
        public override Widget Build(BuildContext context) {
            return new DismissibleDialog {
                CollapsedHeight = 300,
                OnDismiss       = () => Navigator.Of(this.Context).Pop(),
                Child = new Container {
                    Size            = WidgetSize.Stretched,
                    BackgroundColor = Color.white,
                    Child = new ScrollGridFlow {
                        MaxCrossAxisCount = 1,
                        Children = {
                            new DebugListHeaderWidget(this.Widget.Title),
                            this.Widget.Items,
                        },
                    },
                },
            };
        }
    }
}