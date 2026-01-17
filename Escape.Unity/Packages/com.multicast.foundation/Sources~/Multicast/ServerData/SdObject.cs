using MessagePack;
using System;
using System.Collections.Generic;

namespace Multicast.ServerData {
    public abstract class SdObject : SdObjectBase, IDataObject {
        private readonly List<SdObjectBase> children;

        string IDataObject.MyKey => this.GetSdObjectKey();

        protected SdObject(SdArgs args) : base(args) {
            this.children = new List<SdObjectBase>();
        }

        protected SdArgs Child(SdKey key) {
            return this.ChildInternal(key);
        }

        internal sealed override void Serialize(ref MessagePackWriter writer, MessagePackSerializerOptions options) {
            var keyFormatter = options.Resolver.GetFormatterWithVerify<SdKey>();

            writer.WriteMapHeader(this.children.Count);

            foreach (var child in this.children) {
                keyFormatter.Serialize(ref writer, child.MyKey, options);
                child.Serialize(ref writer, options);
            }
        }

        internal sealed override void Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options) {
            options.Security.DepthStep(ref reader);

            var keyFormatter = options.Resolver.GetFormatterWithVerify<SdKey>();

            var count = reader.ReadMapHeader();

            for (var i = 0; i < count; i++) {
                var key = keyFormatter.Deserialize(ref reader, options);

                if (this.TryGetChild(key, out var child)) {
                    child.Deserialize(ref reader, options);
                }
                else {
                    reader.Skip();
                }
            }

            reader.Depth--;
        }

        protected override void AddChild(SdKey key, SdObjectBase child) {
            if (child is null) {
                throw new ArgumentNullException(nameof(child));
            }

            foreach (var it in this.children) {
                if (it.MyKey == child.MyKey) {
                    throw new SdDuplicatedPropertyKeyException($"Duplicated key '{key}' in {this.GetType()}");
                }
            }

            this.children.Add(child);
        }

        private bool TryGetChild(SdKey key, out SdObjectBase child) {
            foreach (var it in this.children) {
                if (it.MyKey == key) {
                    child = it;
                    return true;
                }
            }

            child = null;
            return false;
        }
    }
}