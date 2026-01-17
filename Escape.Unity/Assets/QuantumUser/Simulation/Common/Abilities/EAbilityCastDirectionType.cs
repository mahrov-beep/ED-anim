using System;

namespace Quantum {
  [Flags]
  public enum EAbilityCastDirectionType {
    Aim             = 1 << 0,
    Movement        = 1 << 1,
    FacingDirection = 1 << 2,
  }
}