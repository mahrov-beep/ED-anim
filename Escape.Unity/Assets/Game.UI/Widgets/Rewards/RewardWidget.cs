namespace Game.UI.Widgets.Rewards {
    using System;
    using Items;
    using Multicast.Numerics;
    using Quantum;
    using Shared;
    using Storage.TraderShop;
    using UniMob.UI;
    using UniMob.UI.Widgets;

    [RequireFieldsInit]
    public class RewardWidget : StatefulWidget {
        public Reward Reward;
    }

    public class RewardState : HocState<RewardWidget> {
        public override Widget Build(BuildContext context) {
            var reward = this.Widget.Reward;

            switch (reward.GetItemType()) {
                case SharedConstants.RewardTypes.CURRENCY:
                    return new RewardCurrencyItemWidget {
                        Reward = reward,
                    };

                case SharedConstants.RewardTypes.EXP:
                    return new RewardExpItemWidget {
                        Reward = reward,
                    };
                
                case SharedConstants.RewardTypes.FEATURE:
                    return new RewardFeatureItemWidget {
                        Reward = reward,
                    };

                case SharedConstants.RewardTypes.ITEM:
                    return new RewardStorageItemWidget {
                        Item = new GameSnapshotLoadoutItem {
                            ItemKey               = reward.ItemKey,
                            ItemGuid              = Guid.Empty.ToString(),
                            WeaponAttachments     = null,
                            IndexI                = 0,
                            IndexJ                = 0,
                            Rotated               = false,
                            Used                  = 0,
                            SafeGuid              = null,
                            AddToLoadoutAfterFail = false,
                        },
                    };
            }

            return new Empty();
        }
    }
}