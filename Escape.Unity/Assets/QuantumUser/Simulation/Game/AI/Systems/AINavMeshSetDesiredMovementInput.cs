namespace Quantum {
  using Photon.Deterministic;
  using UnityEngine;
  using UnityEngine.Scripting;
  using static Navigation;

  [Preserve]
  public unsafe class AINavMeshSetDesiredMovementInput : SystemSignalsOnly,
          ISignalOnNavMeshMoveAgent,
          ISignalOnNavMeshWaypointReached,
          ISignalOnNavMeshSearchFailed {

    public void OnNavMeshMoveAgent(Frame f, EntityRef e, FPVector2 desiredDirection) {
      if (!f.Has<Bot>(e)) return;

      var inputContainer = f.GetPointer<InputContainer>(e);

      inputContainer->DesiredDirection = desiredDirection;
    }

    public void OnNavMeshWaypointReached(Frame f, EntityRef e, FPVector3 waypoint, WaypointFlag flags, ref bool resetAgent) {
      // if ((flags & WaypointFlag.Target) != 0) {
      //   f.LogWarning(e, $"NavMesh Waypoint Reached {waypoint}");
      // }
    }

    public void OnNavMeshSearchFailed(Frame f, EntityRef e, ref bool resetAgent) {
      if (!Application.isEditor || !f.IsVerified) {
        return;
      }

      // var agentTransform = f.GetPointer<Transform3D>(e);
      // var closestPoint   = NavMeshHelper.FindNearestPointOnNavMesh(f, agentTransform->Position);
      // f.LogDebug(e,
      //         $"OnNavMeshSearchFailed, agentPosition={agentTransform->Position}, " +
      //         $"closestNMPoint={closestPoint}, " +
      //         $"distance = {FPVector3.Distance(agentTransform->Position, closestPoint)}");
      if (!f.TryGetPointers(e, out Transform3D* transform, out Unit* unit, out Bot* bot)) {
        return;
      }

      bool isLocal = f.Context.IsLocalPlayer(unit->PlayerRef);

      if (!NavMeshHelper.Contains(f, transform->Position)) {
        if (isLocal) {
          f.LogError(e, $"NavMeshSearchFailed for local player. Start path from incorrect position {transform->Position}.");
        }
        else {
          f.LogError(e, $"NavMeshSearchFailed. Start path from incorrect position. {transform->Position}");
        }
      }
      else {
        f.LogError(e, "NavMeshSearchFailed. Target position is invalid.");
      }
    }
  }
}