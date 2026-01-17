namespace Quantum {
  using Photon.Deterministic;

  public unsafe partial struct CharacterStateHealing : ICharacterState {
    public CharacterStates State => CharacterStates.Healing;

    public bool CanEnter(Frame f, EntityRef characterRef) {
      if (CharacterFsm.CurrentStateIs<CharacterStateDead>(f, characterRef) ||
          CharacterFsm.CurrentStateIs<CharacterStateKnocked>(f, characterRef) ||
          CharacterFsm.CurrentStateIs<CharacterStateReviving>(f, characterRef) ||
          CharacterFsm.CurrentStateIs<CharacterStateRoll>(f, characterRef)) {
        return false;
      }

      if (!f.TryGetPointer(characterRef, out Health* health)) {
        return false;
      }

      return health->CurrentValue < health->MaxValue;
    }

    public void Enter(Frame f, EntityRef characterRef) {
      Timer = Duration;
      InputHelper.ResetMovementAndSprintInput(f, characterRef);
    }

    public void Update(Frame f, EntityRef characterRef) {
      if (Duration <= FP._0) {
        Complete(f, characterRef);
        return;
      }

      Timer -= f.DeltaTime;
      if (Timer > FP._0) {
        return;
      }

      Complete(f, characterRef);
    }

    void Complete(Frame f, EntityRef characterRef) {
      Timer = FP._0;
      Applicator.ApplyOn(f, characterRef, characterRef);
      CharacterFsm.TryEnterState(f, characterRef, new CharacterStateIdle());
    }
  }
}

