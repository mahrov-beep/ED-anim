using MessagePack;
using Multicast.Numerics;

[assembly: MessagePackKnownFormatter(typeof(ProtectedIntFormatter))]

namespace Multicast.Numerics {
    using MessagePack;
    using MessagePack.Formatters;

    public class ProtectedIntFormatter : IMessagePackFormatter<ProtectedInt> {
        public static readonly ProtectedIntFormatter Instance = new ProtectedIntFormatter();

        private ProtectedIntFormatter() {
        }

        public void Serialize(ref MessagePackWriter writer, ProtectedInt value, MessagePackSerializerOptions options) {
            writer.Write(value.Value);
        }

        public ProtectedInt Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options) {
            return reader.ReadInt32();
        }
    }
}