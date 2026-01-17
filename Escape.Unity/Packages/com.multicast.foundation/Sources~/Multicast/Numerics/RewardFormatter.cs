using MessagePack;
using Multicast.Numerics;

[assembly: MessagePackKnownFormatter(typeof(RewardFormatter))]

namespace Multicast.Numerics {
    using System;
    using MessagePack;
    using MessagePack.Formatters;

    public class RewardFormatter : IMessagePackFormatter<Reward> {
        public static readonly RewardFormatter Instance = new RewardFormatter();

        public void Serialize(ref MessagePackWriter writer, Reward value, MessagePackSerializerOptions options) {
            if (value.IsNone) {
                writer.WriteNil();
                return;
            }

            writer.WriteArrayHeader(4);

            writer.Write((int)value.AmountType);
            writer.Write(value.GetItemType());
            writer.Write(value.ItemKey);

            switch (value.AmountType) {
                case RewardAmountType.Int:
                    writer.Write(value.IntAmount);
                    break;

                case RewardAmountType.BigDouble:
                    options.Resolver.GetFormatterWithVerify<BigDouble>().Serialize(ref writer, value.BigDoubleAmount, options);
                    break;

                case RewardAmountType.LootBox:
                    writer.WriteArrayHeader(value.LootBoxRewards.Length);

                    foreach (var lookBox in value.LootBoxRewards) {
                        this.Serialize(ref writer, lookBox, options);
                    }

                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public Reward Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options) {
            if (reader.TryReadNil()) {
                return Reward.None;
            }

            var size = reader.ReadArrayHeader();

            if (size != 4) {
                throw new Exception("invalid serialized drop");
            }

            var amountType = (RewardAmountType)reader.ReadInt32();
            var itemType   = reader.ReadString();
            var itemKey    = reader.ReadString();

            switch (amountType) {
                case RewardAmountType.Int:
                    var intAmount = reader.ReadInt32();
                    return Reward.Int(itemType, itemKey, intAmount);

                case RewardAmountType.BigDouble:
                    var bigDoubleAmount = options.Resolver.GetFormatterWithVerify<BigDouble>().Deserialize(ref reader, options);
                    return Reward.BigDouble(itemType, itemKey, bigDoubleAmount);

                case RewardAmountType.LootBox:
                    var lootBoxCount = reader.ReadArrayHeader();
                    var lootBoxes    = new Reward[lootBoxCount];

                    for (var i = 0; i < lootBoxes.Length; i++) {
                        lootBoxes[i] = this.Deserialize(ref reader, options);
                    }

                    return Reward.LootBox(itemType, itemKey, lootBoxes);

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}