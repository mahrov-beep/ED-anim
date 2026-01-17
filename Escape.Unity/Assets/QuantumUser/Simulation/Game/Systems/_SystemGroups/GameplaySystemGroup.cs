namespace Quantum {
public class GameplaySystemGroup : SystemGroup {

  public override bool StartEnabled { get; set; } = true;

  public GameplaySystemGroup(SystemBase[] children) : base(nameof(GameplaySystemGroup), children) { }
}
}