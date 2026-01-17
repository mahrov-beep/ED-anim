namespace Quantum {
  public unsafe class UnitStatsSystem : SystemMainThreadFilter<UnitStatsSystem.Filter>,
    ISignalOnCharacterAfterLoadoutSlotAssigned,
    ISignalOnComponentAdded<Unit> {
    public struct Filter {
      public EntityRef Entity;

      public Unit* Unit;
    }

    public void OnAdded(Frame f, EntityRef unitEntity, Unit* unit) {
      Calculate(f, unitEntity, unit);
    }

    public void OnCharacterAfterLoadoutSlotAssigned(Frame f, EntityRef unitEntity, CharacterLoadoutSlots slot, EntityRef itemEntity) {
      var unit = f.GetPointer<Unit>(unitEntity);
      Calculate(f, unitEntity, unit);
    }

    public override void Update(Frame f, ref Filter filter) {
      if (!f.IsPlayerVerifiedOrLocal(filter.Unit->PlayerRef)) {
        return;
      }

      var unitRef = filter.Entity;
      var unit    = filter.Unit;

      if (f.Has<BotInvisibleByPlayer>(unitRef)) {
        // обновляем статы невидимых ботов только каждый 10й кадр, для оптимизации
        const int schedulePeriod = 10;
        if (unitRef.Index % schedulePeriod != f.Number % schedulePeriod) {
          return;
        }
      }

      Calculate(f, unitRef, unit);
    }

    public void Calculate(Frame f, EntityRef unitEntity, Unit* unit) {
      var unitAsset = f.FindAsset(unit->Asset);
      if (!unitAsset) {
        f.LogError(unitEntity, "UnitAsset not found");
      }

      using var c = FPBoostedCalculator.Create(f);

      // collect attribute source
      CollectAttributeSources(c, f, unitEntity, unit);

      // apply attributes
      ref var stats = ref unit->CurrentStats;

      stats.maxWeight     = c.CalcAdditiveValue(unitAsset.baseMaxWeight, EAttributeType.AdditiveBoost_MaxWeight);
      stats.loadoutWidth  = c.CalcAdditiveValue(unitAsset.baseLoadoutWidth, EAttributeType.AdditiveBoost_LoadoutWidth);
      stats.loadoutHeight = c.CalcAdditiveValue(unitAsset.baseLoadoutHeight, EAttributeType.AdditiveBoost_LoadoutHeight);

      stats.moveSpeed     = unitAsset.baseMoveSpeed * c.CalcPercentMult(EAttributeType.PercentBoost_MoveSpeed);
      stats.rotationSpeed = unitAsset.baseRotationSpeed * FPBoostedMultiplier.One;

      stats.jumpImpulse = unitAsset.baseJumpImpulse * c.CalcPercentMult(EAttributeType.PercentBoost_JumpImpulse);

      stats.resistAllMultiplier        = c.CalcPercentMult(EAttributeType.PercentBoost_ResistAllDamage);
      stats.resistBulletMultiplier     = c.CalcPercentMult(EAttributeType.PercentBoost_ResistBullet);
      stats.resistFireMultiplier       = c.CalcPercentMult(EAttributeType.PercentBoost_ResistFire);
      stats.resistExplosionMultiplier  = c.CalcPercentMult(EAttributeType.PercentBoost_ResistExplosion);
      stats.resistMeleeMultiplier      = c.CalcPercentMult(EAttributeType.PercentBoost_ResistMelee);
      stats.resistZoneMultiplier       = c.CalcPercentMult(EAttributeType.PercentBoost_ResistZone);
      // stats.resistCritChanceMultiplier = c.CalcPercentMult(EAttributeType.PercentBoost_ResistCritChance);
      // stats.resistCritDamageMultiplier = c.CalcPercentMult(EAttributeType.PercentBoost_ResistCritDamage);

      stats.visionDistanceMultiplier = c.CalcPercentMult(EAttributeType.PercentBoost_FOWVisionDistance);
      stats.shotImpulse              = c.CalcPercentMult(EAttributeType.PercentBoost_ShotImpulse);

      stats.audioDistance = c.CalcPercentMult(EAttributeType.PercentBoost_AudioDistance);
      stats.audioVolume   = c.CalcPercentMult(EAttributeType.PercentBoost_AudioVolume);
    }

    static void CollectAttributeSources(FPBoostedCalculator c, Frame f, EntityRef unitEntity, Unit* unit) {
      // apply attributes from self unit
      c.AddSource(unitEntity);

      // apply attributes from active weapon
      if (f.TryGetPointer(unit->ActiveWeaponRef, out WeaponItem* activeWeaponItem)) {
        c.AddSource(unit->ActiveWeaponRef);

        // apply attributes from active weapon attachments
        foreach (var attachmentSlot in WeaponAttachmentSlotsExtension.AllValidSlots) {
          var attachmentAtSlot = activeWeaponItem->AttachmentAtSlot(attachmentSlot);
          if (attachmentAtSlot != EntityRef.None) {
            c.AddSource(attachmentAtSlot);
          }
        }
      }

      // apply attributes from loadout items (EXCEPT weapons)
      if (f.TryGetPointer(unitEntity, out CharacterLoadout* loadout)) {
        foreach (var loadoutSlot in CharacterLoadoutSlotsExtension.NonWeaponSlots) {
          var itemAtSlot = loadout->ItemAtSlot(loadoutSlot);
          if (itemAtSlot != EntityRef.None) {
            c.AddSource(itemAtSlot);
          }
        }
      }
    }
  }
}