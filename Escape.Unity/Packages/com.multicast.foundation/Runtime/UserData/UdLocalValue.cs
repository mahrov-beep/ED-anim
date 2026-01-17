namespace Multicast.UserData {
    using System;
    using System.Collections.Generic;
    using JetBrains.Annotations;
    using MessagePack;
    using Sirenix.OdinInspector;
    using UniMob;

    public sealed class UdLocalValue<T> : UdObjectBase {
        private readonly EqualityComparer<T> comparer;

        private T value;

        public UdLocalValue(UdArgs args, T initialValue = default) : base(args) {
            this.value    = initialValue;
            this.comparer = EqualityComparer<T>.Default;
        }

        [PublicAPI]
        [ShowInInspector, LabelText("@MyKey"), ReadOnly, EnableGUI]
        public T Value {
            get {
                this.Track();
                return this.value;
            }
            set {
                if (this.comparer.Equals(this.value, value)) {
                    return;
                }

                this.value = value;
                this.Invalidate();
            }
        }

        internal override bool IsTransactionActive => this.MyParent.IsTransactionActive;

        internal override int TransactionVersion => this.MyParent.TransactionVersion;

        internal override void Serialize(ref MessagePackWriter writer, MessagePackSerializerOptions options) {
            options.Resolver.GetFormatterWithVerify<T>().Serialize(ref writer, this.value, options);
        }

        internal override void Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options) {
            this.value = options.Resolver.GetFormatterWithVerify<T>().Deserialize(ref reader, options);
        }

        protected override void AddChild(string key, UdObjectBase obj) {
            throw new InvalidOperationException("UdValue cannot contains children");
        }

        internal override void RecordChangeSet(UdDataChangeSet changeSet) {
        }

        internal override void FlushAndClearModifications() {
        }

        public static implicit operator UdLocalValue<T>(UdArgs args) {
            return new UdLocalValue<T>(args);
        }
    }
}