using Photon.Deterministic;

namespace Quantum {
  using System.Diagnostics;
  using Core;
  public static unsafe class NavMeshHelper {

    [Conditional("DEBUG")]
    public static void ValidateNavMesh(FrameThreadSafe f, string navMeshName) {
      NavMesh navMesh = f.Map.GetNavMesh(navMeshName);
      if (!navMesh) {
        ((FrameBase)f).LogError(EntityRef.None,
                $"Can't find {navMeshName}." +
                $"если вы находитесь тут:\n" +
                $"  - проверить что в MapAsset в NavMeshLinks появляется ссылка на нав меш после Bake All\n" +
                $"  Если ссылки там нет:\n" +
                $"  - убедиться что объекты покрытые NavMeshSurface не вылезают за WorldSize указанный в MapAsset");
      }
    }    
    
    [Conditional("DEBUG")]
    public static void ValidateNavMesh(FrameBase f, string navMeshName) {
      NavMesh navMesh = f.Map.GetNavMesh(navMeshName);
      if (!navMesh) {
        f.LogError(EntityRef.None,
                $"Can't find {navMeshName}." +
                $"если вы находитесь тут:\n" +
                $"  - проверить что в MapAsset в NavMeshLinks появляется ссылка на нав меш после Bake All\n" +
                $"  Если ссылки там нет:\n" +
                $"  - убедиться что объекты покрытые NavMeshSurface не вылезают за WorldSize указанный в MapAsset");
      }
    }

    public static FPVector3 FindNearestPointOnNavMesh(FrameBase f, FPVector3 origin) {
      var  navMesh = f.Map.GetNavMesh(AIConstants.NAV_MESH_NAME);
      bool v       = TryFindNearestPointOnNavMesh(navMesh, origin, out FPVector3 point);

      return point;
    }

    public static bool TryFindNearestPointOnNavMesh(
            FrameBase f,
            FPVector3 origin,
            out FPVector3 point,
            int radiusLimit = 3) {

      var navMesh = f.Map.GetNavMesh(AIConstants.NAV_MESH_NAME);

      return TryFindNearestPointOnNavMesh(navMesh, origin, out point, radiusLimit);
    }

    public static bool TryFindNearestPointOnNavMesh(
            FrameThreadSafe f,
            FPVector3 origin,
            out FPVector3 point,
            int radiusLimit = 3) {

      var navMesh = f.Map.GetNavMesh(AIConstants.NAV_MESH_NAME);

      return TryFindNearestPointOnNavMesh(navMesh, origin, out point, radiusLimit);
    }

    public static bool TryFindNearestPointOnNavMesh(
            NavMesh navMesh,
            FPVector3 origin,
            out FPVector3 point,
            int radiusLimit = 3) {

      var radius = FP._0_50;

      var regionMask = NavMeshRegionMask.Default;

      point = origin;

      while (radius < radiusLimit) {
        bool successFind = navMesh.FindClosestTriangle(origin, radius, regionMask,
                out int triangle, 
                out FPVector3 closest);

        if (successFind) {
          point = closest;
          return true;
        }
        radius += FP._0_50;
      }

      return false;
    }

    public static bool Contains(FrameBase f, FPVector3 point) {
      var navMesh    = f.Map.GetNavMesh(AIConstants.NAV_MESH_NAME);
      var regionMask = NavMeshRegionMask.Default;

      return navMesh.Contains(point, regionMask);
    }
  }
}