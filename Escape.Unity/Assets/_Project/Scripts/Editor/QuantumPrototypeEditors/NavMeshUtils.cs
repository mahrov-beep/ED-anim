#if UNITY_EDITOR

using UnityEngine;
using UnityEngine.AI;
using NavMesh = UnityEngine.AI.NavMesh;

public static class NavMeshUtils {
    public static bool FindClosestNavMeshPoint(Vector3 position, float radius, out Vector3 closest) {
        if (NavMesh.SamplePosition(position, out var hit, radius, NavMesh.AllAreas)) {
            closest = hit.position;
            return true;
        }
        closest = position;
        return false;
    }

    public static Vector3 SnapToNavMesh(Vector3 position, float radius) {
        return FindClosestNavMeshPoint(position, radius, out var closest) ? closest : position;
    }

    public static bool IsPointReachable(Vector3 position, float maxDistance) {
        if (NavMesh.SamplePosition(position, out var hit, maxDistance, NavMesh.AllAreas)) {
            if (Vector3.Distance(position, hit.position) <= maxDistance) {
                if (!NavMesh.Raycast(position, hit.position, out _, NavMesh.AllAreas)) {
                    return true;
                }
            }
        }
        return false;
    }

    public static bool IsReachableBetween(Vector3 from, Vector3 to, float maxDistance) {
        if (!NavMesh.SamplePosition(from, out var hitFrom, maxDistance, NavMesh.AllAreas)) return false;
        if (!NavMesh.SamplePosition(to, out var hitTo, maxDistance, NavMesh.AllAreas)) return false;
        var path = new NavMeshPath();
        NavMesh.CalculatePath(hitFrom.position, hitTo.position, NavMesh.AllAreas, path);
        return path.status == NavMeshPathStatus.PathComplete;
    }

    public static float GetDistanceToNavMeshBoundary(Vector3 position) {
        return NavMesh.FindClosestEdge(position, out var hit, NavMesh.AllAreas) ? hit.distance : 0f;
    }
}
#endif