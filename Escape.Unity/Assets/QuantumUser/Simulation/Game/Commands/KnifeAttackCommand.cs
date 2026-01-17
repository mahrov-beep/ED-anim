namespace Quantum.Commands {
  using Photon.Deterministic;

  public sealed class KnifeAttackCommand : CharacterCommandBase {
    public override void Serialize(BitStream stream) { }

    public override unsafe void Execute(Frame f, EntityRef characterEntity) {
      if (!f.TryGetPointer(characterEntity, out Unit* unit)) {
        return;
      }

      if (CharacterFsm.CurrentStateIs<CharacterStateDead>(f, characterEntity) ||
          CharacterFsm.CurrentStateIs<CharacterStateKnocked>(f, characterEntity) ||
          CharacterFsm.CurrentStateIs<CharacterStateHealing>(f, characterEntity) ||
          CharacterFsm.CurrentStateIs<CharacterStateReviving>(f, characterEntity) ||
          CharacterFsm.CurrentStateIs<CharacterStateRoll>(f, characterEntity) ||
          CharacterFsm.CurrentStateIs<CharacterStateKnifeAttack>(f, characterEntity)) {
        return;
      }

      var knifeSettings = KnifeAttackHelper.ResolveSettings(f, unit);
      var duration      = knifeSettings.Duration > FP._0 ? knifeSettings.Duration : KnifeSettings.Default.Duration;

      if (duration <= FP._0) {
        KnifeAttackHelper.ExecuteAttack(f, characterEntity, knifeSettings);
        return;
      }

      var knifeState = new CharacterStateKnifeAttack {
        StateTimer = FrameTimer.FromSeconds(f, duration),
      };

      CharacterFsm.TryEnterState(f, characterEntity, knifeState);
    }
  }
}
