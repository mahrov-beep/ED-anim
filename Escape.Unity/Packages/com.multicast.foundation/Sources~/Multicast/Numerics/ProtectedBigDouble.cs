namespace Multicast.Numerics {
    using System;

    [Serializable]
    public struct ProtectedBigDouble : IEquatable<ProtectedBigDouble> {
        public int  a;
        public long d;
        public int  b;
        public int  c;

        public BigDouble Value {
            get {
                ProtectedBigDoublePacker.Unpack(this.a, this.b, this.c, out var n);
                return new BigDouble(n, this.d ^ this.c);
            }
        }

        public ProtectedBigDouble(BigDouble value, bool randomize = true) {
            ProtectedBigDoublePacker.Pack(value.numerator, randomize, out this.a, out this.b, out this.c);
            this.d = value.exponent ^ this.c;
        }

        public bool Equals(ProtectedBigDouble other) => this.Value == other.Value;

        public override bool Equals(object obj) => obj is ProtectedBigDouble other && this.Equals(other);

        public override int GetHashCode() => this.Value.GetHashCode();

        public override string ToString() => this.Value.ToString();

        public static implicit operator ProtectedBigDouble(float value) {
            return new ProtectedBigDouble(value);
        }

        public static implicit operator ProtectedBigDouble(BigDouble value) {
            return new ProtectedBigDouble(value);
        }

        public static implicit operator BigDouble(ProtectedBigDouble value) {
            return value.Value;
        }

        public static bool operator ==(ProtectedBigDouble a, BigDouble b) => a.Value == b;
        public static bool operator !=(ProtectedBigDouble a, BigDouble b) => a.Value != b;
        public static bool operator >(ProtectedBigDouble a, BigDouble b)  => a.Value > b;
        public static bool operator >=(ProtectedBigDouble a, BigDouble b) => a.Value >= b;
        public static bool operator <(ProtectedBigDouble a, BigDouble b)  => a.Value < b;
        public static bool operator <=(ProtectedBigDouble a, BigDouble b) => a.Value <= b;

        public static BigDouble operator +(ProtectedBigDouble a, BigDouble b) => a.Value + b;
        public static BigDouble operator -(ProtectedBigDouble a, BigDouble b) => a.Value - b;
        public static BigDouble operator *(ProtectedBigDouble a, BigDouble b) => a.Value * b;
        public static BigDouble operator /(ProtectedBigDouble a, BigDouble b) => a.Value / b;
    }
}