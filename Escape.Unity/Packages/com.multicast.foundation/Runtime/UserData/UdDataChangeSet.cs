namespace Multicast.UserData {
    using System.Buffers;
    using System.Collections.Generic;
    using System.Text;
    using JetBrains.Annotations;
    using MessagePack;

    public abstract class UdDataChangeSet {
        public static readonly UdDataChangeSet None = new UdNilDataChangeSet();

        public abstract void Update(UdObjectBase obj);
        public abstract void Delete(UdObjectBase obj);
    }

    internal sealed class UdNilDataChangeSet : UdDataChangeSet {
        public override void Update(UdObjectBase obj) {
        }

        public override void Delete(UdObjectBase obj) {
        }
    }

    public class UdDebugStringDataChangeSet : UdDataChangeSet {
        private readonly StringBuilder sb = new StringBuilder();

        private readonly List<string> tmpPath = new List<string>();

        public void Clear() {
            this.sb.Clear();
        }

        public string Stringify() {
            return this.sb.ToString();
        }

        public override void Update(UdObjectBase obj) {
            BuildPath(this.tmpPath, obj);

            this.sb.Append(" + ");

            for (var i = this.tmpPath.Count - 1; i >= 0; i--) {
                var s = this.tmpPath[i];
                this.sb.Append(s).Append('/');
            }

            var objStr = obj.ToString();
            this.sb.Append(" = ").Append(objStr);

            this.sb.AppendLine();

            this.tmpPath.Clear();
        }

        public override void Delete(UdObjectBase obj) {
            BuildPath(this.tmpPath, obj);

            this.sb.Append(" - ");

            for (var i = this.tmpPath.Count - 1; i >= 0; i--) {
                var s = this.tmpPath[i];
                this.sb.Append(s).Append('/');
            }

            this.sb.AppendLine();

            this.tmpPath.Clear();
        }

        private static void BuildPath(List<string> path, UdObjectBase self) {
            path.Clear();

            while (self != null) {
                path.Add(self.MyKey);

                self = self.MyParent;
            }

            // delete roots
            path.RemoveAt(path.Count - 1);
            path.RemoveAt(path.Count - 1);
        }
    }

    public class UdMessagePackDataChangeSet : UdDataChangeSet {
        private readonly ArrayBufferWriter<byte>      stream;
        private readonly MessagePackSerializerOptions options;

        private readonly List<string> tmpPath = new List<string>();

        private int entries;

        public UdMessagePackDataChangeSet(MessagePackSerializerOptions options) {
            this.stream  = new ArrayBufferWriter<byte>();
            this.options = options;
            this.entries = 0;
        }

        [PublicAPI]
        public void WriteTo(IBufferWriter<byte> output) {
            var writer = new MessagePackWriter(output);
            writer.WriteArrayHeader(this.entries);
            writer.WriteRaw(this.stream.WrittenSpan);
            writer.Flush();
        }

        public override void Update(UdObjectBase obj) {
            BuildPath(this.tmpPath, obj);

            var writer = new MessagePackWriter(this.stream);
            writer.WriteArrayHeader(2 + this.tmpPath.Count);
            writer.Write("add");

            for (var i = this.tmpPath.Count - 1; i >= 0; i--) {
                var p = this.tmpPath[i];
                writer.Write(p);
            }

            obj.Serialize(ref writer, this.options);
            writer.Flush();

            this.entries++;
            this.tmpPath.Clear();
        }

        public override void Delete(UdObjectBase obj) {
            BuildPath(this.tmpPath, obj);

            var writer = new MessagePackWriter(this.stream);
            writer.WriteArrayHeader(2 + this.tmpPath.Count);
            writer.Write("del");

            for (var i = this.tmpPath.Count - 1; i >= 0; i--) {
                var p = this.tmpPath[i];
                writer.Write(p);
            }

            writer.WriteNil();
            writer.Flush();

            this.entries++;
            this.tmpPath.Clear();
        }

        private static void BuildPath(List<string> path, UdObjectBase self) {
            path.Clear();

            while (self != null) {
                path.Add(self.MyKey);

                self = self.MyParent;
            }

            // delete roots
            path.RemoveAt(path.Count - 1);
            path.RemoveAt(path.Count - 1);
        }
    }
}