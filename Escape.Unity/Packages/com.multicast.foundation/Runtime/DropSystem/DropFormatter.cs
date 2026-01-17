namespace Multicast.DropSystem {
    using System;
    using MessagePack;
    using MessagePack.Formatters;
    using Numerics;

    public class DropFormatter : IMessagePackFormatter<Drop> {
        public static readonly DropFormatter Instance = new DropFormatter();

        public void Serialize(ref MessagePackWriter writer, Drop value, MessagePackSerializerOptions options) {
            writer.WriteArrayHeader(4);

            writer.Write((int) value.AmountType);
            writer.Write(value.GetItemType());
            writer.Write(value.ItemKey);

            switch (value.AmountType) {
                case DropAmountType.Int:
                    writer.Write(value.IntAmount);
                    break;

                case DropAmountType.BigDouble:
                    options.Resolver.GetFormatter<BigDouble>().Serialize(ref writer, value.BigDoubleAmount, options);
                    break;

                case DropAmountType.LootBox:
                    writer.WriteArrayHeader(value.LootBoxDrops.Length);

                    foreach (var lookBox in value.LootBoxDrops) {
                        this.Serialize(ref writer, lookBox, options);
                    }

                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public Drop Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options) {
            var size = reader.ReadArrayHeader();

            if (size != 4) {
                throw new Exception("invalid serialized drop");
            }

            var amountType = (DropAmountType) reader.ReadInt32();
            var itemType   = reader.ReadString();
            var itemKey    = reader.ReadString();

            switch (amountType) {
                case DropAmountType.Int:
                    var intAmount = reader.ReadInt32();
                    return Drop.Int(itemType, itemKey, intAmount);

                case DropAmountType.BigDouble:
                    var bigDoubleAmount = options.Resolver.GetFormatter<BigDouble>().Deserialize(ref reader, options);
                    return Drop.BigDouble(itemType, itemKey, bigDoubleAmount);

                case DropAmountType.LootBox:
                    var lootBoxCount = reader.ReadArrayHeader();
                    var lootBoxes    = new Drop[lootBoxCount];

                    for (var i = 0; i < lootBoxes.Length; i++) {
                        lootBoxes[i] = this.Deserialize(ref reader, options);
                    }

                    return Drop.LootBox(itemType, itemKey, lootBoxes);

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}