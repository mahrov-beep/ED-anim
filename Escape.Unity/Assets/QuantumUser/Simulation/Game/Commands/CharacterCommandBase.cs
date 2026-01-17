namespace Quantum.Commands {
  using Photon.Deterministic;

  public abstract class CharacterCommandBase : DeterministicCommand {
    public abstract void Execute(Frame f, EntityRef characterEntity);
  }
}