namespace Quantum.Commands {
  using Photon.Deterministic;

  public sealed class ReviveCommand : CharacterCommandBase {
    public override void Serialize(BitStream stream) { }

    public override unsafe void Execute(Frame f, EntityRef characterEntity) {
      if (!f.TryGetPointers(characterEntity, out Unit* unit, out Health* health, out Team* team, out Transform3D* transform)) {
        return;
      }

      if (health->CurrentValue <= FP._0) {
        return;
      }

      if (CharacterFsm.CurrentStateIs<CharacterStateDead>(f, characterEntity) || 
          CharacterFsm.CurrentStateIs<CharacterStateKnocked>(f, characterEntity)  ||  
          CharacterFsm.CurrentStateIs<CharacterStateHealing>(f, characterEntity) ||
          CharacterFsm.CurrentStateIs<CharacterStateKnifeAttack>(f, characterEntity)) {
        return;
      }

      var settings = KnockHelper.ResolveKnockSettings(f);
      if (!settings.enabled) {
        return;
      }

      EntityRef targetEntity      = EntityRef.None;
      CharacterStateKnocked* targetKnocked  = null;
      FP bestDistanceSqr          = FP.MaxValue;

      var knockedFilter = f.Filter<CharacterStateKnocked>();
      while (knockedFilter.NextUnsafe(out EntityRef knockedEntity, out CharacterStateKnocked* knocked)) {
        if (knocked->CandidateRescuer != characterEntity) {
          continue;
        }

        if (knocked->Rescuer != EntityRef.None && knocked->Rescuer != characterEntity) {
          continue;
        }

        if (targetEntity != EntityRef.None && knocked->CandidateDistanceSqr >= bestDistanceSqr) {
          continue;
        }

        targetEntity     = knockedEntity;
        targetKnocked    = knocked;
        bestDistanceSqr  = knocked->CandidateDistanceSqr;
      }

      if (targetEntity == EntityRef.None) {
        return;
      }

      targetKnocked->Rescuer     = characterEntity;
      targetKnocked->ReviveTimer = targetKnocked->ReviveDuration;
      targetKnocked->SetCandidate(characterEntity, bestDistanceSqr);

      var revivingState = new CharacterStateReviving {
        Target = targetEntity,
      };

      CharacterFsm.TryEnterState(f, characterEntity, revivingState);

      InputHelper.ResetMovementAndSprintInput(f, characterEntity);
    }
  }
}
