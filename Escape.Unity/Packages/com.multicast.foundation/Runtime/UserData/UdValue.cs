namespace Multicast.UserData {
    using System;
    using System.Collections.Generic;
    using JetBrains.Annotations;
    using MessagePack;
    using Sirenix.OdinInspector;
    using UniMob;
    using UnityEngine;

    public sealed class UdValue<T> : UdObjectBase {
        public static Func<T>                  DefaultValue   { get; set; } = () => default;
        public static Func<UdValue<T>, string> CustomToString { get; set; } = null;

        private T    value;
        private bool isValueModified;

        public UdValue(UdArgs args, T initialValue) : base(args) {
            this.value = initialValue;

            this.isValueModified = false;

#if UNITY_EDITOR
            var valueType = typeof(T);

            if (valueType.IsGenericType && valueType.GetGenericTypeDefinition() == typeof(List<>)) {
                Debug.LogError("Use UdListValue<> instead of UdValue<List<>>");
            }

            if (valueType.IsGenericType && valueType.GetGenericTypeDefinition() == typeof(Dictionary<,>)) {
                Debug.LogError("Use UdDictValue<,> instead of UdValue<Dictionary<,>>");
            }

            if (!valueType.IsValueType && valueType != typeof(string)) {
                Debug.LogError($"Non ValueType ({valueType.Name}) usage in UdValue<> may de dangerous");
            }
#endif
        }

        public UdValue(UdArgs args) : this(args, DefaultValue.Invoke()) {
        }

        [PublicAPI]
        [ShowInInspector, LabelText("@MyKey"), ReadOnly, EnableGUI]
        public T Value {
            get {
                this.Track();

                return this.value;
            }
            set {
                using var _ = Atom.NoWatch;

                this.AssertTransactionActive();

                this.value           = value;
                this.isValueModified = true;

                this.Invalidate();
            }
        }

        [PublicAPI]
        public void Dirty() {
            using var _ = Atom.NoWatch;

            this.AssertTransactionActive();

            this.isValueModified = true;

            this.Invalidate();
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

        public override string ToString() {
            if (CustomToString != null) {
                return CustomToString.Invoke(this);
            }

            return base.ToString();
        }

        public static implicit operator UdValue<T>(UdArgs args) {
            return new UdValue<T>(args);
        }
    }
}