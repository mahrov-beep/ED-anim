// ReSharper disable InconsistentNaming

namespace Quantum {
  using System;
  using System.Runtime.InteropServices;
  using Core;

  [Serializable, StructLayout(LayoutKind.Explicit)]
  public unsafe struct QGuid : IQStringUtf8, IEquatable<QGuid> {
    public const int SIZE      = 40;
    public const int ALIGNMENT = 4;

    [FieldOffset(38)]
    fixed Byte _alignment_padding_[2];
    [FieldOffset(0)]
    public UInt16 ByteCount;

    [FieldOffset(2), FixedBufferDynamicLength("ByteCount")]
    public fixed Byte Bytes[36];
    public const int MaxByteCount = 36;

    public QGuid(string str) {
      QStringUtf8.ConstructFrom(str, MaxByteCount, out this);
    }

    public int Length => QStringUtf8.GetLength(this);

    public override string ToString() => QStringUtf8.GetString(this);

    public static bool CanHold(string str) => QStringUtf8.CanHold(str, MaxByteCount);

    int IQStringUtf8.CompareOrdinal(byte* bytes, ushort byteCount) => QStringUtf8.CompareOrdinal(this, bytes, byteCount);

    public int CompareOrdinal(string str) => QStringUtf8.CompareOrdinal(this, str);

    public static implicit operator QGuid(string str) => new(str);

    public static implicit operator string(QGuid str) => str.ToString();

    public override bool Equals(object obj) => QStringUtf8.AreEqual(this, obj);

    public bool Equals(QGuid str) => QStringUtf8.CompareOrdinal(this, str.Bytes, str.ByteCount) == 0;

    public bool Equals<T>(ref T str) where T : unmanaged, IQStringUtf8 {
      return QStringUtf8.CompareOrdinal(this, str) == 0;
    }

    public int CompareOrdinal<T>(ref T str) where T : unmanaged, IQStringUtf8 {
      return QStringUtf8.CompareOrdinal(this, str);
    }

    public override int GetHashCode() {
      unchecked {
        var hash = 19801;
        hash = hash * 31 + ByteCount.GetHashCode();
        fixed (byte* p = Bytes) {
          hash = hash * 31 + HashCodeUtils.GetArrayHashCode(p, this.ByteCount);
        }

        return hash;
      }
    }

    public static void Serialize(void* ptr, FrameSerializer serializer) {
      var p = (QGuid*)ptr;
      serializer.Stream.Serialize(&p->ByteCount);
      Assert.Always(p->ByteCount <= 36, p->ByteCount);
      serializer.Stream.SerializeBuffer(&p->Bytes[0], p->ByteCount);
    }
  }
}