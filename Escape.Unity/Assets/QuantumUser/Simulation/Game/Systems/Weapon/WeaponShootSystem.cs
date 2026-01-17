using UnityEngine.Scripting;

namespace Quantum {
  using Photon.Deterministic;

  [Preserve]
  public unsafe class WeaponShootSystem : SystemMainThreadFilter<WeaponShootSystem.Filter> {
    public struct Filter {
      public EntityRef    Entity;
      public Transform3D* Transform;

      public Unit*           Unit;
      public InputContainer* InputContainer;
    }

    public override ComponentSet Without { get; } = ComponentSet.Create<CharacterStateDead, UnitExited, CharacterStateKnocked, CharacterStateReviving, CharacterStateHealing, CharacterStateKnifeAttack, BotInvisibleByPlayer>();

    public override void Update(Frame f, ref Filter filter) {  

      if (filter.InputContainer->ButtonAbilityIsDown) {
        return;
      }

      if (CharacterFsm.CurrentStateIs<CharacterStateSprint>(f, filter.Entity)) {
        return;
      }

      Unit* unit = filter.Unit;

      if (unit->HasDelayedAbility(f)) {
        return;
      }

      EntityRef weaponEntity = filter.Unit->ActiveWeaponRef;
      if (weaponEntity == EntityRef.None) {
        return;
      }

      EntityRef shooterEntity = filter.Entity;
      Weapon*   weapon        = f.Unsafe.GetPointer<Weapon>(weaponEntity);

      if (!unit->HasTarget) {
        return;
      }

      if (f.TryGetPointer(filter.Entity, out Bot* bot) && bot->SuppressFire) {
        return;
      }

      if (unit->IsWeaponChanging) {
        return;
      }

      if (CharacterFsm.CurrentStateIs<CharacterStateRoll>(f, shooterEntity)) {
        return;
      }    

      if (!weapon->CanShoot(f, shooterEntity, unit->Target)) {
        return;
      }

      // TODO очень много жалоб что боты не стреляют, надо как-то по другому сделать.
      // if (f.GameMode.BotCantShootInStrafe && f.TryGetPointer(shooterEntity, out Bot* bot)) {
      //   var inStrafe = bot->InStrafe(f, shooterEntity);
      //   if (inStrafe) {
      //     return;
      //   }
      // }

      CreateShoot(f, shooterEntity, weaponEntity, weapon);
    }

    public void CreateShoot(Frame f, EntityRef unitRef, EntityRef weaponRef, Weapon* weapon) {
      WeaponItemAsset weaponConfig = weapon->GetConfig(f);
      AttackData      attackData   = f.FindAsset(weaponConfig.attackData);

      var unit    = f.GetPointer<Unit>(unitRef);
      var unitAim = f.GetPointer<UnitAim>(unitRef);

      var weaponStats = weapon->CurrentStats;

      var shotOrigin       = f.GameModeAiming.GetAimOrigin(f, unitRef); 
      var shotTarget       = unitAim->AimCurrentPosition;
      var shotDirection    = shotTarget - shotOrigin;

      var bulletsPerShoot = weaponConfig.bulletsPerShot;

      FP minDamage = weaponStats.minDamage.AsFP;
      FP maxDamage = weaponStats.maxDamage.AsFP;

      if (f.TryGetPointer(unitRef, out Bot* bot)) {
        if (bot->StatsMultipliers != default) {
          var m = f.FindAsset(bot->StatsMultipliers);
          minDamage *= m.Damage;
          maxDamage *= m.Damage;
        }
      }

      for (short i = 0; i < bulletsPerShoot; i++) {
        var bulletRef = f.Create();

        FP effectiveDamage = unit->RNG.Next(minDamage, maxDamage);

        HealthApplicator damageApplicator = HealthApplicator.CreateDamage(
                effectiveDamage, EDamageType.Bullet);

        var spreadAngle = weapon->currentShootingSpread;

        var angleX   = spreadAngle * unit->RNG.Next() - spreadAngle / 2;
        var angleY   = spreadAngle * unit->RNG.Next() - spreadAngle / 2;
        var rotation = FPQuaternion.Euler(angleX, angleY, 0);

        var bulletDirection = rotation * shotDirection;
        
        f.Set(bulletRef, Transform3D.Create(
          shotOrigin,
          FPQuaternion.LookRotation(bulletDirection, FPVector3.Up)
        ));

        f.Set(bulletRef, new WeaponAttack {
          WeaponConfig = weaponConfig,
        });

        // must set last, after all other components
        // otherwise AttackData.Create will be with incorrect data
        f.Set(bulletRef, new Attack {
          MaxDistance               = weaponStats.attackDistance,
          AttackData                = (AssetRef<AttackData>)attackData,
          HealthApplicator          = damageApplicator,
          SourceUnitRef             = unitRef,
          ProjectileSpeedMultiplier = weaponStats.projectileSpeedMultiplier,
          DistanceDamageMultiplier  = weaponStats.distanceDamageMultiplier,
        });
      }

      var maxAmmo = weapon->MaxAmmo;
      if (maxAmmo > 0) {
        weapon->BulletsCount -= 1;
      }

      var fireIntervalSeconds = FP._1 * 60 / weaponConfig.fireRate;
      weapon->FireRateTimer = FrameTimer.FromSeconds(f, fireIntervalSeconds);

      f.Signals.OnCreateShoot(unitRef, weaponRef, weapon);
      f.Events.OnShoot(unitRef, weaponRef);
    }
  }
}