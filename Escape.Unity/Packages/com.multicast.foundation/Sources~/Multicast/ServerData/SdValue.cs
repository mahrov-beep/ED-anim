using MessagePack;
using System;

namespace Multicast.ServerData {
    public class SdValue<T> : SdObjectBase {
        private T value;

        public SdValue(SdArgs args, T initialValue) : base(args) {
            this.value = initialValue;
        }

        public T Value {
            get {
                this.TrackRead();
                return this.value;
            }

            set {
                this.AssertOnWrite();
                this.value = value;
                this.TrackWrite();
            }
        }

        internal sealed override void Serialize(ref MessagePackWriter writer, MessagePackSerializerOptions options) {
            this.SerializeValue(ref writer, options, this.value);
        }

        internal sealed override void Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options) {
            this.value = this.DeserializeValue(ref reader, options);

            this.TrackWrite();
        }

        protected virtual void SerializeValue(ref MessagePackWriter writer, MessagePackSerializerOptions options, T value) {
            options.Resolver.GetFormatterWithVerify<T>().Serialize(ref writer, value, options);
        }

        protected virtual T DeserializeValue(ref MessagePackReader reader, MessagePackSerializerOptions options) {
            return options.Resolver.GetFormatterWithVerify<T>().Deserialize(ref reader, options);
        }

        protected sealed override void AddChild(SdKey key, SdObjectBase obj) {
            throw new InvalidOperationException("SdValueCore cannot contains children");
        }

        public override string ToString() {
            return base.ToString();
        }

        public static implicit operator SdValue<T>(SdArgs args) {
            return new SdValue<T>(args, SdValueDefaults<T>.DefaultValue);
        }
    }
}