#define CINEMACHINE_PHYSICS

#if CINEMACHINE_PHYSICS
using UnityEngine;

namespace Unity.Cinemachine {
  [ExecuteAlways]
  [SaveDuringPlay]
  [DisallowMultipleComponent]
  public class CinemachineFirstPersonAimQuantum : CinemachineExtension {
    [Tooltip("If set, the raycast uses QueryTriggerInteraction.UseGlobal; otherwise Ignore.")]
    public bool UseGlobalTriggers = true;

    public Vector3 AimTarget { get; private set; }
    public GameObject AimTargetObject { get; private set; }

    protected override void PostPipelineStageCallback(
      CinemachineVirtualCameraBase vcam,
      CinemachineCore.Stage stage,
      ref CameraState state,
      float deltaTime) {
      if (stage != CinemachineCore.Stage.Finalize) {
        return;
      }

      var camPos = state.GetFinalPosition();
      var fwd    = state.GetFinalOrientation() * Vector3.forward;

      var triggerInteraction = this.UseGlobalTriggers
        ? QueryTriggerInteraction.UseGlobal
        : QueryTriggerInteraction.Ignore;

      if (Physics.Raycast(camPos, fwd, out var hit, Mathf.Infinity, CinemachineAimMask.AimLayerMask, triggerInteraction)) {
        this.AimTarget       = hit.point;
        this.AimTargetObject = hit.transform ? hit.transform.gameObject : null;
      }
      else {
        const float minFallbackDistance = 500f;
        const float maxFallbackDistance = 5000f;

        var fallbackDistance = state.Lens.FarClipPlane;
        if (float.IsNaN(fallbackDistance) || float.IsInfinity(fallbackDistance) || fallbackDistance <= 0f) {
          fallbackDistance = maxFallbackDistance;
        }

        fallbackDistance     = Mathf.Clamp(fallbackDistance, minFallbackDistance, maxFallbackDistance);
        this.AimTarget       = camPos + fwd * fallbackDistance;
        this.AimTargetObject = null;
      }
    }
  }
}
#endif
