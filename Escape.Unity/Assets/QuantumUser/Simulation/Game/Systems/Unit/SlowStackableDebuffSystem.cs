namespace Quantum {
  using Photon.Deterministic;
  using UnityEngine;
  using static SlowStackableDebuffSystem;

  public unsafe class SlowStackableDebuffSystem : SystemMainThreadFilter<Filter>, ISignalOnUnitDamage {
    public struct Filter {
      public EntityRef   Entity;
      public Unit*       Unit;
      public SlowDebuff* Debuff;
    }

    public override void Update(Frame f, ref Filter filter) {
      var debuff = filter.Debuff;

      if (debuff->timer.ProcessTimer(f)) {
        debuff->hitCount       = 0;
        debuff->slowMultiplier = FP._0;
        debuff->priority       = -1;
        debuff->hitLimit       = 0;

        return;
      }

      if (debuff->hitCount == 0) {
        return;
      }

      var unit = filter.Unit;

      unit->CurrentStats.moveSpeed *= FPMath.Clamp(FP._1 - debuff->slowMultiplier, FP._0_20, FP._1);
    }

    public void OnUnitDamage(Frame f, EntityRef source, EntityRef target, FP value) {
      if (!f.TryGetPointer(target, out SlowDebuff* debuff)) {
        return;
      }

      if (!f.TryGetPointer<Attack>(source, out var attack)) {
        return;
      }
      
      var sourceUnit = f.GetPointer<Unit>(attack->SourceUnitRef);
      if (!f.Exists(sourceUnit->ActiveWeaponRef)) {
        return;
      }

      var weaponCfg = sourceUnit->GetActiveWeaponConfig(f);
      if (weaponCfg!.slowDuration <= FP._0) {
        return;
      }

      if (weaponCfg.slowStackLimit <= 0) {
        return;
      }

      var newPrio  = weaponCfg.slowDebuffPriority;
      var newLimit = weaponCfg.slowStackLimit;

      if (debuff->hitCount > 0) {
        if (newPrio > debuff->priority) {
          debuff->hitCount       = 1;
          debuff->slowMultiplier = FP._0;
          debuff->priority       = newPrio;
          debuff->hitLimit       = newLimit;
        }
        else if (newPrio == debuff->priority) {
          debuff->hitLimit = Mathf.Max(debuff->hitLimit, newLimit);
        }
        else {
          return;
        }
      }
      else {
        debuff->priority = newPrio;
        debuff->hitLimit = newLimit;
      }

      debuff->timer = FPMath.Max(debuff->timer, weaponCfg.slowDuration);

      if (debuff->hitCount >= debuff->hitLimit) {
        return;
      }

      debuff->hitCount++;
      debuff->slowMultiplier += weaponCfg.slowMultiplierPerStack;
      debuff->timer          =  weaponCfg.slowDuration;
    }
  }
}