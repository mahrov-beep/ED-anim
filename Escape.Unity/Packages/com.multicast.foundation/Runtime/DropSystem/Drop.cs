namespace Multicast.DropSystem {
    using System;
    using System.Globalization;
    using JetBrains.Annotations;
    using Numerics;
    using Sirenix.OdinInspector;
    using UnityEngine;

    [Serializable, InlineProperty]
    public struct Drop {
        [PublicAPI]
        public static Drop None => Drop.LootBox("none", "none", Array.Empty<Drop>());
        
        [SerializeField, Required]
        [BoxGroup, LabelWidth(80)]
        private string itemType;

        [SerializeField, Required]
        [BoxGroup, LabelWidth(80)]
        private string itemKey;

        [SerializeField, Required]
        [BoxGroup, LabelWidth(80)]
        private DropAmountType amountType;

        [ShowIf("@amountType == DropAmountType.Int")]
        [BoxGroup, LabelWidth(80), LabelText("Amount")]
        [SerializeField, Required]
        private ProtectedInt intAmount;

        [ShowIf("@amountType == DropAmountType.BigDouble")]
        [BoxGroup, LabelWidth(80), LabelText("Amount")]
        [SerializeField, Required]
        private BigDouble bigDoubleAmount;

        [ShowIf("@amountType == DropAmountType.LootBox")]
        [BoxGroup, LabelWidth(80), LabelText("Drops")]
        [SerializeField, Required]
        private Drop[] lootBoxDrops;

        [PublicAPI]
        public string GetItemType() => this.itemType;

        [PublicAPI]
        public string ItemKey => this.itemKey;

        [PublicAPI]
        public DropAmountType AmountType => this.amountType;

        [PublicAPI]
        public int IntAmount {
            get {
                if (this.amountType != DropAmountType.Int) {
                    throw new InvalidOperationException($"Cannot get IntAmount for {this.amountType} drop");
                }

                return this.intAmount.Value;
            }
        }

        [PublicAPI]
        public BigDouble BigDoubleAmount {
            get {
                if (this.amountType != DropAmountType.BigDouble) {
                    throw new InvalidOperationException($"Cannot get BigDoubleAmount for {this.amountType} drop");
                }

                return this.bigDoubleAmount;
            }
        }

        [PublicAPI]
        public Drop[] LootBoxDrops {
            get {
                if (this.amountType != DropAmountType.LootBox) {
                    throw new InvalidOperationException($"Cannot get LootBoxDrops for {this.amountType} drop");
                }

                return this.lootBoxDrops;
            }
        }

        private Drop(string itemType, string itemKey, DropAmountType dropAmountType,
            ProtectedInt intAmount = default,
            BigDouble bigDoubleAmount = default,
            Drop[] lootBoxDrops = null) {
            this.itemType        = itemType;
            this.itemKey         = itemKey;
            this.amountType      = dropAmountType;
            this.intAmount       = intAmount;
            this.bigDoubleAmount = bigDoubleAmount;
            this.lootBoxDrops    = lootBoxDrops;
        }

        [PublicAPI]
        public string GetStringAmount() => this.amountType switch {
            DropAmountType.Int => this.intAmount.Value.ToString(CultureInfo.InvariantCulture),
            DropAmountType.BigDouble => BigString.ToString(this.bigDoubleAmount),
            DropAmountType.LootBox => this.lootBoxDrops.Length.ToString(CultureInfo.InvariantCulture),
            _ => string.Empty,
        };

        [PublicAPI]
        public bool ItemTypeIs(string type) => this.itemType.Equals(type, StringComparison.InvariantCulture);

        [PublicAPI]
        public bool AmountTypeIs(DropAmountType type) => this.AmountType == type;

        [PublicAPI]
        public bool IsNone() {
            return this.AmountTypeIs(None.AmountType) && this.ItemTypeIs(None.GetItemType()) && this.itemKey == None.itemKey;
        }

        public override string ToString() {
            switch (this.amountType) {
                case DropAmountType.Int:
                    return $"[Drop: type={this.itemType}, key={this.itemKey}, intAmount={this.intAmount}]";
                case DropAmountType.BigDouble:
                    return $"[Drop: type={this.itemType}, key={this.itemKey}, bigDoubleAmount={this.bigDoubleAmount}]";
                case DropAmountType.LootBox:
                    var dropsToString = Array.ConvertAll(this.lootBoxDrops, d => d.ToString());
                    return $"[Drop: type={this.itemType}, key={this.itemKey}, lootBoxDrops={string.Join(", ", dropsToString)}]";
                default:
                    return base.ToString();
            }
        }

        [PublicAPI]
        public static Drop Int(string itemType, string itemKey, int amount) =>
            new Drop(itemType, itemKey, DropAmountType.Int, intAmount: amount);

        [PublicAPI]
        public static Drop BigDouble(string itemType, string itemKey, BigDouble amount) =>
            new Drop(itemType, itemKey, DropAmountType.BigDouble, bigDoubleAmount: amount);

        [PublicAPI]
        public static Drop LootBox(string itemType, string itemKey, Drop[] drops) =>
            new Drop(itemType, itemKey, DropAmountType.LootBox, lootBoxDrops: drops ?? Array.Empty<Drop>());

        [PublicAPI]
        public static Drop FromReward(Reward reward) => reward.AmountType switch {
            RewardAmountType.Int => Drop.Int(reward.GetItemType(), reward.ItemKey, reward.IntAmount),
            RewardAmountType.BigDouble => Drop.BigDouble(reward.GetItemType(), reward.ItemKey, reward.BigDoubleAmount),
            RewardAmountType.LootBox => Drop.LootBox(reward.GetItemType(), reward.ItemKey, Array.ConvertAll(reward.LootBoxRewards, FromReward)),
            _ => throw new ArgumentException($"Unexpected reward type '{reward.AmountType}'"),
        };
    }

    public enum DropAmountType {
        Int       = 1,
        BigDouble = 2,
        LootBox   = 3,
    }
}