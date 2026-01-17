namespace Quantum {
  using System;
  public unsafe partial struct Team : IEquatable<Team> {
    public bool Equals(Team other) {
      return Index == other.Index;
    }

    public bool Equals(Team* other) {
      return other != null && Index == other->Index;
    }

    public override bool Equals(object obj) {
      return obj is Team other && Equals(other);
    }

    public static bool operator ==(Team* ptr, Team value) {
      return ptr->Equals(value);
    }

    public static bool operator !=(Team* ptr, Team value) {
      return !ptr->Equals(value);
    }

    public static bool operator ==(Team a, Team b) {
      return a.Equals(b);
    }

    public static bool operator !=(Team a, Team b) {
      return !(a == b);
    }

    public override string ToString() {
      return $"Team {Index}";
    }
  }
}