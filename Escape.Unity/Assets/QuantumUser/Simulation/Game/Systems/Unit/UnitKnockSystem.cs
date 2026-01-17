namespace Quantum {
  using Photon.Deterministic;
  using UnityEngine.Scripting;

  [Preserve]
  public unsafe class UnitKnockSystem : SystemMainThreadFilter<UnitKnockSystem.Filter> {
    public struct Filter {
      public EntityRef            Entity;
      public Unit*                Unit;
      public CharacterStateKnocked* Knocked;
      public Health*              Health;
      public Team*                Team;
      public Transform3D*         Transform;
    }

    public override void Update(Frame f, ref Filter filter) {
      var entity  = filter.Entity;
      var knocked = filter.Knocked;

      var settings = KnockHelper.ResolveKnockSettings(f);
      if (!settings.enabled) {
        FinalizeDeath(f, ref filter, knocked);
        return;
      }

      knocked->TickDamageImmunity(f.DeltaTime);

      if (knocked->IsBeingRevived) {
        if (KnockHelper.IsRescuerValid(f,
                entity,
                knocked,
                filter.Team,
                filter.Transform,
                knocked->Rescuer,
                settings,
                out var distanceSqr)) {
          knocked->SetCandidate(knocked->Rescuer, distanceSqr);
          ProcessRescue(f, ref filter, knocked, settings);
          return;
        }

        StopRescue(f, knocked);
        UpdateReviveCandidate(f, ref filter, settings);
      } else {
        UpdateReviveCandidate(f, ref filter, settings);
      }

      if (!knocked->IsBeingRevived) {
        ApplyKnockAutoDecay(f, knocked, filter.Health);
      }

      if (knocked->KnockHealth <= FP._0) {
        FinalizeDeath(f, ref filter, knocked);
      }
    }

    static void UpdateReviveCandidate(Frame f, ref Filter knockedFilter, KnockSettings settings) {
      var knocked = knockedFilter.Knocked;
      if (KnockHelper.TryFindBestRescuer(f,
              knockedFilter.Entity,
              knocked,
              knockedFilter.Team,
              knockedFilter.Transform,
              settings,
              out var candidate,
              out var distanceSqr)) {
        knocked->SetCandidate(candidate, distanceSqr);
      } else if (!knocked->IsBeingRevived) {
        knocked->ClearCandidate();
      }
    }

    static void ProcessRescue(Frame f, ref Filter knockedFilter, CharacterStateKnocked* knocked, KnockSettings settings) {
      var rescuer = knocked->Rescuer;

      //SanitizeRescuerInput(f, rescuer);
      var revivingState = new CharacterStateReviving {
        Target = knockedFilter.Entity,
      };
      CharacterFsm.TryEnterState(f, rescuer, revivingState);

      if (!knocked->ReviveTimer.ProcessTimer(f)) {
        return;
      }

      CompleteRescue(f, ref knockedFilter, knocked, settings);
    }

    static void StopRescue(Frame f, CharacterStateKnocked* knocked) {
      var rescuer = knocked->Rescuer;
      knocked->ResetRescue();

      if (rescuer != EntityRef.None && (CharacterFsm.CurrentStateIs<CharacterStateReviving>(f, rescuer) ||
                                        CharacterFsm.CurrentStateIs<CharacterStateHealing>(f, rescuer) ||
                                        CharacterFsm.CurrentStateIs<CharacterStateKnifeAttack>(f, rescuer))) {
        CharacterFsm.TryEnterState(f, rescuer, new CharacterStateIdle());
      }
    }

    static void CompleteRescue(Frame f, ref Filter knockedFilter, CharacterStateKnocked* knocked, KnockSettings settings) {
      var rescuer = knocked->Rescuer;
      var entity  = knockedFilter.Entity;

      var health      = knockedFilter.Health;
      var restoreBase = knocked->RevivedHealthValue > FP._0
              ? knocked->RevivedHealthValue
              : KnockSettings.Default.revivedHealthValue;
      var restore = FPMath.Clamp(restoreBase, FP._1, health->MaxValue);

      StopRescue(f, knocked);

      CharacterFsm.TryEnterState(f, entity, new CharacterStateIdle());
      health->ApplyHeal(f, rescuer, restore);

      if (f.TryGetPointer<Bot>(entity, out var bot)) {
        bot->ForceBTUpdate = true;
      }
    }

    static void FinalizeDeath(Frame f, ref Filter knockedFilter, CharacterStateKnocked* knocked) {
      var entity = knockedFilter.Entity;
      var killer = KnockHelper.ResolveDamageSourceUnitRef(f, knocked->LastDamageSource);

      StopRescue(f, knocked);

      knockedFilter.Health->ForceDeath(f, killer);

      var deadState = new CharacterStateDead {
        KilledBy = killer,
      };
      CharacterFsm.TryEnterState(f, entity, deadState);

      f.Signals.OnUnitDead(entity);
      f.Events.UnitDead(entity);
    }

    static void SanitizeRescuerInput(Frame f, EntityRef rescuer) {
      InputHelper.ResetMovementAndSprintInput(f, rescuer);
    }

    static void ApplyKnockAutoDecay(Frame f, CharacterStateKnocked* knocked, Health* health) {
      if (knocked->KnockDuration <= FP._0) {
        return;
      }

      var deltaTime = f.DeltaTime;
      if (deltaTime <= FP._0) {
        return;
      }

      knocked->KnockTimer = FPMath.Max(FP._0, knocked->KnockTimer - deltaTime);

      var startKnockHealth = knocked->KnockStartHealth;
      if (startKnockHealth <= FP._0) {
        knocked->KnockHealth = FP._0;
        return;
      }

      var autoDecayPerSecond = startKnockHealth / knocked->KnockDuration;
      var autoDecay          = autoDecayPerSecond * deltaTime;
      knocked->KnockHealth   = FPMath.Max(FP._0, knocked->KnockHealth - autoDecay);
    }
  }
}
