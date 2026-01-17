namespace Game.Shared.UserProfile.Commands.Rewards {
    using Multicast.DropSystem;
    using Multicast.Numerics;

    public static class RewardBuildUtilityHelper {
        public static Reward FromDrop(Drop drop) {
            return drop.AmountType switch {
                DropAmountType.Int => Reward.Int(drop.GetItemType(), drop.ItemKey, drop.IntAmount),
                DropAmountType.BigDouble => Reward.BigDouble(drop.GetItemType(), drop.ItemKey, drop.BigDoubleAmount),
            };
        }
    }
}