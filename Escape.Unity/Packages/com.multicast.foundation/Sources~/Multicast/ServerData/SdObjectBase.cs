using JetBrains.Annotations;
using MessagePack;
using System.Buffers;

namespace Multicast.ServerData {
    public abstract class SdObjectBase {
        [CanBeNull] private readonly ISdObjectTracker tracker;
        [CanBeNull] private readonly object           trackerObj;

        internal readonly SdKey MyKey;

        protected SdObjectBase(SdArgs args) {
            this.tracker    = args.Tracker;
            this.trackerObj = this.tracker?.Create(this) ?? null;

            this.MyKey = args.Key;

            args.Parent?.AddChild(args.Key, this);
        }

        protected internal string GetSdObjectKey() {
            return this.MyKey.ToString();
        }

        internal abstract void Serialize(ref MessagePackWriter writer, MessagePackSerializerOptions options);
        internal abstract void Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options);

        protected abstract void AddChild(SdKey key, SdObjectBase child);

        internal SdArgs ChildInternal(SdKey key) {
            return new SdArgs(this, key, this.tracker);
        }

        protected void TrackRead() {
            this.tracker?.OnRead(this.trackerObj);
        }

        protected void TrackWrite() {
            this.tracker?.OnWrite(this.trackerObj);
        }

        protected void AssertOnWrite() {
            this.tracker?.AssertOnWrite(this.trackerObj);
        }

        public override string ToString() {
            var stream = new ArrayBufferWriter<byte>();
            var writer = new MessagePackWriter(stream);
            this.Serialize(ref writer, MessagePackSerializer.DefaultOptions);
            writer.Flush();
            return MessagePackSerializer.ConvertToJson(stream.WrittenMemory);
        }
    }
}