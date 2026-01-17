namespace Quantum {
  using Photon.Deterministic;

  public unsafe class WeaponCalcShootingSpreadSystem : SystemMainThreadFilter<WeaponCalcShootingSpreadSystem.Filter> {
    public struct Filter {
      public EntityRef Entity;

      public Weapon*          Weapon;
      public Item*            Item;
      public ItemOwnerIsUnit* ItemOwnerIsUnit;
    }

    public override void Update(Frame f, ref Filter filter) {
      if (!f.TryGetPointer(filter.Item->Owner, out Unit* unit)) {
        return;
      }

      var weapon       = filter.Weapon;
      var weaponStats  = weapon->CurrentStats;
      var weaponConfig = f.FindAsset(weapon->Config);

      FP spread = weaponStats.spreadAngle.AsFP;
      if (f.TryGetPointer(filter.Item->Owner, out Bot* bot)) {
        if (bot->StatsMultipliers != default) {
          var m = f.FindAsset(bot->StatsMultipliers);
          spread *= m.Spread;
        }
      }
      weapon->currentShootingSpread = spread;

      if (unit->CurrentSpeed > FP._0) {
        var spreadSpeedCoefficient = weaponConfig.spreadSpeedCoefficient;
        spreadSpeedCoefficient *= weaponStats.shootingSpreadInMovementMultiplier;

        var additionalMoveSpread = weaponConfig.baseSpreadAngle * unit->CurrentSpeedCoefficient * spreadSpeedCoefficient;
        weapon->currentShootingSpread += additionalMoveSpread;
      }
    }
  }
}