namespace Quantum {
  using Photon.Deterministic;

  public unsafe partial struct CharacterStateIdle : ICharacterState {
    public CharacterStates State => CharacterStates.Idle;

    public bool CanEnter(Frame f, EntityRef characterRef) => true;

    public void Enter(Frame f, EntityRef characterRef) {
      UnitColliderHeightHelper.ResetHeight(f, characterRef);
    }

    public void Update(Frame f, EntityRef characterRef) {
      var input = f.GetPointer<InputContainer>(characterRef);

      if (input->InputAccelerated.Magnitude > FP._0_05) {
        if (CharacterFsm.TryEnterState(f, characterRef, new CharacterStateWalk())) {
          return;
        }
      }
      
    }
  }
}