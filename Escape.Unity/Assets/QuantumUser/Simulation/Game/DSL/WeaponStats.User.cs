namespace Quantum {
  using System;
  using Photon.Deterministic;

  public partial struct WeaponStats {
    public void VisitStats(Action<string, FPBoostedValue?, FPBoostedMultiplier?> visit) {
      visit("w_min_damage", minDamage, null);
      visit("w_max_damage", maxDamage, null);
      visit("w_spread_angle", spreadAngle, null);
      visit("w_reloading_time", reloadingTime, null);
      visit("w_attack_distance", attackDistance, null);
      visit("w_trigger_angle", triggerAngleX, null);
      visit("w_max_ammo", maxAmmo, null);
      visit("w_crit_chance", critChance, null);
      visit("w_crit_damage", critDamage, null);
      visit("w_pre_shot_aiming_seconds", preShotAimingSeconds, null);
      visit("w_shot_sound_range", weaponShotSoundRange, null);

      visit("w_projectile_speed", null, projectileSpeedMultiplier);
      visit("w_distance_damage", null, distanceDamageMultiplier);
      visit("w_shooting_spread_in_movement", null, shootingSpreadInMovementMultiplier);
      visit("w_recoil_x", null, recoilXMultiplier);
      visit("w_recoil_y", null, recoilYMultiplier);
    }
  }
}