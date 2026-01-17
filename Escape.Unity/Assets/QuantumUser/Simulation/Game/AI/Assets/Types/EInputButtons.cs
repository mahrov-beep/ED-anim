using System;

namespace Quantum {
  [Flags]
  public enum EInputButtons {
    None    = 0,
    Ability = 1 << 0,
    All     = /*FireButton | */ /*ReloadWeapon |*/ Ability,
  }
}