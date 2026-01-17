namespace Quantum {
  public unsafe class UnitAimingFlagSystem : SystemMainThreadFilter<UnitAimingFlagSystem.Filter> {
    public struct Filter {
      public EntityRef Entity;

      public Unit*           Unit;
      public InputContainer* InputContainer;
    }

    public override void Update(Frame f, ref Filter filter) {
      var unit = filter.Unit;

      if (filter.InputContainer->Input.AimButton.WasPressed) {
        if (unit->Aiming) {
          unit->Aiming = false;
        }
        else {
          if (IsAimingAvailable(f, filter.Entity, unit)) {
            unit->Aiming = true;
          }
        }
      }

      if (unit->Aiming && !IsAimingAvailable(f, filter.Entity, unit)) {
        unit->Aiming = false;
      }
    }

    bool IsAimingAvailable(Frame f, EntityRef unitRef, Unit* unit) {
      if (CharacterFsm.CurrentStateIs<CharacterStateSprint>(f, unitRef)) {
        return false;
      }

      if (unit->IsWeaponChanging) {
        return false;
      }

      if (f.TryGetPointer(unitRef, out KCC* kcc) && !kcc->Data.IsGrounded) {
        return false;
      }

      if (f.TryGetPointer(unit->ActiveWeaponRef, out Weapon* activeWeapon)) {
        if (activeWeapon->IsReloading) {
          return false;
        }
      }

      return true;
    }
  }
}