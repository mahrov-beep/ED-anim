using MessagePack;
using Multicast.Numerics;

[assembly: MessagePackKnownFormatter(typeof(ProtectedBigDoubleFormatter))]

namespace Multicast.Numerics {
    using MessagePack;
    using MessagePack.Formatters;

    public class ProtectedBigDoubleFormatter : IMessagePackFormatter<ProtectedBigDouble> {
        public static readonly ProtectedBigDoubleFormatter Instance = new ProtectedBigDoubleFormatter();

        private ProtectedBigDoubleFormatter() {
        }

        public void Serialize(ref MessagePackWriter writer, ProtectedBigDouble value, MessagePackSerializerOptions options) {
            options.Resolver.GetFormatter<BigDouble>().Serialize(ref writer, value, options);
        }

        public ProtectedBigDouble Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options) {
            return options.Resolver.GetFormatter<BigDouble>().Deserialize(ref reader, options);
        }
    }
}