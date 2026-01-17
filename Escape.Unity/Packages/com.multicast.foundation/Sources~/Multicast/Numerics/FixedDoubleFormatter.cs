[assembly: MessagePack.MessagePackKnownFormatter(typeof(Multicast.Numerics.FixedDoubleFormatter))]

namespace Multicast.Numerics {
    using System;
    using MessagePack;
    using MessagePack.Formatters;

    public class FixedDoubleFormatter : IMessagePackFormatter<FixedDouble> {
        public static readonly FixedDoubleFormatter Instance = new FixedDoubleFormatter();

        private FixedDoubleFormatter() {
        }

        public void Serialize(ref MessagePackWriter writer, FixedDouble value, MessagePackSerializerOptions options) {
            writer.Write(value.rawValue);
        }

        public FixedDouble Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options) {
            if (reader.IsNil) {
                throw new InvalidOperationException("typecode is null, struct not supported");
            }

            return FixedDouble.FromRaw(reader.ReadInt64());
        }
    }
}