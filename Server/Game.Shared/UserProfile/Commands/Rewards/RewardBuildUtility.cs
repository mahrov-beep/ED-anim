namespace Game.Shared.UserProfile.Commands.Rewards {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Defs;
    using Multicast.Numerics;
    using Multicast.RewardSystem;

    public static class RewardBuildUtility {
        public static Reward BuildLootBox(string lootBoxType, string lootBoxKey, List<RewardDef> rewardDefs) {
            return Reward.LootBox(lootBoxType, lootBoxKey, rewardDefs.Select(it => Build(it)).ToArray());
        }

        public static Reward Build(RewardDef rewardDef) {
            return rewardDef switch {
                CurrencyRewardDef currencyRewardDef => BuildCurrency(currencyRewardDef),
                ItemRewardDef itemRewardDef => BuildItem(itemRewardDef),
                FeatureRewardDef featureRewardDef => BuildFeature(featureRewardDef),

                _ => throw new InvalidOperationException($"Unknown reward type: {rewardDef?.type}"),
            };
        }

        public static Reward Combine(string lootBoxKey, params Reward[] rewards) {
            return Reward.LootBox(SharedConstants.LootBoxTypes.COMBINE_ONLY, lootBoxKey, rewards);
        }

        public static Reward BuildCurrency(CurrencyRewardDef rewardDef) {
            return BuildCurrency(rewardDef.currency, rewardDef.amount);
        }

        public static Reward BuildCurrency(string currencyKey, int amount) {
            return Reward.Int(SharedConstants.RewardTypes.CURRENCY, currencyKey, amount);
        }

        public static Reward BuildItem(ItemRewardDef rewardDef) {
            return BuildItem(rewardDef.item);
        }

        public static Reward BuildItem(string itemKey) {
            return Reward.Int(SharedConstants.RewardTypes.ITEM, itemKey, amount: 1);
        }

        public static Reward BuildFeature(FeatureRewardDef rewardDef) {
            return Reward.Int(SharedConstants.RewardTypes.FEATURE, rewardDef.feature, 1);
        }

        public static Reward BuildExpOrNone(string expKey, int amount) {
            return amount == 0 ? Reward.None : Reward.Int(SharedConstants.RewardTypes.EXP, expKey, amount);
        }
    }
}