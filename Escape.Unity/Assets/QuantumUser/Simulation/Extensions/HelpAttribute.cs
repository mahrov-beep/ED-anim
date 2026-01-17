namespace Quantum {
  using System;
  using System.Diagnostics;

  [Conditional("UNITY_EDITOR")]
  [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
  public class HelpAttribute : Attribute {
    public string Help { get; }

    public HelpAttribute(string help) {
      Help = help;
    }
  }
}