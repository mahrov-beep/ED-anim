namespace Quantum {
  using Photon.Deterministic;

  public unsafe partial struct CharacterStateReviving : ICharacterState {
    public CharacterStates State => CharacterStates.Reviving;

    public bool CanEnter(Frame f, EntityRef characterRef) => true;

    public void Enter(Frame f, EntityRef characterRef) {
      //InputHelper.ResetMovementInput(f, characterRef);
    }

    public void Update(Frame f, EntityRef characterRef) {      
    }
  }
}
