namespace Multicast.Misc.Tutorial {
    using System;

    [Serializable]
    public struct TutorialObjectID : IEquatable<TutorialObjectID> {
        public string primary;
        public string secondary;

        public TutorialObjectID(string primary, string secondary = "") {
            this.primary   = primary;
            this.secondary = secondary;
        }

        public bool Equals(TutorialObjectID other) {
            return this.primary == other.primary && this.secondary == other.secondary;
        }

        public override bool Equals(object obj) {
            return obj is TutorialObjectID other && this.Equals(other);
        }

        public override int GetHashCode() {
            return HashCode.Combine(this.primary, this.secondary);
        }
    }
}