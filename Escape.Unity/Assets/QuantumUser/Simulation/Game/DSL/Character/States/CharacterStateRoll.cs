namespace Quantum {
  public unsafe partial struct CharacterStateRoll : ICharacterState {
    public CharacterStates State => CharacterStates.Roll;

    public bool CanEnter(Frame f, EntityRef characterRef) => true;

   public void Enter(Frame f, EntityRef characterRef) {
    }

    public void Update(Frame f, EntityRef characterRef) {
      if (StateTimer.IsSet && !StateTimer.IsRunning(f)) {
        CharacterFsm.TryEnterState(f, characterRef, new CharacterStateIdle());
      }
    }
  }
}