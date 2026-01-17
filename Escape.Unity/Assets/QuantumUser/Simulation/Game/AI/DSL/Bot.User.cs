namespace Quantum {
  using System.Collections;
  using System.Collections.Generic;
  using Core;
  using Quantum;
  public unsafe partial struct Bot {
    public bool InStrafe(Frame f, EntityRef botRef) {
      //TODO InStrafe всегда false
      
      return false;
      // if (!f.TryGetPointer<AIBlackboardComponent>(botRef, out var bb)) {
      //   return false;
      // }
      //
      // if (!bb->TryGetBoolean(f, AIConstants.BB_FLAG_IS_STRAIFING, out QBoolean inStrafe)) {
      //   return false;
      // }
      //
      // return inStrafe;
    }
  }
}