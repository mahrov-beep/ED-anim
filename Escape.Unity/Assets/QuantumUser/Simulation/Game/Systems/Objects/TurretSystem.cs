using Photon.Deterministic;

namespace Quantum {
  using UnityEngine;
  public unsafe class TurretSystem : SystemMainThreadFilter<TurretSystem.Filter> {
    public struct Filter {
      public EntityRef    Entity;
      public Transform3D* Transform;

      public Turret*                   Turret;
      public Unit*                     Unit;
      public Team*                     Team;
      public CharacterSpectatorCamera* Spectator;

      public ref FPVector3    Position => ref Transform->Position;
      public ref FPQuaternion Rotation => ref Transform->Rotation;
    }

    public override ComponentSet Without { get; } = ComponentSet.Create<CharacterStateDead>();

    public override void Update(Frame f, ref Filter filter) {
      var e               = filter.Entity;
      var turret          = filter.Turret;
      var unit            = filter.Unit;

      var triggerArea = f.GetPointer<TriggerArea>(turret->TriggerAreaEntity);

      // сброс активной цели если она вне TriggerArea
      if (unit->Target != EntityRef.None && !triggerArea->HasEntityInside(f, unit->Target)) {
        unit->Target    = EntityRef.None;
        unit->HasTarget = false;
      }

      // поиск новой цели
      if (!f.Exists(unit->Target)) {
        var myTeam = *filter.Team;

        unit->Target = triggerArea->SearchClosestEntityInside(f,
                filter: TargetFilter,
                state: (e, myTeam, filter.Position));

        unit->HasTarget = unit->Target != EntityRef.None;
       }

      if (!unit->HasTarget) {
        return;
      }

      if (!LineOfSightHelper.HasLineSightFast(f, e, unit->Target)) {
        unit->Target    = EntityRef.None;
        unit->HasTarget = false;
        return;
      }

      var targetTransform = f.GetPointer<Transform3D>(unit->Target);
      var targetPosition  = targetTransform->Position;
      var toTarget        = (targetPosition - filter.Position)/*.XOZ*/.Normalized;

      var desiredHorizontalAngle = FPVector3.SignedAngle(
              FPVector3.Forward,
              toTarget,
              FPVector3.Up);

      desiredHorizontalAngle *= FP.Deg2Rad;

      filter.Spectator->SpectatorCameraDesiredRotation.Yaw = desiredHorizontalAngle;
    }

    static bool TargetFilter(Frame f, TriggerArea.SearchData it, (EntityRef e, Team myTeam, FPVector3 myPosition) turret) {
      var isSelf = it.OtherEntity == turret.e;
      if (isSelf) {
        return false;
      }

      var otherTeam = f.GetPointer<Team>(it.OtherEntity);
      if (otherTeam == turret.myTeam) {
        return false;
      }

      if (CharacterFsm.CurrentStateIs<CharacterStateDead>(f, it.OtherEntity) || f.Has<UnitExited>(it.OtherEntity)) {
        return false;
      }

      if (!f.Has<Unit>(it.OtherEntity)) {
        return false;
      }

      bool hasLineSight = LineOfSightHelper.HasLineSightFast(f, turret.e, it.OtherEntity);
      if (!hasLineSight) {
        return false;
      }

      return true;
    }
  }
}