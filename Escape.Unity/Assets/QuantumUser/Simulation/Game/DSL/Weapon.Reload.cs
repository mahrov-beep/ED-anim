namespace Quantum {
  using System;
  using Photon.Deterministic;
public unsafe partial struct Weapon {
  public short MaxAmmo         => FPMathHelper.RoundToInt16(CurrentStats.maxAmmo);
  public bool  IsReloading     => ReloadingTimer > FP._0;
  public bool  IsEmptyMagazine => BulletsCount <= 0;
  public bool  IsFullMagazine  => BulletsCount >= MaxAmmo;

  public static void LoadAmmoFromAmmoBoxToWeapon(Frame f, EntityRef weaponRef) {
    if (!f.TryGetPointers(weaponRef, out Weapon* weapon, out WeaponItem* weaponItem, out Item* item)) {
      return;
    }

    if (!weaponItem->HasAttachmentAtSlot(WeaponAttachmentSlots.Ammo)) {
      return;
    }

    weapon->FinishReload(f, item->Owner, weaponRef);
  }

  public static void UnloadAmmoFromWeaponToAmmoBox(Frame f, EntityRef weaponRef) {
    if (!f.TryGetPointers(weaponRef, out WeaponItem* weaponItem, out Weapon* weapon)) {
      return;
    }

    if (!weaponItem->HasAttachmentAtSlot(WeaponAttachmentSlots.Ammo)) {
      return;
    }

    weapon->UnloadBulletsToAmmoBox(f, weaponItem->AttachmentAtSlot(WeaponAttachmentSlots.Ammo));
  }

  public void UnloadBulletsToAmmoBox(Frame f, EntityRef ammoBoxItemRef) {
    if (BulletsCount > Item.GetUsedUsages(f, ammoBoxItemRef)) {
      var weaponConfig  = GetConfig(f);
      var ammoBoxConfig = f.FindAsset(f.GetPointer<Item>(ammoBoxItemRef)->Asset);
      // такое возможно и если произойдет то патроны потеряются, но всё равно нужно выгрузить ВСЕ патроны
      Log.Error($"BulletCount in weapon '{weaponConfig.ItemKey}' greater than empty space in attached AmmoBox '{ammoBoxConfig.ItemKey}'");
    }

    Item.DeUseItem(f, ammoBoxItemRef, BulletsCount);
    BulletsCount = 0;
    
    ResetReloadingTimer();
  }

  public bool TryLoadAmmo(Frame f, EntityRef ownerUnit, EntityRef weaponRef) {
    if (f.Has<Turret>(ownerUnit)) {
      return false;
    }

    if (IsReloading) {
      return false;
    }

    if (IsFullMagazine) {
      return false;
    }

    if (!f.TryGetPointer(ownerUnit, out CharacterLoadout* loadout)) {
      return false;
    }

    if (!f.TryGetPointer(weaponRef, out WeaponItem* weaponItem)) {
      return false;
    }

    if (weaponItem->HasAttachmentAtSlot(WeaponAttachmentSlots.Ammo)) {
      return false;
    }

    if (!loadout->TryFindAmmoBoxInTrash(f, GetConfig(f), out var ammoBoxRef)) {
      return false;
    }

    if (!loadout->RemoveItemFromTetris(f, ammoBoxRef)) {
      return false;
    }

    if (!weaponItem->AssignAttachmentToSlot(f, WeaponAttachmentSlots.Ammo, ammoBoxRef)) {
      Log.Error("WeaponLoadAmmo failed to assign AmmoBox to weapon slot");
      return false;
    }

    return true;
  }

  public bool TryStartReload(Frame f, EntityRef ownerUnit, EntityRef weaponRef) {
    var isTurret = f.Has<Turret>(ownerUnit);
    
    if (IsReloading) {
      return false;
    }

    if (MaxAmmo == 0) {
      return false;
    }

    if (IsFullMagazine) {
      return false;
    }

    if (GetAvailableAmmo(f, weaponRef) <= 0 && !f.Has<Bot>(ownerUnit) && !isTurret) {
      return false;
    }
    
    ReloadingTimer = CurrentStats.reloadingTime;

    f.Signals.OnReloading(ownerUnit, weaponRef);
    return true;
  }

  public void ResetReloadingTimer() {
    ReloadingTimer    = FP._0;
    PreReloadingTimer = default;
  }

  public void FinishReload(Frame f, EntityRef ownerUnit, EntityRef weaponRef) {
    if (f.Has<Bot>(ownerUnit) || f.Has<Turret>(ownerUnit)) {
      var ammo = MaxAmmo;
      BulletsCount   = (short)ammo;
      ReloadingTimer = -FP._1;
      return;
    }

    if (!TryGetAmmoItem(f, weaponRef, out var ammoItemRef)) {
      Log.Error("FinishReload called but no AmmoBox attached");
      return;
    }

    var missingBullets = Math.Clamp(MaxAmmo - BulletsCount, 0, MaxAmmo);

    MergeEnoughBulletsToAmmoFromInventory(f, weaponRef, ownerUnit, wantsUsages: missingBullets);

    var availableAmmo = GetAvailableAmmo(f, weaponRef);
    var ammoToLoad    = Math.Min(availableAmmo, missingBullets);

    BulletsCount   += (short)ammoToLoad;
    ReloadingTimer =  -FP._1;

    Item.UseItem(f, ammoItemRef, ammoToLoad);
  }

  bool TryGetAmmoItem(Frame f, EntityRef weaponRef, out EntityRef ammoItemRef) {
    if (!f.TryGetPointer(weaponRef, out WeaponItem* weaponItem)) {
      ammoItemRef = EntityRef.None;
      return false;
    }

    ammoItemRef = weaponItem->AttachmentAtSlot(WeaponAttachmentSlots.Ammo);
    return ammoItemRef != EntityRef.None;
  }

  void MergeEnoughBulletsToAmmoFromInventory(Frame f, EntityRef weaponRef, EntityRef characterRef, int wantsUsages) {
    if (!TryGetAmmoItem(f, weaponRef, out var ammoRef)) {
      return;
    }

    if (!f.TryGetPointer(characterRef, out CharacterLoadout* loadout)) {
      return;
    }

    int maxIterations = 100;
    while (GetAvailableAmmo(f, weaponRef) is var availableAmmo && 
           availableAmmo < wantsUsages &&
           Item.GetUsedUsages(f, ammoRef) > 0 &&
           loadout->TryFindMergeableInTrash(f, ammoRef, out var mergeSourceItemRef, skipEmptyItems: true)) {
      var missingUsages = Math.Min(wantsUsages - availableAmmo, Item.GetUsedUsages(f, ammoRef));
      
      if (missingUsages <= 0) {
        break;
      }

      loadout->Merge(f, sourceItemRef: mergeSourceItemRef, targetItemRef: ammoRef, maxUsagesToMerge: missingUsages);

      if (maxIterations-- <= 0) {
        Log.Error("MergeEnoughBulletsToAmmoFromInventory infinite loop");
        break;
      }
    }
  }

  public int CalcAvailableAmmoWithInventory(Frame f, EntityRef weaponRef, EntityRef characterRef) {
    var count = 0;
    
    if (TryGetAmmoItem(f, weaponRef, out var ammoRef)) {
      count += Item.GetRemainingUsages(f, ammoRef);

      if (f.TryGet(characterRef, out CharacterLoadout loadout)) {
        count += loadout.GetAllMergeableInTrashRemainingUsagesSum(f, ammoRef);
      }
    }

    return count;
  }
  
  public int GetAvailableAmmo(Frame f, EntityRef weaponRef) {
    if (!TryGetAmmoItem(f, weaponRef, out var ammoItemRef)) {
      return 0;
    }

    return Item.GetRemainingUsages(f, ammoItemRef);
  }
}
}