namespace Quantum {
public class CharacterControlSystemGroup : SystemGroup {
  public override bool StartEnabled { get; set; } = true;
  public CharacterControlSystemGroup(SystemBase[] children) : base(nameof(CharacterControlSystemGroup), children) { }
}
}