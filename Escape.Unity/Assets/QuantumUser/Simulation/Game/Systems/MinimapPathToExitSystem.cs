namespace Quantum {
  using Photon.Deterministic;
  using UnityEngine;
  using static MinimapPathToExitSystem;

  public unsafe class MinimapPathToExitSystem : SystemMainThreadFilter<Filter>,
          ISignalOnGameStart {

    public struct Filter {
      public EntityRef Entity;

      public Transform3D* Transform;

      public Unit*              Unit;
      public NavMeshPathfinder* Pathfinder;
    }

    public override bool UseCulling => false;

    int schedulePeriod = 6;

    public override ComponentSet Without { get; } = ComponentSet.Create<Bot>();

    public override void OnInit(Frame f) {
      schedulePeriod = Quantum.Input.MaxCount;
    }

    public void OnGameStart(Frame f) {
      var iter = f.FilterStruct(out Filter filter, Without);
      while (iter.Next(&filter)) {
        var e    = filter.Entity;
        var unit = filter.Unit;

        bool hasExitZone = f.Exists(unit->TargetExitZone);

        if (!hasExitZone) {
          if (Application.isEditor) {
            f.LogError(e, $"Unit has no exit zone!! U must setup exist zone for {unit->PlayerRef}! Farthest was set! ");
          }
          e.TryFindFarthest<ExitZone>(f, out EntityRef exitZoneRef);

          unit->TargetExitZone = exitZoneRef;
        }
      }
    }

    public override void Update(Frame f, ref Filter filter) {
      var unit = filter.Unit;

      if (unit->PlayerRef == PlayerRef.None) {
        return;
      }

      var exitZoneRef = unit->TargetExitZone;

      if (!exitZoneRef.IsValid) {
        return;
      }

      var e          = filter.Entity;
      var pathfinder = filter.Pathfinder;

      bool isUpdateTime = f.Number % schedulePeriod == e.Index % schedulePeriod;
      if (!isUpdateTime) {
        var point = pathfinder->GetWaypoint(f, pathfinder->WaypointIndex);
        // DebugDrawHelper.DrawLine(f, filter.Transform->Position, point, ColorRGBA.Magenta, FP._0_50);
        return;
      }

      var moveSpeed = unit->CurrentStats.moveSpeed.AsFP;
      pathfinder->WaypointDetectionDistanceSqr = moveSpeed * moveSpeed;

      var exitZone = f.GetPointer<Transform3D>(exitZoneRef);
      var navMesh  = f.Map.GetNavMesh(AIConstants.NAV_MESH_NAME);

      if (Application.isEditor && !navMesh) {
        f.LogError(EntityRef.None, "NavMesh is not found!");
        return;
      }

      pathfinder->SetTarget(f, exitZone->Position, navMesh);
      pathfinder->ForceRepath(f);
    }
  }
}