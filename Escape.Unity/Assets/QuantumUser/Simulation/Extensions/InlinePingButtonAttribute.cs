namespace Quantum {
  using System;
  using System.Diagnostics;

  [Conditional("UNITY_EDITOR")]
  [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
  public class InlinePingButtonAttribute : Attribute {
    
  }
}