namespace Quantum {
public class AISystemGroup : SystemGroup {
  public AISystemGroup(params SystemBase[] children) : base(nameof(AISystemGroup), children) { }
}
}