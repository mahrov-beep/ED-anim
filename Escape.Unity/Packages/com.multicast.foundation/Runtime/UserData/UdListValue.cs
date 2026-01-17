namespace Multicast.UserData {
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using JetBrains.Annotations;
    using MessagePack;
    using Sirenix.OdinInspector;
    using UniMob;

    public sealed class UdListValue<T> : UdObjectBase, IEnumerable<T> {
        private List<T> value;
        private bool    isValueModified;

        public UdListValue(UdArgs args, List<T> initialValue) : base(args) {
            this.value = initialValue;

            this.isValueModified = false;
        }

        public UdListValue(UdArgs args) : this(args, new List<T>()) {
        }

        [PublicAPI]
        [ShowInInspector, LabelText("@MyKey"), ReadOnly, EnableGUI]
        private List<T> InspectorValue {
            get {
                this.Track();
                return this.value;
            }
        }

        [PublicAPI]
        public List<T> AsList() {
            this.Track();
            return this.value;
        }

        [PublicAPI]
        public int Count {
            get {
                this.Track();
                return this.value.Count;
            }
        }

        [PublicAPI]
        public T this[int index] {
            get {
                this.Track();
                return this.value[index];
            }
            set {
                this.value[index] = value;
                this.Dirty();
            }
        }

        [PublicAPI]
        public int IndexOf(T item) {
            this.Track();
            return this.value.IndexOf(item);
        }

        [PublicAPI]
        public int IndexOf(T item, int index) {
            this.Track();
            return this.value.IndexOf(item, index);
        }

        [PublicAPI]
        public void Add(T item) {
            this.value.Add(item);
            this.Dirty();
        }

        [PublicAPI]
        public void AddRange(IEnumerable<T> item) {
            this.value.AddRange(item);
            this.Dirty();
        }

        [PublicAPI]
        public void Insert(int index, T item) {
            this.value.Insert(index, item);
            this.Dirty();
        }

        [PublicAPI]
        public bool Remove(T item) {
            var removed = this.value.Remove(item);
            this.Dirty();
            return removed;
        }

        [PublicAPI]
        public void RemoveAt(int index) {
            this.value.RemoveAt(index);
            this.Dirty();
        }

        [PublicAPI]
        public void Clear() {
            this.value.Clear();
            this.Dirty();
        }

        [PublicAPI]
        public bool Contains(T item) {
            this.Track();
            return this.value.Contains(item);
        }

        [PublicAPI]
        public void Dirty() {
            using var _ = Atom.NoWatch;

            this.AssertTransactionActive();

            this.isValueModified = true;

            this.Invalidate();
        }

        public IEnumerator<T> GetEnumerator() {
            this.Track();
            return this.value.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return this.GetEnumerator();
        }

        internal override bool IsTransactionActive => this.MyParent.IsTransactionActive;

        internal override int TransactionVersion => this.MyParent.TransactionVersion;

        internal override void Serialize(ref MessagePackWriter writer, MessagePackSerializerOptions options) {
            options.Resolver.GetFormatterWithVerify<List<T>>().Serialize(ref writer, this.value, options);
        }

        internal override void Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options) {
            this.value = options.Resolver.GetFormatterWithVerify<List<T>>().Deserialize(ref reader, options);
        }

        protected override void AddChild(string key, UdObjectBase obj) {
            throw new InvalidOperationException("UdValue cannot contains children");
        }

        internal override void RecordChangeSet(UdDataChangeSet changeSet) {
            if (this.isValueModified) {
                changeSet.Update(this);
            }
        }

        internal override void FlushAndClearModifications() {
            if (!this.isValueModified) {
                return;
            }

            this.isValueModified = false;

            this.NotifySelfChanged();
        }

        public static implicit operator UdListValue<T>(UdArgs args) {
            return new UdListValue<T>(args);
        }
    }
}