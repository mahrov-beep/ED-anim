namespace Multicast.UserData {
    using System.Collections.Generic;
    using JetBrains.Annotations;
    using MessagePack;
    using Sirenix.OdinInspector;
    using UnityEngine.Pool;

    public abstract class UdObject : UdObjectBase, IDataObject {
        [ShowInInspector, LabelText("@MyKey"), ReadOnly, EnableGUI]
        [ListDrawerSettings(
            HideAddButton    = true,
            HideRemoveButton = true,
            DraggableItems   = false)]
        private readonly List<UdObjectBase> children = new List<UdObjectBase>();

        protected UdObject(UdArgs args) : base(args) {
        }

        string IDataObject.MyKey => this.MyKey;
        
        internal sealed override bool IsTransactionActive => this.MyParent.IsTransactionActive;

        internal sealed override int TransactionVersion => this.MyParent.TransactionVersion;

        internal sealed override void Serialize(ref MessagePackWriter writer, MessagePackSerializerOptions options) {
            using (ListPool<UdObjectBase>.Get(out var childrenToSerialize)) {
                foreach (var child in this.children) {
                    // if (child.ShouldBeSerialized) {
                    //     childrenToSerialize.Add(child);
                    // }
                    childrenToSerialize.Add(child);
                }

                writer.WriteMapHeader(childrenToSerialize.Count);

                foreach (var child in childrenToSerialize) {
                    writer.Write(child.MyKey);
                    child.Serialize(ref writer, options);
                }
            }
        }

        internal sealed override void Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options) {
            options.Security.DepthStep(ref reader);

            var count = reader.ReadMapHeader();

            for (var i = 0; i < count; i++) {
                var key = reader.ReadString();

                if (this.TryGetChild(key, out var child)) {
                    child.Deserialize(ref reader, options);
                }
                else {
                    reader.Skip();
                }
            }

            reader.Depth--;
        }

        internal override void RecordChangeSet(UdDataChangeSet changeSet) {
            foreach (var child in this.children) {
                child.RecordChangeSet(changeSet);
            }
        }

        internal sealed override void FlushAndClearModifications() {
            foreach (var child in this.children) {
                child.FlushAndClearModifications();
            }
        }

        protected override void AddChild(string key, UdObjectBase child) {
            foreach (var it in this.children) {
                if (it.MyKey == child.MyKey) {
                    throw new UdDuplicatedPropertyKeyException($"Duplicated key '{key}' in {this.GetType()}");
                }
            }

            this.children.Add(child);
        }

        private bool TryGetChild(string key, out UdObjectBase child) {
            foreach (var it in this.children) {
                if (it.MyKey == key) {
                    child = it;
                    return true;
                }
            }

            child = null;
            return false;
        }

        [PublicAPI]
        protected UdArgs Child(string key) {
            return new UdArgs(this, key);
        }
    }
}