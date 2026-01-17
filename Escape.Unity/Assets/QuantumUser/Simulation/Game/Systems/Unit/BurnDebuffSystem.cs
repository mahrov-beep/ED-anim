namespace Quantum {
  using Photon.Deterministic;
  public unsafe class BurnDebuffSystem : SystemSignalsOnly, ISignalOnUnitDamage {
    public void OnUnitDamage(Frame f, EntityRef source, EntityRef target, FP value) {
      if (!f.TryGetPointer<Attack>(source, out var attack)) {
        return;
      }      
      
      if (attack->DamageType == EDamageType.DamageZone) {
        return;
      }
      
      var sourceUnit = f.GetPointer<Unit>(attack->SourceUnitRef);

      if (!f.Exists(sourceUnit->ActiveWeaponRef)) {
        return;
      } // иначе конфиг точно не null

      var weaponCfg = sourceUnit->GetActiveWeaponConfig(f);

      if (weaponCfg!.fireDamage < FP._0_01) {
        return;
      }

      if (weaponCfg.fireDuration < FP._0_01) {
        return;
      }

      if (weaponCfg.fireChancePercent == FP._0) {
        return;
      }

      var random = sourceUnit->RNG.NextInclusive(1, 100);

      bool isBurn = random <= weaponCfg.fireChancePercent;
      if (isBurn) {
        var applicator = HealthApplicator.CreateBurn(weaponCfg.fireDamage, weaponCfg.fireDuration);
        applicator.ApplyOn(f, attack->SourceUnitRef, target);
      }
    }
  }
}