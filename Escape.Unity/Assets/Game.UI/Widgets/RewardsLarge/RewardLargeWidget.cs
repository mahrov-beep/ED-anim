namespace Game.UI.Widgets.RewardsLarge {
    using Multicast.Numerics;
    using Shared;
    using UniMob.UI;
    using UniMob.UI.Widgets;
    using UnityEngine;

    [RequireFieldsInit]
    public class RewardLargeWidget : StatefulWidget {
        public Reward Reward;
    }

    public class RewardLargeState : HocState<RewardLargeWidget> {
        public override Widget Build(BuildContext context) {
            var reward = this.Widget.Reward;

            return reward.GetItemType() switch {
                SharedConstants.RewardTypes.CURRENCY => new RewardLargeCurrencyWidget {
                    CurrencyReward = reward,
                },
                SharedConstants.RewardTypes.FEATURE => new RewardLargeFeatureWidget {
                    FeatureReward = reward,
                },
                SharedConstants.RewardTypes.ITEM => new RewardLargeItemWidget {
                    ItemReward = reward,
                },
                _ => BuildUnknownReward(reward),
            };
        }

        private static Widget BuildUnknownReward(Reward reward) {
            Debug.LogError($"Reward of type {reward.GetItemType()} is not implemented in {nameof(RewardLargeWidget)}");
            return new Empty();
        }
    }
}