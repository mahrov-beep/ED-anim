namespace Multicast.Boosts {
    using System;

    [Serializable]
    public readonly struct BoostTag : IEquatable<BoostTag> {
        public static readonly BoostTag None = new(null);

        private readonly string tag;

        public bool IsNone => this.Equals(None);

        public BoostTag(string tag) => this.tag = tag;

        public override string ToString() => this.tag ?? string.Empty;

        public bool Equals(BoostTag other) => this.tag == other.tag;

        public override bool Equals(object obj) => obj is BoostTag other && this.Equals(other);

        public override int GetHashCode() => this.tag?.GetHashCode() ?? 0;
    }
}