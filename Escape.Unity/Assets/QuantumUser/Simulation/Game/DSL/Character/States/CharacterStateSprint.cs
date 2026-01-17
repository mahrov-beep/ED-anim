namespace Quantum {
  using Photon.Deterministic;

  public unsafe partial struct CharacterStateSprint : ICharacterState {
    public CharacterStates State => CharacterStates.Sprint;

    public bool CanEnter(Frame f, EntityRef characterRef) {
      if (!CharacterFsm.CurrentStateIs<CharacterStateWalk>(f, characterRef)) {
        return false;
      }

      if (!IsSprintAvailable(f, characterRef, entering: true)) {
        return false;
      }

      return true;
    }

    public void Enter(Frame f, EntityRef characterRef) {
      UnitColliderHeightHelper.ResetHeight(f, characterRef);
    }

    public void Update(Frame f, EntityRef characterRef) {
      if (!IsSprintAvailable(f, characterRef, entering: false)) {
        CharacterFsm.TryEnterState(f, characterRef, new CharacterStateWalk());
        return;
      }
    }

    bool IsSprintAvailable(Frame f, EntityRef characterRef, bool entering) {
      if (!f.TryGetPointer(characterRef, out Unit* unit)) {
        return false;
      }

      if (!f.TryGetPointer(characterRef, out UnitFeatureSprintWithStamina* stamina)) {
        return false;
      }

      var unitConfig = f.FindAsset(unit->Asset);

      // пользователь отключил спринт
      if (!f.TryGetPointer(characterRef, out InputContainer* inputContainer) ||
          !inputContainer->Input.SprintButton.IsDown) {
        return false;
      }

      if (entering) {
        if (!f.TryGetPointer(characterRef, out KCC* kcc) || !kcc->Data.IsGrounded) {
          return false;
        }
      }

      // недостаточно стамины чтобы начать спринт (но продолжать можно)
      if (entering && !stamina->CanStartSprint(unitConfig.sprintSettings)) {
        return false;
      }

      // полностью закончилась стамина
      if (stamina->IsDepleted) {
        return false;
      }

      // меняем оружие
      if (unit->HideWeaponTimer > 0 || unit->GetWeaponTimer > 0) {
        return false;
      }

      if (f.TryGetPointer(unit->ActiveWeaponRef, out Weapon* activeWeapon)) {
        // начали перезарядку
        if (activeWeapon->IsReloading) {
          return false;
        }
      }

      return true;
    }
  }
}