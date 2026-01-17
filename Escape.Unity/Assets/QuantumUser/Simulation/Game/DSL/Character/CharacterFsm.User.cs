namespace Quantum {
  using JetBrains.Annotations;

  public unsafe partial struct CharacterFsm {
    [PublicAPI]
    public static CharacterFsm* Of(Frame f, EntityRef characterRef) {
      return f.Unsafe.GetPointer<CharacterFsm>(characterRef);
    }

    [PublicAPI]
    public static bool CanEnterState<TState>(Frame f, EntityRef characterRef)
      where TState : unmanaged, ICharacterState {
      return default(TState).CanEnter(f, characterRef);
    }

    [PublicAPI]
    public static bool TryEnterState<TState>(Frame f, EntityRef characterRef, TState state)
      where TState : unmanaged, ICharacterState {
      return Of(f, characterRef)->TryEnterState(f, state);
    }

    [PublicAPI]
    public static bool CurrentStateIs<TState>(Frame f, EntityRef characterRef)
      where TState : unmanaged, ICharacterState {
      return f.Has<TState>(characterRef);
    }

    [PublicAPI]
    public static bool CurrentStateIs<TState>(Frame f, EntityRef characterRef, out TState* state)
      where TState : unmanaged, ICharacterState {
      return f.TryGetPointer(characterRef, out state);
    }

    [PublicAPI]
    public bool TryEnterState<TState>(Frame f, TState newState)
      where TState : unmanaged, ICharacterState {
      if (!newState.CanEnter(f, SelfEntity)) {
        return false;
      }

      var prevState = CurrentState;

      if (CurrentState != CharacterStates.Invalid) {
        f.Remove(SelfEntity, CurrentStateTypeId);

        CurrentState       = CharacterStates.Invalid;
        CurrentStateTypeId = 0;
      }

      CurrentState       = newState.State;
      CurrentStateTypeId = ComponentTypeId<TState>.Id;

      f.Add(SelfEntity, newState, out var statePtr);

#if UNITY_EDITOR || DEVELOPMENT_BUILD
      if (f.IsVerified && f.Context.IsLocalPlayer(f.Get<Unit>(SelfEntity).PlayerRef)) {
        // Log.Info($"[CharacterFsm(local)]: {prevState} -> {newState.State}");
      }
#endif

      statePtr->Enter(f, SelfEntity);
      return true;
    }
  }

  public interface ICharacterState : IComponent {
    CharacterStates State { get; }

    [PublicAPI]
    bool CanEnter(Frame f, EntityRef characterRef);

    [PublicAPI]
    void Enter(Frame f, EntityRef characterRef);

    [PublicAPI]
    void Update(Frame f, EntityRef characterRef);
  }
}