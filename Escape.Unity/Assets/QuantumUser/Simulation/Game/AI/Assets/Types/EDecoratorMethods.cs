using System;

namespace Quantum {
  [Flags]
  public enum EDecoratorMethods {
    None           = 0,
    OnEnter        = 1 << 0,
    OnExit         = 1 << 1,
    OnAbort        = 1 << 2,
    CheckCondition = 1 << 3,
    All            = OnEnter | OnExit | OnAbort | CheckCondition,
  }
}