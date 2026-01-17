namespace Quantum {
  using Photon.Deterministic;

  public unsafe partial struct CharacterStateCrouchMove : ICharacterState {
    public CharacterStates State => CharacterStates.CrouchMove;

    public bool CanEnter(Frame f, EntityRef characterRef) {
      return true;
    }

    public void Enter(Frame f, EntityRef characterRef) {
      if (f.TryGetPointer(characterRef, out Unit* unit)) {
        var settings = CrouchHelper.ResolveCrouchSettings(f, unit);
        var ratio    = settings.CrouchHeightRatio;
        UnitColliderHeightHelper.ApplyHeight(f, characterRef, ratio);
      }
    }

    public void Update(Frame f, EntityRef characterRef) {
      var input = f.GetPointer<InputContainer>(characterRef);

      if (input->InputAccelerated.SqrMagnitude < FP._0_10) {
        if (CharacterFsm.TryEnterState(f, characterRef, new CharacterStateCrouchIdle())) {
          return;
        }
      }
    }
  }
}


