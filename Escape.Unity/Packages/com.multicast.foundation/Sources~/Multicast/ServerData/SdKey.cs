using Multicast.ServerData;

[assembly: MessagePack.MessagePackKnownFormatter(typeof(SdKey.SdKeyFormatter))]

namespace Multicast.ServerData {
    using System;
    using MessagePack;
    using MessagePack.Formatters;

    public readonly struct SdKey : IEquatable<SdKey> {
        private readonly uint   intKey;
        private readonly string stringKey;

        public SdKey(uint key) {
            this.intKey    = key;
            this.stringKey = null;
        }

        public SdKey(string key) {
            this.intKey    = 0;
            this.stringKey = key;
        }

        public bool TryGetIntKey(out uint result) {
            result = this.intKey;
            return this.stringKey == null;
        }

        public bool Equals(SdKey other) {
            return this.intKey == other.intKey && this.stringKey == other.stringKey;
        }

        public override bool Equals(object obj) {
            return obj is SdKey other && this.Equals(other);
        }

        public override int GetHashCode() {
            return HashCode.Combine(this.intKey, this.stringKey);
        }

        public override string ToString() {
            return this.stringKey ?? $"[sd-{this.intKey}]";
        }

        public static bool operator ==(SdKey a, SdKey b) {
            return a.Equals(b);
        }

        public static bool operator !=(SdKey a, SdKey b) {
            return !a.Equals(b);
        }

        public static implicit operator SdKey(uint key) {
            return new SdKey(key);
        }

        public static implicit operator SdKey(string key) {
            return new SdKey(key);
        }

        public class SdKeyFormatter : IMessagePackFormatter<SdKey> {
            public void Serialize(ref MessagePackWriter writer, SdKey value, MessagePackSerializerOptions options) {
                if (value.stringKey != null) {
                    writer.Write(value.stringKey);
                }
                else {
                    writer.Write(value.intKey);
                }
            }

            public SdKey Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options) {
                if (reader.NextMessagePackType == MessagePackType.String) {
                    var stringKey = reader.ReadString();
                    return new SdKey(stringKey);
                }

                var intKey = reader.ReadUInt32();
                return new SdKey(intKey);
            }
        }
    }
}