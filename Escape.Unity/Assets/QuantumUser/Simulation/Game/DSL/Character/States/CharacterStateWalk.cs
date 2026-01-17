namespace Quantum {
  using Photon.Deterministic;

  public unsafe partial struct CharacterStateWalk : ICharacterState {
    public CharacterStates State => CharacterStates.Walk;

    public bool CanEnter(Frame f, EntityRef characterRef) => true;

    public void Enter(Frame f, EntityRef characterRef) {
      UnitColliderHeightHelper.ResetHeight(f, characterRef);
    }

    public void Update(Frame f, EntityRef characterRef) {
      var input = f.GetPointer<InputContainer>(characterRef);

      if (input->InputAccelerated.SqrMagnitude < FP._0_10) {
        if (CharacterFsm.TryEnterState(f, characterRef, new CharacterStateIdle())) {
          return;
        }
      }
      
    }
  }
}