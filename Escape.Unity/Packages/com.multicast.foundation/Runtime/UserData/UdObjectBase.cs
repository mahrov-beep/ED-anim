namespace Multicast.UserData {
    using System;
    using System.Buffers;
    using MessagePack;
    using UniMob;
    using UnityEngine;
    using UnityEngine.Pool;

    public abstract class UdObjectBase {
        private readonly MutableAtom<int> version;

        internal readonly UdObjectBase MyParent;
        internal readonly string       MyKey;

        internal event Action SelfChanged;

        protected UdObjectBase(UdArgs args) {
            this.MyParent = args.Parent;
            this.MyKey    = args.Key;

            // ReSharper disable once VirtualMemberCallInConstructor
            this.version = Atom.Value(this.TransactionVersion);

            this.MyParent?.AddChild(args.Key, this);
        }

        protected internal string GetUdObjectKey() {
            return this.MyKey;
        }

        internal void NotifySelfChanged() {
            this.version.Value = this.TransactionVersion;
            this.SelfChanged?.Invoke();
        }

        internal void AssertTransactionActive() {
            if (!this.IsTransactionActive) {
                throw new InvalidOperationException("Cannot modify user data outside of transaction");
            }
        }

        internal abstract int  TransactionVersion  { get; }
        internal abstract bool IsTransactionActive { get; }

        internal abstract void Serialize(ref MessagePackWriter writer, MessagePackSerializerOptions options);
        internal abstract void Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options);

        internal abstract void RecordChangeSet(UdDataChangeSet changeSet);
        internal abstract void FlushAndClearModifications();

        protected abstract void AddChild(string key, UdObjectBase child);

        // public virtual bool ShouldBeSerialized { get; } = true;

        protected void Track() {
            this.version.Get();
        }

        protected void Invalidate() {
            this.version.Invalidate();
        }

        public override string ToString() {
            using (UdRoot.ArrayBufferWriterPool.Get(out var stream)) {
                var writer = new MessagePackWriter(stream);
                this.Serialize(ref writer, MessagePackSerializer.DefaultOptions);
                writer.Flush();

                return MessagePackSerializer.ConvertToJson(stream.WrittenMemory);
            }
        }
    }
}