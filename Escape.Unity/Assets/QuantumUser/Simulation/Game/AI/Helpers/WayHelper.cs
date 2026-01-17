namespace Quantum {
  using Core;
  using Photon.Deterministic;

  public static unsafe class WayHelper {
    public static FPVector3 GetWaypoint(FrameBase f, Way* way, int index) {
      var points = f.ResolveList(way->Points);
      return points[index % points.Count];
    }

    public static int GetNextIndex(FrameBase f, Way* way, int currentIndex) {
      var points = f.ResolveList(way->Points);
      return (currentIndex + 1) % points.Count;
    }

    public static bool IsNear(FPVector3 position, FPVector3 waypoint, FP threshold) {
      return FPVector3.DistanceSquared(position, waypoint) <= threshold * threshold;
    }
  }
}
