namespace Quantum {
  using Photon.Deterministic;

  public unsafe partial struct CharacterStateJump : ICharacterState {
    public CharacterStates State => CharacterStates.Jump;

    public bool CanEnter(Frame f, EntityRef characterEntity) {
      if (EAttributeType.Set_LockMovement.IsValueSet(f, characterEntity)) {
        return false;
      }

      if (CharacterFsm.CurrentStateIs<CharacterStateKnocked>(f, characterEntity) ||
          CharacterFsm.CurrentStateIs<CharacterStateReviving>(f, characterEntity) ||
          CharacterFsm.CurrentStateIs<CharacterStateHealing>(f, characterEntity) ||
          CharacterFsm.CurrentStateIs<CharacterStateKnifeAttack>(f, characterEntity)) {
        return false;
      }

      if (!f.TryGetPointers(characterEntity, out Unit* unit, out KCC* kcc)) {
        return false;
      }

      var unitAsset    = f.FindAsset(unit->Asset);
      var jumpSettings = unitAsset.GetJumpSettings();

      if (f.TryGetPointer(characterEntity, out UnitFeatureSprintWithStamina* stamina)) {
        var minRequired = jumpSettings.GetRequiredStamina(unitAsset.sprintSettings.maxStamina);
        if (stamina->current < minRequired) {
          return false;
        }
      }

      if (!kcc->Data.IsGrounded) {
        return false;
      }

      var impulseMagnitude = unit->CurrentStats.jumpImpulse.AsFP;
      if (impulseMagnitude <= FP._0) {
        return false;
      }

      return true;
    }

    public void Enter(Frame f, EntityRef characterRef) {
      if (!f.TryGetPointers(characterRef, out Unit* unit, out KCC* kcc)) {
        return;
      }

      var impulseMagnitude = unit->CurrentStats.jumpImpulse.AsFP;

      var impulse = new FPVector3(FP._0, impulseMagnitude, FP._0);
      kcc->Jump(impulse);

      var unitAsset    = f.FindAsset(unit->Asset);
      var jumpSettings = unitAsset.GetJumpSettings();

      if (f.TryGetPointer(characterRef, out UnitFeatureSprintWithStamina* stamina)) {
        var cost = jumpSettings.GetStaminaCost(unitAsset.sprintSettings.maxStamina);
        if (cost > FP._0) {
          stamina->current    = FPMath.Max(FP._0, stamina->current - FPMath.Min(cost, stamina->current));
          stamina->regenTimer = unitAsset.sprintSettings.regenDelay;
        }
      }

      unit->WeaponAimSecondsElapsed = FP._0;
    }

    public void Update(Frame f, EntityRef characterRef) {
      var minJumpDurationSeconds = FP._0_20;

      if (AirTime > minJumpDurationSeconds) {
        if (!f.TryGetPointer(characterRef, out KCC* kcc) || kcc->IsGrounded) {
          CharacterFsm.TryEnterState(f, characterRef, new CharacterStateIdle());
        }
      }

      AirTime += f.DeltaTime;
    }
  }
}