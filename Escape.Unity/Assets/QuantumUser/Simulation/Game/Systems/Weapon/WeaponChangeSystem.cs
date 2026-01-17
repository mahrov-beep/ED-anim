namespace Quantum {
  using UnityEngine;
  using UnityEngine.Scripting;

  [Preserve]
  public unsafe class WeaponChangeSystem : SystemMainThreadFilter<WeaponChangeSystem.Filter> {
    public struct Filter {
      public EntityRef Entity;

      public Unit* Unit;
    }

    public override void Update(Frame f, ref Filter filter) {
      if (CharacterFsm.CurrentStateIs<CharacterStateKnocked>(f, filter.Entity) ||
          CharacterFsm.CurrentStateIs<CharacterStateReviving>(f, filter.Entity) ||
          CharacterFsm.CurrentStateIs<CharacterStateHealing>(f, filter.Entity) ||
          CharacterFsm.CurrentStateIs<CharacterStateRoll>(f, filter.Entity) ||
          CharacterFsm.CurrentStateIs<CharacterStateKnifeAttack>(f, filter.Entity)) {
        return;
      }

      Unit* unit = filter.Unit;

      if (!unit->IsWeaponChanging) {
        return;
      }

      if (unit->NeedToStartWeaponHideSignal(f)) {
        f.Signals.OnUnitHideWeapon(filter.Entity);
      }

      bool finishHiding = unit->HideWeaponTimer.ProcessTimer(f);

      if (!finishHiding) {
        return;
      }

      if (unit->NeedToStartWeaponGetSignal(f)) {
        f.Signals.OnUnitGetWeapon(filter.Entity);
      }

      bool finishGetting = unit->GetWeaponTimer.ProcessTimer(f);

      if (!finishGetting) {
        return;
      }
    }
  }
}