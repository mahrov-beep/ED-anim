using MessagePack;
using System;
using System.Buffers;

namespace Multicast.ServerData {
    using JetBrains.Annotations;

    public static class SdObjectSerializer {
        public static void Serialize([NotNull] SdObjectBase obj, IBufferWriter<byte> buffer, MessagePackSerializerOptions options) {
            if (obj == null) {
                throw new ArgumentNullException(nameof(obj));
            }

            var writer = new MessagePackWriter(buffer);
            Serialize(obj, ref writer, options);
            writer.Flush();
        }

        public static void Deserialize([NotNull] SdObjectBase obj, ReadOnlyMemory<byte> buffer, MessagePackSerializerOptions options) {
            if (obj == null) {
                throw new ArgumentNullException(nameof(obj));
            }

            var reader = new MessagePackReader(buffer);
            Deserialize(obj, ref reader, options);
        }

        public static void Serialize(SdObjectBase obj, ref MessagePackWriter writer, MessagePackSerializerOptions options) {
            if (obj is null) {
                throw new ArgumentNullException(nameof(obj));
            }

            obj.Serialize(ref writer, options);
        }

        public static void Deserialize(SdObjectBase obj, ref MessagePackReader reader, MessagePackSerializerOptions options) {
            if (obj is null) {
                throw new ArgumentNullException(nameof(obj));
            }

            obj.Deserialize(ref reader, options);
        }
    }
}