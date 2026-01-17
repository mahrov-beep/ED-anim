using MessagePack;
using System;
using System.Collections.Generic;

namespace Multicast.ServerData {
    public abstract class SdArrayObject : SdObjectBase, IDataObject {
        private readonly List<SdObjectBase> children;

        string IDataObject.MyKey => this.GetSdObjectKey();

        protected SdArrayObject(SdArgs args) : base(args) {
            this.children = new List<SdObjectBase>();
        }

        protected SdArgs Child(uint key) {
            return this.ChildInternal(key);
        }

        internal sealed override void Serialize(ref MessagePackWriter writer, MessagePackSerializerOptions options) {
            writer.WriteArrayHeader(this.children.Count);

            foreach (var child in this.children) {
                child.Serialize(ref writer, options);
            }
        }

        internal sealed override void Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options) {
            options.Security.DepthStep(ref reader);

            var count = reader.ReadArrayHeader();

            for (var i = 0; i < count; i++) {
                if (i < this.children.Count) {
                    this.children[i].Deserialize(ref reader, options);
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

            if (!key.TryGetIntKey(out var intKey)) {
                throw new ArgumentException("Only SdKey(int) allowed for SdObject");
            }

            if (intKey != this.children.Count) {
                throw new ArgumentException("Child must be added in order of SdKey int values");
            }

            this.children.Add(child);
        }
    }
}