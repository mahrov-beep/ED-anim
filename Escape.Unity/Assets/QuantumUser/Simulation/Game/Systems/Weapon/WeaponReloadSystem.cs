namespace Quantum {
  using UnityEngine.Scripting;

  [Preserve]
  public unsafe class WeaponReloadSystem : SystemMainThreadFilter<WeaponReloadSystem.Filter> {
    public struct Filter {
      public EntityRef Entity;

      public Weapon* Weapon;
      public Item*   Item;
    }

    public override void Update(Frame f, ref Filter filter) {
      var owner = filter.Item->Owner;
      var isTurret = f.Has<Turret>(owner);
      if (owner == EntityRef.None) {
        return;
      }

      if (f.Has<UnitExited>(owner)) {
        return;
      }

      if (!f.TryGetPointer(owner, out Unit* unit)) {
        return; // овнер не юнит (по идее такого быть не может)
      }

      EntityRef weaponRef = filter.Entity;

      if (!isTurret && (CharacterFsm.CurrentStateIs<CharacterStateKnocked>(f, owner) ||
                        CharacterFsm.CurrentStateIs<CharacterStateReviving>(f, owner) ||
                        CharacterFsm.CurrentStateIs<CharacterStateHealing>(f, owner) ||
                        CharacterFsm.CurrentStateIs<CharacterStateRoll>(f, owner) ||
                        CharacterFsm.CurrentStateIs<CharacterStateKnifeAttack>(f, owner))) {
        return;
      }

      bool isActiveWeapon = unit->ActiveWeaponRef == weaponRef; // юнит держит это оружие в руках?
      if (!isTurret && !isActiveWeapon) {
        return;
      }
     
      Weapon* weapon = filter.Weapon;

      bool needStartReload = weapon->IsEmptyMagazine || weapon->PreReloadingTimer.IsSet;
      
      if (needStartReload && !weapon->IsReloading) {
        if (!weapon->PreReloadingTimer.IsSet) {
          weapon->PreReloadingTimer = FrameTimer.FromSeconds(f, weapon->GetConfig(f).preReloadAmmoSeconds);
        }

        if (!weapon->PreReloadingTimer.IsRunning(f)) {
          if (!weapon->TryStartReload(f, owner, weaponRef)) {
            // try to load AmmoBox from trash if reload cannot be started
            if (weapon->TryLoadAmmo(f, owner, weaponRef)) {
              weapon->TryStartReload(f, owner, weaponRef);
            }
          }
        }
      }

      if (!weapon->IsReloading) {
        return;
      }

      bool finishReload = weapon->ReloadingTimer.ProcessTimer(f);
      if (finishReload) {
        weapon->PreReloadingTimer = default;
        weapon->FinishReload(f, owner, weaponRef);

        return;
      }
    }
  }
}