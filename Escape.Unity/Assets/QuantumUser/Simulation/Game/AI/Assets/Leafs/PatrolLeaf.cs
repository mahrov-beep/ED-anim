namespace Quantum {
  using System;
  using Photon.Deterministic;
  using UnityEngine.Serialization;

  [Serializable]
  public unsafe class PatrolLeaf : BTLeaf {
    public BTDataIndex currentWaypointIndex;

    public override void Init(BTParams p, ref AIContext c) {
      p.SetIntData(0, currentWaypointIndex);
    }

    protected override BTStatus OnUpdate(BTParams p, ref AIContext c) {
      var f          = p.Frame;
      var data       = c.Data();
      var pathfinder = data.Pathfinder;

      if (data.CurrentWay == null) {
        return BTStatus.Failure;
      }

      var index    = p.GetIntData(currentWaypointIndex);
      var waypoint = WayHelper.GetWaypoint(f, data.CurrentWay, index);

      if (WayHelper.IsNear(data.Position, waypoint, FP._2)) {
        index = WayHelper.GetNextIndex(f, data.CurrentWay, index);
        p.SetIntData(index, currentWaypointIndex);
        return BTStatus.Success;
      }

      if (!BotNavMeshHelper.IsMoving(pathfinder)) {
        data.Bot->Intent.MovementTarget = waypoint;
        BotNavMeshHelper.SetTarget(f, pathfinder, waypoint);
      }

      return BTStatus.Running;
    }

    public override void OnExit(BTParams p, ref AIContext c) {
      base.OnExit(p, ref c);
      var data = c.Data();
      BotNavMeshHelper.Stop(p.Frame, p.Entity, data.Pathfinder);
      data.InputContainer->ResetAllInput();
    }

    public override void OnAbort(BTParams p, ref AIContext c, BTAbort abortType) {
      base.OnAbort(p, ref c, abortType);
      var data = c.Data();
      BotNavMeshHelper.Stop(p.Frame, p.Entity, data.Pathfinder);
      data.InputContainer->ResetAllInput();
    }
  }
}