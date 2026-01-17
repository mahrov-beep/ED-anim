using Photon.Deterministic;

namespace Quantum {
  using UnityEngine.Scripting;

  [Preserve]
  public unsafe class MineSystem : SystemMainThreadFilter<MineSystem.Filter> {

    public struct Filter {
      public EntityRef Entity;
      public Transform3D* Transform;
      public Mine* Mine;
      public Team* Team;

      public ref FPVector3 Position => ref Transform->Position;
      public ref FPQuaternion Rotation => ref Transform->Rotation;
    }

    public override void Update(Frame f, ref Filter filter) {
      var mineEntity = filter.Entity;
      var mine = filter.Mine;
      var mineTeam = *filter.Team;

      if (!f.Exists(mine->TriggerAreaEntity)) {
        return;
      }

      var triggerArea = f.GetPointer<TriggerArea>(mine->TriggerAreaEntity);

      if (mine->IsTriggered) {
        mine->ExplosionTimer.Tick(f.DeltaTime);

        if (!mine->ExplosionTimer.IsRunning) {
          Explode(f, mineEntity, filter.Position, mine);

          var triggerAreaEntity = mine->TriggerAreaEntity;
          f.Destroy(mineEntity);
          if (triggerAreaEntity != EntityRef.None && f.Exists(triggerAreaEntity)) {
            f.Destroy(triggerAreaEntity);
          }
        }

        return;
      }

      var enemyDetected = triggerArea->SearchAnyEntityInside(f,
        TargetFilter,
        (mineEntity, mineTeam, filter.Position, mine->VisibilityDistance));

      if (enemyDetected != EntityRef.None) {
        mine->IsTriggered = true;
        mine->ExplosionTimer.Start(mine->ExplosionDelay);

        f.Signals.OnMineTrigger(mineEntity);
      }
    }

    static bool TargetFilter(Frame f, TriggerArea.SearchData it,
      (EntityRef mineEntity, Team mineTeam, FPVector3 minePosition, FP visibilityDistance) data) {

      var otherEntity = it.OtherEntity;

      if (otherEntity == data.mineEntity) {
        return false;
      }

      if (!f.TryGetPointer(otherEntity, out Team* otherTeam)) {
        return false;
      }

      if (otherTeam->Index == data.mineTeam.Index) {
        return false;
      }

      if (CharacterFsm.CurrentStateIs<CharacterStateDead>(f, otherEntity) || f.Has<UnitExited>(otherEntity)) {
        return false;
      }

      if (!f.Has<Unit>(otherEntity)) {
        return false;
      }

      var otherTransform = f.GetPointer<Transform3D>(otherEntity);
      var distance = FPVector3.Distance(data.minePosition, otherTransform->Position);

      return distance <= data.visibilityDistance;
    }
    private void Explode(Frame f, EntityRef mineEntity, FPVector3 explosionPosition, Mine* mine) {
      if (!mine->ExplosionAttackData.IsValid) {
        Log.Warn($"[Mine] Attack data not configured for mine {mineEntity}. Explosion skipped.");
        return;
      }

      Attack attack = default;
      attack.AttackData = mine->ExplosionAttackData;
      attack.SourceUnitRef = mine->Owner;
      attack.MaxDistance = mine->ExplosionRadius;
      attack.HealthApplicator = mine->ExplosionHealthApplicator;

      var attackRef = f.Create();
      f.Set(attackRef, Transform3D.Create(explosionPosition, FPQuaternion.Identity));
      f.Set(attackRef, attack);

      f.Events.MineExploded(mineEntity, explosionPosition, mine->ExplosionRadius);
    }
  }
}
