namespace Quantum {
  using Photon.Deterministic;

  public unsafe partial struct CharacterStateCrouchIdle : ICharacterState {
    public CharacterStates State => CharacterStates.CrouchIdle;

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

      if (input->InputAccelerated.Magnitude > FP._0_05) {
        var unit = f.GetPointer<Unit>(characterRef);
        var crouchMoveState = new CharacterStateCrouchMove {
          CrouchSpeedMultiplier = CrouchHelper.ResolveCrouchSettings(f, unit).CrouchSpeedMultiplier,
        };      
        if (CharacterFsm.TryEnterState(f, characterRef, crouchMoveState)) {
          return;
        }
      }
    }
  }
}


