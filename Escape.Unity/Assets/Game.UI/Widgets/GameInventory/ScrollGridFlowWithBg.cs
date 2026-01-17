namespace Game.UI.Widgets.GameInventory {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using UniMob.UI;
    using UniMob.UI.Widgets;

    [RequireFieldsInit]
    public class ScrollGridFlowWithBg : StatefulWidget {
        public Func<List<Widget>> BackgroundBuilder { get; set; }
        public Func<List<Widget>> ChildrenBuilder { get; set; }
        public int MaxCrossAxisCount { get; set; }
        public MainAxisAlignment MainAxisAlignment { get; set; }
        public CrossAxisAlignment CrossAxisAlignment { get; set; }
    }

    public class ScrollGridFlowWithBgState : HocState<ScrollGridFlowWithBg> {
        public override Widget Build(BuildContext context) {
            return new ScrollGridFlow {
                MainAxisAlignment  = this.Widget.MainAxisAlignment,
                CrossAxisAlignment = this.Widget.CrossAxisAlignment,
                MaxCrossAxisCount  = this.Widget.MaxCrossAxisCount,
                ChildrenBuilder    = this.Widget.ChildrenBuilder,
                BackgroundContent = new GridFlow {
                    MainAxisAlignment  = this.Widget.MainAxisAlignment,
                    CrossAxisAlignment = this.Widget.CrossAxisAlignment,
                    MaxCrossAxisCount  = this.Widget.MaxCrossAxisCount,
                    ChildrenBuilder    = this.Widget.BackgroundBuilder,
                },
            };
        }
    }
}

