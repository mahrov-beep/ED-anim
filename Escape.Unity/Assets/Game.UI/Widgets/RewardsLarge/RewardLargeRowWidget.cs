namespace Game.UI.Widgets.RewardsLarge {
    using System.Collections.Generic;
    using System.Linq;
    using Multicast.Numerics;
    using UniMob.UI;
    using UniMob.UI.Widgets;

    [RequireFieldsInit]
    public class RewardLargeRowWidget : StatefulWidget {
        public List<Reward> Rewards;

        public AxisSize           MainAxisSize       { get; set; } = AxisSize.Min;
        public AxisSize           CrossAxisSize      { get; set; } = AxisSize.Min;
        public MainAxisAlignment  MainAxisAlignment  { get; set; } = MainAxisAlignment.Start;
        public CrossAxisAlignment CrossAxisAlignment { get; set; } = CrossAxisAlignment.Start;
    }

    public class RewardLargeRowState : HocState<RewardLargeRowWidget> {
        public override Widget Build(BuildContext context) {
            return new Row {
                MainAxisSize       = this.Widget.MainAxisSize,
                CrossAxisSize      = this.Widget.CrossAxisSize,
                MainAxisAlignment  = this.Widget.MainAxisAlignment,
                CrossAxisAlignment = this.Widget.CrossAxisAlignment,

                Children = {
                    this.Widget.Rewards.Select(it => this.BuildReward(it)),
                },
            };
        }

        private Widget BuildReward(Reward reward) {
            return new RewardLargeWidget {
                Reward = reward,
                Key    = Key.Of(reward),
            };
        }
    }
}