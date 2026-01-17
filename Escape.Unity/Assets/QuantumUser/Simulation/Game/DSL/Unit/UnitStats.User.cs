namespace Quantum {
  using System;
  using System.Runtime.InteropServices;
  using Photon.Deterministic;

  public partial struct UnitStats : IEquatable<UnitStats> {
    public bool Equals(UnitStats other) {
      // без base.Equals(other) сравнение не очень надежно, но этот вызов вызывает заметные аллокации каждый кадр,
      // IEquatable используется только для обновления UI, так что некорректное вычисление не должно быть критично
      return this.GetHashCode() == other.GetHashCode()/* && base.Equals(other)*/;
    }

    public void VisitStats(Action<string, FPBoostedValue?, FPBoostedMultiplier?> visit) {
      visit("move_speed", moveSpeed, null);

      visit("resist_all", null, resistAllMultiplier);
      visit("resist_bullet", null, resistBulletMultiplier);
      visit("resist_fire", null, resistFireMultiplier);
      visit("resist_explosion", null, resistExplosionMultiplier);
      visit("resist_melee", null, resistMeleeMultiplier);
      visit("resist_zone", null, resistZoneMultiplier);
      // visit("resist_crit_chance", null, resistCritChanceMultiplier);
      // visit("resist_crit_damage", null, resistCritDamageMultiplier);
      visit("shot_impulse", null, shotImpulse);
      
      visit("vision_distance", null, visionDistanceMultiplier);

      visit("jump_impulse", jumpImpulse, null);
      visit("max_weight", maxWeight, null);
      visit("loadout_width", loadoutWidth, null);
      visit("loadout_height", loadoutHeight, null);
    }
  }
}