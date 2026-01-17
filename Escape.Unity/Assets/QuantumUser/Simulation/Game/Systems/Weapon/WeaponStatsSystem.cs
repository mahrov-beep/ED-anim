namespace Quantum {
  using UnityEngine;
  using Photon.Deterministic;
  public unsafe class WeaponStatsSystem : SystemMainThreadFilter<WeaponStatsSystem.Filter>,
          ISignalOnCharacterAfterLoadoutSlotAssigned {
    public struct Filter {
      public EntityRef Entity;

      public Item*       Item;
      public WeaponItem* WeaponItem;
      public Weapon*     Weapon;

      public ItemOwnerIsUnit* ItemOwnerIsUnit; // Update only unit-owned items for performance optimization
    }

    public void OnCharacterAfterLoadoutSlotAssigned(Frame f, EntityRef unitRef, CharacterLoadoutSlots slot, EntityRef itemRef) {
      if (!CharacterLoadoutSlotsExtension.IsWeaponSlot(slot)) {
        return;
      }

      var weapon     = f.GetPointer<Weapon>(itemRef);
      var item       = f.GetPointer<Item>(itemRef);
      var weaponItem = f.GetPointer<WeaponItem>(itemRef);

      Calculate(f, itemRef, weapon, item, weaponItem);
    }

    public override void Update(Frame f, ref Filter filter) {
      if (!f.IsVerified) {
        return;
      }

      var weaponRef  = filter.Entity;
      var weapon     = filter.Weapon;
      var item       = filter.Item;
      var weaponItem = filter.WeaponItem;

      if (f.Has<BotInvisibleByPlayer>(item->Owner)) {
        // обновляем статы невидимых ботов только каждый 10й кадр, для оптимизации
        const int schedulePeriod = 10;
        if (weaponRef.Index % schedulePeriod != f.Number % schedulePeriod) {
          return;
        }
      }

      Calculate(f, weaponRef, weapon, item, weaponItem);
    }

    public void Calculate(Frame f, EntityRef weaponRef, Weapon* weapon, Item* item, WeaponItem* weaponItem) {
      var weaponAsset = weapon->GetConfig(f);

      using var c = FPBoostedCalculator.Create(f);

      // collect attribute source
      CollectAttributeSources(c, f, weaponRef, item, weaponItem);

      // apply attributes
      ref var stats = ref weapon->CurrentStats;

      // resets stats to default, then reapply all boosts
      stats = weaponAsset.GetBaseStats();

      stats.minDamage *= c.CalcPercentMult(EAttributeType.PercentBoost_WeaponDamage);
      stats.maxDamage *= c.CalcPercentMult(EAttributeType.PercentBoost_WeaponDamage);

      var isAiming = f.TryGetPointer(item->Owner, out Unit* ownerUnitAiming) && ownerUnitAiming->Aiming;

      if (isAiming) {
        stats.spreadAngle *= weaponAsset.spreadCoefficientInAimState;
      }

      stats.spreadAngle *= c.CalcPercentMult(EAttributeType.PercentBoost_ShootingSpread);

      {
        var ownerRef = item->Owner;
        if (ownerRef != EntityRef.None && f.TryGetPointer(ownerRef, out Unit* ownerUnit)) {
          bool isCrouching = CharacterFsm.CurrentStateIs<CharacterStateCrouchIdle>(f, ownerRef) ||
                             CharacterFsm.CurrentStateIs<CharacterStateCrouchMove>(f, ownerRef);

          KCC* ownerKcc = null;
          bool hasKcc   = f.TryGetPointer(ownerRef, out ownerKcc);
          bool isJumping = hasKcc && !ownerKcc->Data.IsGrounded;

          FP stateMultiplier = FP._1;

          if (ownerUnit->Asset.IsValid) {
            var spreadSettings = f.FindAsset(ownerUnit->Asset).GetAimSpreadSettings();
            stateMultiplier    = spreadSettings.ResolveMultiplier(isJumping, isCrouching);
          }
          else {
            if (isJumping) {
              stateMultiplier = FP._1_25;
            }
            else if (isCrouching) {
              stateMultiplier = FP._0_50;
            }
          }

          stats.spreadAngle *= stateMultiplier;
        }
      }

      stats.reloadingTime  *= c.CalcPercentMult(EAttributeType.PercentBoost_WeaponReloadDuration);
      stats.attackDistance *= c.CalcPercentMult(EAttributeType.PercentBoost_WeaponTriggerDistance);
      if (isAiming) {
        stats.attackDistance *= c.CalcPercentMult(EAttributeType.PercentBoost_WeaponTriggerDistanceInAim);
      }
      stats.triggerAngleX  *= FPBoostedMultiplier.One;
      stats.triggerAngleY  *= FPBoostedMultiplier.One;
      stats.maxAmmo        *= c.CalcPercentMult(EAttributeType.PercentBoost_MaxAmmo);
      stats.critChance     *= c.CalcPercentMult(EAttributeType.PercentBoost_CritChance);
      stats.critDamage     *= c.CalcPercentMult(EAttributeType.PercentBoost_CritDamage);

      stats.preShotAimingSeconds *= c.CalcPercentMult(EAttributeType.PercentBoost_PreShotAiming);

      stats.projectileSpeedMultiplier          *= c.CalcPercentMult(EAttributeType.PercentBoost_ProjectileSpeed);
      stats.distanceDamageMultiplier           *= c.CalcPercentMult(EAttributeType.PercentBoost_WeaponDistanceDamage);
      stats.shootingSpreadInMovementMultiplier *= c.CalcPercentMult(EAttributeType.PercentBoost_ShootingSpreadInMovement);
      stats.recoilXMultiplier                  *= c.CalcPercentMult(EAttributeType.PercentBoost_RecoilX);
      stats.recoilYMultiplier                  *= c.CalcPercentMult(EAttributeType.PercentBoost_RecoilY);

      stats.weaponShotSoundRange *= c.CalcPercentMult(EAttributeType.PercentBoost_WeaponShotSoundRange);
    }

    static void CollectAttributeSources(FPBoostedCalculator c, Frame f, EntityRef weaponEntity, Item* item, WeaponItem* weaponItem) {
      // apply attributes from self weapon
      c.AddSource(weaponEntity);

      // apply attributes from weapon attachments
      foreach (var attachmentSlot in WeaponAttachmentSlotsExtension.AllValidSlots) {
        var attachmentAtSlot = weaponItem->AttachmentAtSlot(attachmentSlot);
        if (attachmentAtSlot != EntityRef.None) {
          c.AddSource(attachmentAtSlot);
        }
      }

      // apply attributes from loadout items
      if (f.TryGetPointer(item->Owner, out CharacterLoadout* loadout)) {
        foreach (var loadoutSlot in CharacterLoadoutSlotsExtension.NonWeaponSlots) {
          var itemAtSlot = loadout->ItemAtSlot(loadoutSlot);
          if (itemAtSlot != EntityRef.None) {
            c.AddSource(itemAtSlot);
          }
        }
      }

      // apply attributes from unit
      if (f.Has<Unit>(item->Owner)) {
        c.AddSource(item->Owner);
      }
    }
  }
}