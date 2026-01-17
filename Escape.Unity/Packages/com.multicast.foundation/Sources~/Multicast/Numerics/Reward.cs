// ReSharper disable FieldCanBeMadeReadOnly.Local

namespace Multicast.Numerics {
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using JetBrains.Annotations;

    public struct Reward {
        private string           itemType;
        private string           itemKey;
        private RewardAmountType amountType;
        private ProtectedInt     intAmount;
        private BigDouble        bigDoubleAmount;
        private Reward[]         lootBoxRewards;

        [PublicAPI] public string GetItemType() => this.itemType;

        [PublicAPI] public string ItemKey => this.itemKey;

        [PublicAPI] public RewardAmountType AmountType => this.amountType;

        [PublicAPI] public int IntAmount => this.amountType == RewardAmountType.Int
            ? this.intAmount.Value
            : throw new InvalidOperationException($"Cannot get IntAmount for {this.amountType} reward");

        [PublicAPI] public BigDouble BigDoubleAmount => this.amountType == RewardAmountType.BigDouble
            ? this.bigDoubleAmount
            : throw new InvalidOperationException($"Cannot get BigDoubleAmount for {this.amountType} reward");

        [PublicAPI] public Reward[] LootBoxRewards => this.amountType == RewardAmountType.LootBox
            ? this.lootBoxRewards
            : throw new InvalidOperationException($"Cannot get LootBoxRewards for {this.amountType} reward");

        private Reward(string itemType, string itemKey, RewardAmountType amountType,
            ProtectedInt intAmount = default,
            BigDouble bigDoubleAmount = default,
            Reward[] lootBoxRewards = null) {
            this.itemType        = itemType;
            this.itemKey         = itemKey;
            this.amountType      = amountType;
            this.intAmount       = intAmount;
            this.bigDoubleAmount = bigDoubleAmount;
            this.lootBoxRewards  = lootBoxRewards;
        }

        [PublicAPI] public string GetStringAmount() => this.amountType switch {
            RewardAmountType.Int => this.intAmount.Value.ToString(CultureInfo.InvariantCulture),
            RewardAmountType.BigDouble => BigString.ToString(this.bigDoubleAmount),
            RewardAmountType.LootBox => this.lootBoxRewards.Length.ToString(CultureInfo.InvariantCulture),
            _ => string.Empty,
        };

        [PublicAPI] public bool ItemTypeIs(string type) => this.itemType.Equals(type, StringComparison.InvariantCulture);

        [PublicAPI] public bool AmountTypeIs(RewardAmountType type) => this.AmountType == type;

        [PublicAPI] public bool IsNone => this.AmountTypeIs(RewardAmountType.None);

        public override string ToString() => this.amountType switch {
            RewardAmountType.Int => $"[Reward: type={this.itemType}, key={this.itemKey}, intAmount={this.intAmount}]",
            RewardAmountType.BigDouble => $"[Reward: type={this.itemType}, key={this.itemKey}, bigDoubleAmount={this.bigDoubleAmount}]",
            RewardAmountType.LootBox => $"[Reward: type={this.itemType}, key={this.itemKey}, lootBoxRewards={string.Join(", ", Array.ConvertAll(this.lootBoxRewards, d => d.ToString()))}]",
            _ => base.ToString(),
        };

        [PublicAPI] public static Reward None => new Reward("none", "none", RewardAmountType.None);

        [PublicAPI] public static Reward Int(string itemType, string itemKey, int amount) =>
            new Reward(itemType, itemKey, RewardAmountType.Int, intAmount: amount);

        [PublicAPI] public static Reward BigDouble(string itemType, string itemKey, BigDouble amount) =>
            new Reward(itemType, itemKey, RewardAmountType.BigDouble, bigDoubleAmount: amount);

        [PublicAPI] public static Reward LootBox(string itemType, string itemKey, Reward[] rewards) =>
            new Reward(itemType, itemKey, RewardAmountType.LootBox, lootBoxRewards: rewards);

        [PublicAPI]
        public List<Reward> EnumerateAllRewards() {
            var list = new List<Reward>();
            EnumerateAllRewards(list, this);
            return list;
        }

        private static void EnumerateAllRewards(List<Reward> list, Reward reward) {
            if (reward.AmountType == RewardAmountType.LootBox) {
                foreach (var innerReward in reward.LootBoxRewards) {
                    EnumerateAllRewards(list, innerReward);
                }
            }
            else {
                list.Add(reward);
            }
        }
    }

    public enum RewardAmountType {
        None      = 0,
        Int       = 1,
        BigDouble = 2,
        LootBox   = 3,
    }
}