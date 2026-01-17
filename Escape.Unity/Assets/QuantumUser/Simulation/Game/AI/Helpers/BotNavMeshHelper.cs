namespace Quantum {
  using Core;
  using Photon.Deterministic;

  public static unsafe class BotNavMeshHelper {
    public static void SetTarget(FrameBase f, NavMeshPathfinder* pathfinder, FPVector3 target) {
      var navMesh = f.Map.GetNavMesh(AIConstants.NAV_MESH_NAME);
      pathfinder->SetTarget(f, target, navMesh);
    }

    public static bool IsMoving(NavMeshPathfinder* pathfinder) {
      return pathfinder->IsActive;
    }

    public static void Stop(FrameBase f, EntityRef entity, NavMeshPathfinder* pathfinder, bool resetVelocity = true) {
      pathfinder->Stop(f, entity, resetVelocity);
    }

    public static FPVector3 GetRandomPointAround(ref RNGSession rng, FPVector3 center, FP radius) {
      var angle = rng.Next() * FP.PiTimes2;
      var dist  = rng.Next() * radius;
      return center + new FPVector3(FPMath.Cos(angle) * dist, 0, FPMath.Sin(angle) * dist);
    }

    public static FPVector3 GetRandomPointAroundOnNavMesh(FrameThreadSafe f, ref RNGSession rng, FPVector3 center, FP radius) {
      var randomPoint = GetRandomPointAround(ref rng, center, radius);

      if (NavMeshHelper.TryFindNearestPointOnNavMesh(f, randomPoint, out var navMeshPoint)) {
        return navMeshPoint;
      }

      return center;
    }
  }
}
