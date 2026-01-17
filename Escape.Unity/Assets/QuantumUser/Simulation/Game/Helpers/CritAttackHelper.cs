namespace Quantum {
  using Photon.Deterministic;
  public static unsafe class CritAttackHelper {
    public static bool TryCritDamage(Frame f, EntityRef attackRef, EntityRef target, ref FP damage) {
      if (!f.TryGetPointer<Attack>(attackRef, out var attack)) {
        return false;
      }

      if (!f.TryGetPointer<Unit>(attack->SourceUnitRef, out var sourceUnit)) {
        return false;
      }

      if (!f.TryGetPointer<Unit>(target, out var targetUnit)) {
        return false;
      }

      var weaponItemAsset = sourceUnit->GetActiveWeaponConfig(f);
      if (!weaponItemAsset) {
        return false;
      }

      var weapon = f.GetPointer<Weapon>(sourceUnit->ActiveWeaponRef);

      var chancePercent    = weapon->CurrentStats.critChance.AsFP;
      var addDamagePercent = weapon->CurrentStats.critDamage.AsFP;

      if (chancePercent <= FP._1) {
        return false;
      }

      if (addDamagePercent <= FP._1) {
        return false;
      }

      /*
      // чтобы была возможность настроить очень маленький крит
      FP minCritChancePercent = FPMath.Min(f.GameMode.minCritChancePercent, chancePercent);
      // подрезаем крит шанс источника резистом к шансу крита цели, но не меньше минимального значения.
      FPBoostedMultiplier resistChance = targetUnit->CurrentStats.resistCritChanceMultiplier;
      chancePercent = FPMath.Clamp((chancePercent * resistChance).AsFP, minCritChancePercent, FP._100);
      */

      var random = sourceUnit->RNG.NextInclusive(FP._1, FP._100);

      var isCrit = random <= chancePercent;

      if (isCrit) {
        var critDamage = damage * (addDamagePercent * FP._0_01);

        var resistedCritDamage = (critDamage * targetUnit->CurrentStats.resistCritDamageMultiplier).AsFP;

        /*
        // тут минимум чтобы если у оружия задано значение ниже минимального бралось именно оно
        FP minCritPercent = FPMath.Min(f.GameMode.minCritDamagePersent * FP._0_01, addDamagePersent);

        FP minCritDamage  = damage * (minCritPercent - FP._1);
        resistedCritDamage = FPMath.Max(resistedCritDamage, minCritDamage);
        */

        critDamage = FPMath.Clamp(
                FP._2 * critDamage - resistedCritDamage,
                FP._0,
                critDamage
        );

        damage += critDamage;
      }

      return isCrit;
    }
  }
}
