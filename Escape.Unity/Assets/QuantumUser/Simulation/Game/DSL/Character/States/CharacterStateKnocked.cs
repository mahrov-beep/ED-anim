namespace Quantum {
  using Photon.Deterministic;

  public unsafe partial struct CharacterStateKnocked : ICharacterState {
    public CharacterStates State => CharacterStates.Knocked;

    public bool CanEnter(Frame f, EntityRef characterRef) {
      var settings = KnockHelper.ResolveKnockSettings(f);
      if (!settings.enabled) {
        return false;
      }

      if (KnockDuration <= FP._0 || ReviveDuration <= FP._0) {
        return false;
      }

      return KnockHelper.HasAliveTeammate(f, characterRef);
    }

    public void Enter(Frame f, EntityRef characterRef) {
      InputHelper.ResetMovementInput(f, characterRef);
      ClearCandidate();

      var settings = KnockHelper.ResolveKnockSettings(f);
      var ratio    = settings.knockHeightRatio;
      UnitColliderHeightHelper.ApplyHeight(f, characterRef, ratio);
    }

    public void Update(Frame f, EntityRef characterRef) {
      
    }
  }
}
