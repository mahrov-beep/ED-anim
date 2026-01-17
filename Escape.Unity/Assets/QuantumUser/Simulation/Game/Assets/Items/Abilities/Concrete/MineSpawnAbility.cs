namespace Quantum {
  using System;
  using Photon.Deterministic;
  using Prototypes;
  using UnityEngine;

  [Serializable]
  public class MineSpawnAbility : PlacementPreviewAbilityItem {
    [Header("Mine Detection")]
    [Tooltip("Distance at which enemies can see and trigger the mine")]
    public FP visibilityDistance = FP._3;

    [Header("Explosion Settings")]
    [Tooltip("Delay before explosion after triggering (in seconds)")]
    public FP explosionDelay = FP._1;

    [Tooltip("Explosion radius")]
    public FP explosionRadius = FP._5;

    [Tooltip("Attack prototype used to apply explosion damage/effects")]
    public AttackPrototype attackPrototype;

    protected override unsafe void SetupSpawned(Frame f, EntityRef spawnedRef, EntityRef ownerRef, Ability* ability) {
      var mine = f.GetPointer<Mine>(spawnedRef);
      mine->Owner = ownerRef;
      mine->VisibilityDistance = visibilityDistance;
      mine->ExplosionDelay = explosionDelay;
      mine->ExplosionRadius = explosionRadius;
      mine->IsTriggered = false;
      mine->ExplosionTimer = default;
      if (attackPrototype != null) {
        Attack explosionAttack = default;
        attackPrototype.Materialize(f, ref explosionAttack);

        mine->ExplosionAttackData = explosionAttack.AttackData;
        mine->ExplosionHealthApplicator = explosionAttack.HealthApplicator;
        mine->Damage = mine->ExplosionHealthApplicator.Value;
      }
      else {
        mine->ExplosionAttackData = default;
        mine->ExplosionHealthApplicator = default;
        mine->Damage = default;
      }

      if (f.TryGetPointer(ownerRef, out Team* ownerTeam)) {
        f.Set(spawnedRef, *ownerTeam);
      }
    }
  }
}

