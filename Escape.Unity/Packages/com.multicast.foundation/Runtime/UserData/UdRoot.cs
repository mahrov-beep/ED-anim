namespace Multicast.UserData {
    using System;
    using System.Buffers;
    using System.Security.Cryptography;
    using JetBrains.Annotations;
    using MessagePack;
    using Modules.UserData;
    using Sirenix.OdinInspector;
    using UniMob;
    using UnityEngine;
    using UnityEngine.Pool;
    using Random = UnityEngine.Random;

    public abstract class UdRoot : UdObjectBase {
        internal const string HASH_SALT = "5b5496vn7456oodw976d95mo";

        public static readonly ObjectPool<ArrayBufferWriter<byte>> ArrayBufferWriterPool
            = new ObjectPool<ArrayBufferWriter<byte>>(() => new ArrayBufferWriter<byte>(), actionOnRelease: it => it.Clear());

        public static readonly ObjectPool<byte[]> Byte16ArrayPool
            = new ObjectPool<byte[]>(() => new byte[16], actionOnRelease: it => Array.Clear(it, 0, it.Length));

        private static readonly MessagePackSerializerOptions Options = MessagePackSerializer.DefaultOptions
            .WithCompression(MessagePackCompression.Lz4BlockArray);

        [PublicAPI]
        public static UdRoot<T> Create<T>(Func<UdArgs, T> factory)
            where T : UdObjectBase {
            using var _ = Atom.NoWatch;

            return new UdRoot<T>(factory);
        }

        [PublicAPI]
        public static UdRoot<T> FromMemory<T>(Func<UdArgs, T> factory, ReadOnlyMemory<byte> input)
            where T : UdObjectBase {
            using var _ = Atom.NoWatch;

            return FromMemory(factory, input, Options);
        }

        [PublicAPI]
        public static UdRoot<T> FromMemory<T>(Func<UdArgs, T> factory, ReadOnlyMemory<byte> input, MessagePackSerializerOptions options)
            where T : UdObjectBase {
            using var _ = Atom.NoWatch;

            var root   = new UdRoot<T>(factory);
            var reader = new MessagePackReader(input);
            root.Deserialize(ref reader, options);
            return root;
        }

        [PublicAPI]
        public static void Serialize(UdRoot root, IBufferWriter<byte> output)  {
            using var _ = Atom.NoWatch;

            Serialize(root, output, Options);
        }

        [PublicAPI]
        public static void Serialize(UdObjectBase root, IBufferWriter<byte> output, MessagePackSerializerOptions options) {
            using var _ = Atom.NoWatch;

            var writer = new MessagePackWriter(output);
            root.Serialize(ref writer, options);
            writer.Flush();
        }

        [PublicAPI]
        public static string SerializeToJson(UdObjectBase root) {
            using var _ = Atom.NoWatch;

            return SerializeToJson(root, Options);
        }

        [PublicAPI]
        public static string SerializeToJson(UdObjectBase root, MessagePackSerializerOptions options) {
            using var _ = Atom.NoWatch;

            var stream = new ArrayBufferWriter<byte>();
            Serialize(root, stream, options);
            return MessagePackSerializer.ConvertToJson(stream.WrittenMemory);
        }

        protected UdRoot(UdArgs args) : base(args) {
        }

        public abstract bool TryGetActiveTransaction(out string transactionId);

        public abstract int  BeginTransaction(string transactionId);
        public abstract void CommitTransaction(UdDataChangeSet changeSet = null);

        [MustDisposeResource]
        public UdTransactionScope BeginTransactionScope(string transactionId) {
            return new UdTransactionScope(this, transactionId);
        }
    }

    public readonly struct UdTransactionScope : IDisposable {
        private readonly UdRoot root;

        public UdTransactionScope(UdRoot root, string transactionId) {
            this.root = root;
            this.root.BeginTransaction(transactionId);
        }

        public void Dispose() {
            this.root.CommitTransaction();
        }
    }

    public sealed class UdRoot<T> : UdRoot
        where T : UdObjectBase {
        private int  transactionVersion;
        private string activeTransactionId;

        [PublicAPI]
        [ShowInInspector, InlineProperty, HideLabel, ReadOnly, EnableGUI]
        public T Value { get; }

        internal UdRoot(Func<UdArgs, T> factory)
            : base(new UdArgs(null, null)) {
            this.Value = factory.Invoke(new UdArgs(this, null));
        }

        [PublicAPI]
        public override int BeginTransaction(string transactionId) {
            if (this.activeTransactionId != null) {
                throw new InvalidOperationException($"Transaction already active: {this.activeTransactionId}");
            }

            this.activeTransactionId = transactionId;
            this.transactionVersion++;

            return this.transactionVersion;
        }

        [PublicAPI]
        public override void CommitTransaction(UdDataChangeSet changeSet = null) {
            if (this.activeTransactionId == null) {
                throw new InvalidOperationException("Transaction not active");
            }

            using var _ = Atom.NoWatch;

            this.activeTransactionId = null;

            if (changeSet != null) {
                this.RecordChangeSet(changeSet);
            }

            this.FlushAndClearModifications();
        }

        [PublicAPI]
        public override bool TryGetActiveTransaction(out string transactionId) {
            transactionId = this.activeTransactionId;
            return transactionId != null;
        }

        internal override int TransactionVersion => this.transactionVersion;

        internal override bool IsTransactionActive => this.activeTransactionId != null;

        internal override void Serialize(ref MessagePackWriter writer, MessagePackSerializerOptions options) {
            var salt = Random.Range(int.MinValue, int.MaxValue);

            using (UdRoot.ArrayBufferWriterPool.Get(out var dataStream))
            using (var md5 = MD5.Create())
            using (UdRoot.Byte16ArrayPool.Get(out var hashBytes)) {
                writer.WriteMapHeader(5);

                writer.Write("version");
                writer.WriteInt32(2);

                writer.Write("salt");
                writer.WriteInt64(salt);

                writer.Write("time");
                writer.Write(DateTime.UtcNow);

                writer.Write("data");

                var dataWriter = new MessagePackWriter(dataStream);
                this.Value.Serialize(ref dataWriter, options);
                dataWriter.Flush();

                writer.WriteRaw(dataStream.WrittenSpan);

                dataWriter.Write(UdRoot.HASH_SALT);
                dataWriter.Write(salt);
                dataWriter.Flush();

                if (!md5.TryComputeHash(dataStream.WrittenSpan, hashBytes, out _)) {
                    Debug.LogError("UdRoot - Failed to calculate MD5");
                }

                writer.Write("hash");
                writer.Write(BitConverter.ToString(hashBytes));
            }
        }

        internal override void Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options) {
            if (reader.TryReadNil()) {
                return;
            }

            options.Security.DepthStep(ref reader);

            using (UdRoot.ArrayBufferWriterPool.Get(out var hashStream))
            using (var md5 = MD5.Create())
            using (UdRoot.Byte16ArrayPool.Get(out var hashBytes)) {
                var count = reader.ReadMapHeader();

                int    salt       = 0;
                string actualHash = null;
                string realHash   = null;

                for (int i = 0; i < count; i++) {
                    switch (reader.ReadString()) {
                        case "hash":
                            actualHash = reader.ReadString();
                            break;

                        case "salt":
                            salt = reader.ReadInt32();
                            break;

                        case "data":
                            if (!reader.TryReadNil()) {
                                var dataSequence = reader.ReadRaw();

                                var dataReader = new MessagePackReader(dataSequence);
                                this.Value.Deserialize(ref dataReader, options);

                                var hashWriter = new MessagePackWriter(hashStream);
                                hashWriter.WriteRaw(dataSequence);
                                hashWriter.Write(UdRoot.HASH_SALT);
                                hashWriter.Write(salt);
                                hashWriter.Flush();

                                if (!md5.TryComputeHash(hashStream.WrittenSpan, hashBytes, out _)) {
                                    Debug.LogError("UdRoot - Failed to calculate MD5");
                                }

                                realHash = BitConverter.ToString(hashBytes);
                            }

                            break;

                        default:
                            reader.Skip();
                            break;
                    }
                }

                if (actualHash == null) {
                    Debug.LogError("UdRoot - No hash in input stream");
                }
                else if (actualHash != realHash) {
                    Debug.LogError("UdRoot - Hash mismatch");
                }
            }

            reader.Depth--;
        }

        internal override void RecordChangeSet(UdDataChangeSet changeSet) {
            this.Value.RecordChangeSet(changeSet);
        }

        internal override void FlushAndClearModifications() {
            this.Value.FlushAndClearModifications();
        }

        protected override void AddChild(string key, UdObjectBase child) {
        }
    }
}