using System;

namespace Quantum {
  [Flags]
  public enum EInputType {
    None              = 0,
    MovementDirection = 1 << 0,
    ActionDirection   = 1 << 1,
    RotationDelta     = 1 << 2,
  }
}