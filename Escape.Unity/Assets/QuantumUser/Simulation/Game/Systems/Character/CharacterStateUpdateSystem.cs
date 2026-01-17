namespace Quantum {
  public unsafe class CharacterStateUpdateSystem<TState> : SystemMainThreadFilter<CharacterStateUpdateSystem<TState>.Filter>
    where TState : unmanaged, ICharacterState {
    public struct Filter {
      public EntityRef Entity;

      public TState* State;
    }

    public override void Update(Frame f, ref Filter filter) {
      filter.State->Update(f, filter.Entity);
    }
  }
}