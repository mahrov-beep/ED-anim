namespace Game.UI.Widgets.Rewards {
    using System.Collections.Generic;
    using System.Linq;
    using Multicast.Numerics;
    using UniMob.UI;
    using UniMob.UI.Widgets;

    [RequireFieldsInit]
    public class RewardsRowWidget : StatefulWidget {
        public List<Reward>      Rewards;
        public MainAxisAlignment Alignment     { get; set; } = MainAxisAlignment.Start;
        public AxisSize          MainAxisSize  { get; set; } = AxisSize.Max;
        public AxisSize          CrossAxisSize { get; set; } = AxisSize.Max;
    }

    public class RewardsRowState : HocState<RewardsRowWidget> {
        public override Widget Build(BuildContext context) {
            return new Row {
                MainAxisSize       = this.Widget.MainAxisSize,
                CrossAxisSize      = this.Widget.CrossAxisSize,
                MainAxisAlignment  = this.Widget.Alignment,
                CrossAxisAlignment = CrossAxisAlignment.Center,

                Children = {
                    this.Widget.Rewards.Select(it => this.BuildReward(it)),
                },
            };
        }

        private Widget BuildReward(Reward reward) {
            return new RewardWidget {
                Reward = reward,
                Key    = Key.Of(reward),
            };
        }
    }
}