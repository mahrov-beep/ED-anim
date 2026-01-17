namespace Game.UI.Widgets.Game.Hud {
    using System.Collections.Generic;
    using Multicast;
    using UniMob.UI;
    using UniMob.UI.Widgets;

    public class WeaponAmmoOptionsWidget : StatefulWidget {
        public WeaponAmmoOptionWidget[] Options;
        public float                     Spacing;
        public float                     PaddingBottom;
    }

    public class WeaponAmmoOptionsState : HocState<WeaponAmmoOptionsWidget> {
        public override Widget Build(BuildContext context) {
            if (this.Widget.Options == null || this.Widget.Options.Length == 0) {
                return new Empty();
            }

            var spacing = this.Widget.Spacing;
            var paddingBottom = this.Widget.PaddingBottom;
            var hasSpacing = spacing > 0f;
            var hasPadding = paddingBottom > 0f;
            var children = new List<Widget>(this.Widget.Options.Length * 2);

            for (var i = 0; i < this.Widget.Options.Length; i++) {
                var option = this.Widget.Options[i];
                if (option == null) {
                    continue;
                }

                if (children.Count > 0 && hasSpacing) {
                    children.Add(new Container {
                        Size = WidgetSize.FixedHeight(spacing),
                    });
                }

                children.Add(option);
            }

            if (children.Count > 0 && hasPadding) {
                children.Add(new Container {
                    Size = WidgetSize.FixedHeight(paddingBottom),
                });
            }

            return new Column {
                CrossAxisAlignment = CrossAxisAlignment.Center,
                Children           = children,
            };
        }
    }
}
