namespace Multicast.Numerics {
    using System;

    [Serializable]
    public struct ProtectedInt : IEquatable<ProtectedInt> {
        public int v;
        public int c;
        public int s;

        public bool IsValid => ProtectedIntAccess.IsValid(ref this.v, ref this.c, ref this.s);

        public int Value {
            get => ProtectedIntAccess.GetValue(ref this.v, ref this.c, ref this.s, true);
            set => ProtectedIntAccess.SetValue(ref this.v, ref this.c, ref this.s, value, false);
        }

        public ProtectedInt(int initialValue, bool randomize = true) {
            this.v = this.c = this.s = 0;
            ProtectedIntAccess.SetValue(ref this.v, ref this.c, ref this.s, initialValue, true, randomize);
        }

        public int GetValue(bool throws = true) {
            return ProtectedIntAccess.GetValue(ref this.v, ref this.c, ref this.s, throws);
        }

        public bool Equals(ProtectedInt other) {
            return this.IsValid && other.IsValid && this.GetValue(false) == other.GetValue(false);
        }

        public override bool Equals(object obj) => obj is ProtectedInt other && this.Equals(other);

        public override int GetHashCode() => this.GetValue(false).GetHashCode();

        public override string ToString() => this.GetValue(false).ToString();

        public static implicit operator ProtectedInt(int value) => new ProtectedInt(value);
        public static implicit operator int(ProtectedInt value) => value.Value;

        public static bool operator ==(ProtectedInt a, ProtectedInt b) => a.IsValid && b.IsValid && a.GetValue(false) == b.GetValue(false);
        public static bool operator !=(ProtectedInt a, ProtectedInt b) => a.IsValid && b.IsValid && a.GetValue(false) != b.GetValue(false);

        public static bool operator >(ProtectedInt a, ProtectedInt b) => a.GetValue(false) > b.GetValue(false);
        public static bool operator <(ProtectedInt a, ProtectedInt b) => a.GetValue(false) < b.GetValue(false);

        public static bool operator >=(ProtectedInt a, ProtectedInt b) => a.GetValue(false) >= b.GetValue(false);
        public static bool operator <=(ProtectedInt a, ProtectedInt b) => a.GetValue(false) <= b.GetValue(false);

        public static ProtectedInt operator +(ProtectedInt a, ProtectedInt b) => (ProtectedInt) (a.GetValue(false) + b.GetValue(false));
        public static ProtectedInt operator -(ProtectedInt a, ProtectedInt b) => (ProtectedInt) (a.GetValue(false) - b.GetValue(false));
        public static ProtectedInt operator *(ProtectedInt a, ProtectedInt b) => (ProtectedInt) (a.GetValue(false) * b.GetValue(false));
        public static ProtectedInt operator /(ProtectedInt a, ProtectedInt b) => (ProtectedInt) (a.GetValue(false) / b.GetValue(false));
    }
}