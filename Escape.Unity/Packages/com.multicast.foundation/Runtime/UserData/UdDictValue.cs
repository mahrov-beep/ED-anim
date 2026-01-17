namespace Multicast.UserData {
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using JetBrains.Annotations;
    using MessagePack;
    using Sirenix.OdinInspector;
    using UniMob;

    public sealed class UdDictValue<T> : UdObjectBase, IEnumerable<KeyValuePair<string, T>> {
        private Dictionary<string, T> value;
        private bool                  isValueModified;

        public UdDictValue(UdArgs args, Dictionary<string, T> initialValue) : base(args) {
            this.value = initialValue;

            this.isValueModified = false;
        }

        public UdDictValue(UdArgs args) : this(args, new Dictionary<string, T>()) {
        }

        [PublicAPI]
        [ShowInInspector, LabelText("@MyKey"), ReadOnly, EnableGUI]
        private Dictionary<string, T> InspectorValue {
            get {
                this.Track();
                return this.value;
            }
        }

        public Dictionary<string, T> AsDictionary() {
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
        public IEnumerable<string> Keys {
            get {
                this.Track();
                return this.value.Keys;
            }
        }

        [PublicAPI]
        public IEnumerable<T> Values {
            get {
                this.Track();
                return this.value.Values;
            }
        }

        [PublicAPI]
        public T this[string key] {
            get {
                this.Track();
                return this.value[key];
            }
            set {
                this.value[key] = value;
                this.Dirty();
            }
        }

        [PublicAPI]
        public bool TryGetValue(string key, out T item) {
            this.Track();
            return this.value.TryGetValue(key, out item);
        }

        [PublicAPI]
        public bool ContainsKey(string key) {
            this.Track();
            return this.value.ContainsKey(key);
        }

        [PublicAPI]
        public bool ContainsValue(T item) {
            this.Track();
            return this.value.ContainsValue(item);
        }

        [PublicAPI]
        public void Add(string key, T item) {
            this.value.Add(key, item);
            this.Dirty();
        }

        [PublicAPI]
        public bool Remove(string key) {
            var removed = this.value.Remove(key);
            this.Dirty();
            return removed;
        }

        [PublicAPI]
        public void Clear() {
            this.value.Clear();
            this.Dirty();
        }

        [PublicAPI]
        public void Dirty() {
            using var _ = Atom.NoWatch;

            this.AssertTransactionActive();

            this.isValueModified = true;

            this.Invalidate();
        }

        public Dictionary<string, T>.Enumerator GetEnumerator() {
            this.Track();
            return this.value.GetEnumerator();
        }

        IEnumerator<KeyValuePair<string, T>> IEnumerable<KeyValuePair<string, T>>.GetEnumerator() {
            this.Track();
            return this.value.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return this.GetEnumerator();
        }

        internal override bool IsTransactionActive => this.MyParent.IsTransactionActive;

        internal override int TransactionVersion => this.MyParent.TransactionVersion;

        internal override void Serialize(ref MessagePackWriter writer, MessagePackSerializerOptions options) {
            options.Resolver.GetFormatterWithVerify<Dictionary<string, T>>().Serialize(ref writer, this.value, options);
        }

        internal override void Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options) {
            this.value = options.Resolver.GetFormatterWithVerify<Dictionary<string, T>>().Deserialize(ref reader, options);
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

        public static implicit operator UdDictValue<T>(UdArgs args) {
            return new UdDictValue<T>(args);
        }
    }
}