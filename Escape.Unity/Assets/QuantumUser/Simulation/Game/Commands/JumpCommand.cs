namespace Quantum.Commands {
  using Photon.Deterministic;

  public sealed class JumpCommand : CharacterCommandBase {
    public override void Serialize(BitStream stream) {
    }

    public override unsafe void Execute(Frame f, EntityRef characterEntity) {
      if (CharacterFsm.CurrentStateIs<CharacterStateCrouchIdle>(f, characterEntity) ||
          CharacterFsm.CurrentStateIs<CharacterStateCrouchMove>(f, characterEntity)) {
        CharacterFsm.TryEnterState(f, characterEntity, new CharacterStateIdle());
      }
      else {
        CharacterFsm.TryEnterState(f, characterEntity, new CharacterStateJump());
      }
    }
  }
}