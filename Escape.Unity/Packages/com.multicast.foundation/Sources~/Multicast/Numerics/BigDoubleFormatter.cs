[assembly: MessagePack.MessagePackKnownFormatter(typeof(Multicast.Numerics.BigDoubleFormatter))]

namespace Multicast.Numerics {
    using System;
    using MessagePack;
    using MessagePack.Formatters;

    public class BigDoubleFormatter : IMessagePackFormatter<BigDouble> {
        public static readonly BigDoubleFormatter Instance = new BigDoubleFormatter();

        private BigDoubleFormatter() {
        }

        private readonly BigDouble floatMaxValue = float.MaxValue;

        public void Serialize(ref MessagePackWriter writer, BigDouble value, MessagePackSerializerOptions options) {
            if (value < this.floatMaxValue) {
                writer.Write(value.ToFloatUnsafe());
            }
            else {
                writer.Write(value.ToString());
            }
        }

        public BigDouble Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options) {
            if (reader.IsNil) {
                throw new InvalidOperationException("typecode is null, struct not supported");
            }

            if (reader.NextMessagePackType == MessagePackType.Float) {
                var f = reader.ReadSingle();
                return f;
            }

            var str = reader.ReadString();
            return BigDouble.Parse(str);
        }
    }
}