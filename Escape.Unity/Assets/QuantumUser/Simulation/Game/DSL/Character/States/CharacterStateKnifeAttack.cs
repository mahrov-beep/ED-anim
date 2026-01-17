namespace Quantum {
  using Photon.Deterministic;
  public unsafe partial struct CharacterStateKnifeAttack : ICharacterState {
    public CharacterStates State => CharacterStates.KnifeAttack;

    public bool CanEnter(Frame f, EntityRef characterRef) {
      if (CharacterFsm.CurrentStateIs<CharacterStateDead>(f, characterRef) ||
          CharacterFsm.CurrentStateIs<CharacterStateKnocked>(f, characterRef) ||
          CharacterFsm.CurrentStateIs<CharacterStateHealing>(f, characterRef) ||
          CharacterFsm.CurrentStateIs<CharacterStateReviving>(f, characterRef) ||
          CharacterFsm.CurrentStateIs<CharacterStateRoll>(f, characterRef) ||
          CharacterFsm.CurrentStateIs<CharacterStateKnifeAttack>(f, characterRef)) {
        return false;
      }

      return true;
    }

    public void Enter(Frame f, EntityRef characterRef) {
      InputHelper.ResetMovementAndSprintInput(f, characterRef);

      if (f.TryGetPointer(characterRef, out Unit* unit) && !StateTimer.IsSet) {
        var duration = ResolveDuration(KnifeAttackHelper.ResolveSettings(f, unit));
        if (duration > FP._0) {
          StateTimer = FrameTimer.FromSeconds(f, duration);
        }
      }
    }

    public void Update(Frame f, EntityRef characterRef) {
      if (!f.TryGetPointer(characterRef, out Unit* unit)) {
        CharacterFsm.TryEnterState(f, characterRef, new CharacterStateIdle());
        return;
      }

      var knifeSettings = KnifeAttackHelper.ResolveSettings(f, unit);
      var duration      = ResolveDuration(knifeSettings);
      if (duration <= FP._0) {
        KnifeAttackHelper.ExecuteAttack(f, characterRef, knifeSettings);
        CharacterFsm.TryEnterState(f, characterRef, new CharacterStateIdle());
        return;
      }

      if (!StateTimer.IsSet) {
        StateTimer = FrameTimer.FromSeconds(f, duration);
      }

      TryTriggerKnifeAttack(f, characterRef, knifeSettings, duration);

      if (!StateTimer.IsRunning(f)) {
        CharacterFsm.TryEnterState(f, characterRef, new CharacterStateIdle());
      }
    }

    static FP ResolveDuration(KnifeSettings settings) {
      return settings.Duration > FP._0 ? settings.Duration : KnifeSettings.Default.Duration;
    }

    static FP ResolveAttackEvent(KnifeSettings settings, FP duration) {
      var attackEvent = settings.AttackEvent > FP._0 ? settings.AttackEvent : KnifeSettings.Default.AttackEvent;
      return FPMath.Clamp(attackEvent, FP._0, duration);
    }

    static int SecondsToFrames(Frame f, FP seconds) {
      if (seconds <= FP._0 || f.DeltaTime <= FP._0) {
        return 0;
      }

      return FPMath.CeilToInt(seconds / f.DeltaTime);
    }

    void TryTriggerKnifeAttack(Frame f, EntityRef characterRef, KnifeSettings knifeSettings, FP duration) {
      if (!StateTimer.IsSet) {
        return;
      }

      var attackEvent      = ResolveAttackEvent(knifeSettings, duration);
      var attackEventFrame = StateTimer.StartFrame + SecondsToFrames(f, attackEvent);
      var previousFrame    = f.Number - 1;

      if (previousFrame < attackEventFrame && f.Number >= attackEventFrame) {
        KnifeAttackHelper.ExecuteAttack(f, characterRef, knifeSettings);
      }
    }
  }
}
