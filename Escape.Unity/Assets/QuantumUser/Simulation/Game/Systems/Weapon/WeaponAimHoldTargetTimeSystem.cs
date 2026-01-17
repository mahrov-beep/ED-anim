namespace Quantum {
  using Photon.Deterministic;

  public unsafe class WeaponAimHoldTargetTimeSystem : SystemMainThreadFilter<WeaponAimHoldTargetTimeSystem.Filter>,
          ISignalOnUnitHideWeapon,
          ISignalOnUnitGetWeapon {
    public struct Filter {
      public EntityRef Entity;

      public Unit* Unit;
    }

    public override void Update(Frame f, ref Filter filter) {
      if (filter.Unit->HasTarget) {
        filter.Unit->WeaponAimSecondsElapsed += f.DeltaTime;
      }
      else {
        filter.Unit->WeaponAimSecondsElapsed = FP._0;
      }
    }

    public void OnUnitHideWeapon(Frame f, EntityRef unitRef) {
      var unit = f.GetPointer<Unit>(unitRef);

      unit->WeaponAimSecondsElapsed = FP._0;
    }

    public void OnUnitGetWeapon(Frame f, EntityRef unitRef) {
      var unit = f.GetPointer<Unit>(unitRef);

      unit->WeaponAimSecondsElapsed = FP._0;
    }
  }
}