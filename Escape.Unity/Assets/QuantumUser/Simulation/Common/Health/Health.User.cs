namespace Quantum {
  using Photon.Deterministic;

  public unsafe partial struct Health {
    public bool IsFull    => CurrentValue >= MaxValue;
    public bool IsNotZero => CurrentValue > 0;
    public bool IsDead    => CurrentValue <= FP._0;

    public void ModifyHealth(Frame f, FP valueToAdd) {
      CurrentValue = FPMath.Clamp(CurrentValue + valueToAdd, FP._0, MaxValue);
      SetupUnitDeadComponent(f, EntityRef.None);
    }

    public void ModifyMaxHealth(Frame f, FP valueToAdd) {
      MaxValue = FPMath.Max(MaxValue + valueToAdd, 0);

      // при изменении максимального здоровья сразу меняем и само здоровье
      ModifyHealth(f, valueToAdd > FP._0 ? valueToAdd : FP._0);
    }

    public void ApplyHeal(Frame f, EntityRef source, FP valueToHeal) {
      CurrentValue = FPMath.Clamp(CurrentValue + valueToHeal, FP._0, MaxValue);
      SetupUnitDeadComponent(f, EntityRef.None);

      f.Signals.OnUnitHeal(source, SelfEntity, valueToHeal);
      f.Events.UnitHeal(source, SelfEntity, valueToHeal);
    }

    public void ApplyDamage(Frame f, EntityRef source, FP baseDamage, EDamageType damageType) {
      var target = SelfEntity;

      bool isKnocked = CharacterFsm.CurrentStateIs<CharacterStateKnocked>(f, target);

      if (!isKnocked && IsDead) {
        return;
      }

      if (f.Has<Attributes>(target)) {
        if (AttributesHelper.IsValueSet(f, target, EAttributeType.Set_Immunity)) {
          return;
        }
      }

      if (f.TryGetPointer(target, out Unit* targetUnit)) {
        var resistMult = targetUnit->CurrentStats.resistAllMultiplier +
                         GetResistMultiplierByDamageType(targetUnit->CurrentStats, damageType);

        baseDamage = FPMath.Clamp(baseDamage * FP._2 - (baseDamage * resistMult).AsFP, FP._0, baseDamage);
      }

      bool isCrit = CritAttackHelper.TryCritDamage(f, source, target, 
              ref baseDamage);

      if (isKnocked) {
        if (!ApplyDamageWhileKnocked(f, source, baseDamage)) {
          return;
        }

      //  SetupUnitDeadComponent(f, source);
        DispatchDamageEvents(f, source, target, baseDamage, isCrit);
        return;
      }
      
      CurrentValue = FPMath.Clamp(CurrentValue - baseDamage, FP._0, MaxValue);
      bool isDeadAfterHit = CurrentValue <= FP._0;
      bool enteredKnock   = false;

      if (isDeadAfterHit) {
        enteredKnock = TryEnterKnockdown(f, source);
      }

      SetupUnitDeadComponent(f, source);

      DispatchDamageEvents(f, source, target, baseDamage, isCrit);

      if (isDeadAfterHit && !enteredKnock) {
        f.Signals.OnUnitDead(target);
        f.Events.UnitDead(target);
      }
    }

    void SetupUnitDeadComponent(Frame f, EntityRef source) {
      bool isKnocked = CharacterFsm.CurrentStateIs<CharacterStateKnocked>(f, SelfEntity);

      if (IsDead && !CharacterFsm.CurrentStateIs<CharacterStateDead>(f, SelfEntity) && !isKnocked) {
        var deadState = new CharacterStateDead {
          KilledBy = GetKillerUnit(),
        };
        CharacterFsm.TryEnterState(f, SelfEntity, deadState);
      }
      else if (!IsDead && !isKnocked && CharacterFsm.CurrentStateIs<CharacterStateDead>(f, SelfEntity)) {
        CharacterFsm.TryEnterState(f, SelfEntity, new CharacterStateIdle());
      }

      EntityRef GetKillerUnit() {
        var killer = KnockHelper.ResolveDamageSourceUnitRef(f, source);
        if (killer == EntityRef.None && source != EntityRef.None) {
          Log.Error("Unexpected damage source: not unit or attack");
        }

        return killer;
      }
    }

    bool TryEnterKnockdown(Frame f, EntityRef source) {
      if (f.Has<Turret>(SelfEntity)) {
        return false;
      }

      if (CharacterFsm.CurrentStateIs<CharacterStateKnocked>(f, SelfEntity)) {
        return true;
      }

      var settings = KnockHelper.ResolveKnockSettings(f);

      var crawlMultiplier = settings.crawlSpeedMultiplier > FP._0 ? settings.crawlSpeedMultiplier : KnockSettings.Default.crawlSpeedMultiplier;
      crawlMultiplier = FPMath.Clamp(crawlMultiplier, FP._0, FP._1);

      var reviveValue      = settings.revivedHealthValue;
      var knockStartHealth = settings.knockStartHealth;
      var knock = new CharacterStateKnocked {
        KnockDuration        = settings.knockDurationSec,
        KnockTimer           = settings.knockDurationSec,
        ReviveDuration       = settings.reviveDurationSec,
        ReviveTimer          = settings.reviveDurationSec,
        CrawlSpeedMultiplier = crawlMultiplier,
        RevivedHealthValue   = FPMath.Clamp(reviveValue, FP._0, MaxValue),
        Rescuer              = EntityRef.None,
        LastDamageSource     = KnockHelper.ResolveDamageSourceUnitRef(f, source),
        CandidateRescuer     = EntityRef.None,
        CandidateDistanceSqr = FP.MaxValue,
        KnockHealth          = knockStartHealth,
        DamageImmunityTimer  = settings.knockDamageImmunityDurationSec,
        KnockStartHealth     = knockStartHealth,
      };

      return CharacterFsm.TryEnterState(f, SelfEntity, knock);
    }

    bool ApplyDamageWhileKnocked(Frame f, EntityRef source, FP appliedDamage) {
      if (appliedDamage <= FP._0) {
        return false;
      }

      if (!f.TryGetPointer(SelfEntity, out CharacterStateKnocked* knocked)) {
        return false;
      }

      var startKnockHealth = knocked->KnockStartHealth;

      if (knocked->HasDamageImmunity) {
        return false;
      }

      knocked->LastDamageSource = KnockHelper.ResolveDamageSourceUnitRef(f, source);

      if (MaxValue <= FP._0) {
        knocked->KnockHealth = FP._0;
        knocked->KnockTimer  = FP._0;
        return true;
      }

      knocked->KnockHealth = FPMath.Max(FP._0, knocked->KnockHealth - appliedDamage);

      if (knocked->KnockDuration > FP._0 && startKnockHealth > FP._0) {
        var timeDelta = (appliedDamage * knocked->KnockDuration) / startKnockHealth;
        knocked->KnockTimer = FPMath.Max(FP._0, knocked->KnockTimer - timeDelta);
      }

      return true;
    }

    void DispatchDamageEvents(Frame f, EntityRef source, EntityRef target, FP appliedDamage, bool isCrit) {
      f.Signals.OnUnitDamage(source, target, appliedDamage);

      if (f.TryGet(source, out Attack attack)) {
        attack.HealthApplicator.Value = appliedDamage;
        var attackTransform     = f.GetPointer<Transform3D>(source);
        var criticalViewInUnity = isCrit || attack.IsHeadshot;
        f.Events.UnitDamage(attack, target, criticalViewInUnity, attackTransform->Position);
      }
    }

    static FPBoostedMultiplier GetResistMultiplierByDamageType(UnitStats stats, EDamageType damageType) {
      return damageType switch {
        EDamageType.Bullet => stats.resistBulletMultiplier,
        EDamageType.Fire => stats.resistFireMultiplier,
        EDamageType.Explosion => stats.resistExplosionMultiplier,
        EDamageType.DamageZone => stats.resistZoneMultiplier,
        EDamageType.Melee => stats.resistMeleeMultiplier,
        _ => FPBoostedMultiplier.One,
      };
    }
  }
}