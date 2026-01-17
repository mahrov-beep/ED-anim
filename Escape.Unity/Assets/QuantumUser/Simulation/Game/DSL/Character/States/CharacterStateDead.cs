namespace Quantum {
  public unsafe partial struct CharacterStateDead : ICharacterState {
    public CharacterStates State => CharacterStates.Dead;

    public bool CanEnter(Frame f, EntityRef characterRef) => true;

    public void Enter(Frame f, EntityRef characterRef) {
    }

    public void Update(Frame f, EntityRef characterRef) {
    }
  }
}