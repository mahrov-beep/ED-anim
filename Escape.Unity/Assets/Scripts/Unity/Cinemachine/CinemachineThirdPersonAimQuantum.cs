#define CINEMACHINE_PHYSICS

#if CINEMACHINE_PHYSICS
using UnityEngine;

// ReSharper disable InconsistentNaming

namespace Unity.Cinemachine {
    using System.Collections.Generic;
    using Quantum;
    using Sirenix.OdinInspector;
    using LayerMask = UnityEngine.LayerMask;

    internal static class CinemachineAimMask {
        private static LayerMask? aimLayerMaskBacking;

        public static LayerMask AimLayerMask =>
            aimLayerMaskBacking ??= LayerMask.GetMask("AimAssistTarget", "Static", "Static_Far", "Static_Near");
    }

    /// <summary>
    /// An add-on module for CinemachineCamera that forces the LookAt
    /// point to the center of the screen, based on the Follow target's orientation,
    /// cancelling noise and other corrections.
    /// This is useful for third-person style aim cameras that want a dead-accurate
    /// aim at all times, even in the presence of positional or rotational noise.
    /// </summary>
    [ExecuteAlways]
    [SaveDuringPlay]
    [DisallowMultipleComponent]
    public class CinemachineThirdPersonAimQuantum : CinemachineExtension {
        public static LayerMask AimLayerMask => CinemachineAimMask.AimLayerMask;

        [SerializeField, Required]
        public CinemachineImpulseListener ImpulseListener;

        /// <summary>How far to project the object detection ray.</summary>
        [Tooltip("How far to project the object detection ray")]
        public float AimDistance;

        private float WallCloseDistance = 2f;

        /// <summary>If set, camera noise will be adjusted to stabilize target on screen.</summary>
        [Tooltip("If set, camera noise will be adjusted to stabilize target on screen")]
        public bool NoiseCancellation = true;

        /// <summary>World space position of where the player would hit if a projectile were to 
        /// be fired from the player origin.  This may be different
        /// from state.ReferenceLookAt due to camera offset from player origin.</summary>
        public Vector3 AimTarget { get; private set; }

        public GameObject AimTargetObject { get; private set; }

        private float noiseCancellationLerp = 1f;

        private void OnValidate() {
            this.AimDistance = Mathf.Max(1, this.AimDistance);
        }

        private void Reset() {
            this.AimDistance       = 200.0f;
            this.NoiseCancellation = true;
        }

        private void Update() {
            var haveImpulse = CinemachineImpulseManager.Instance.GetImpulseAt(
                this.transform.position, this.ImpulseListener.Use2DDistance, this.ImpulseListener.ChannelMask, out _, out _);

            var maxDelta = haveImpulse ? 1 : Time.smoothDeltaTime * 1f;
            this.noiseCancellationLerp = Mathf.MoveTowards(this.noiseCancellationLerp, haveImpulse ? 0 : 1, maxDelta);
        }

        /// <summary>
        /// Sets the ReferenceLookAt to be the result of a raycast in the direction of camera forward.
        /// If an object is hit, point is placed there, else it is placed at AimDistance along the ray.
        /// </summary>
        /// <param name="vcam">The virtual camera being processed</param>
        /// <param name="stage">The current pipeline stage</param>
        /// <param name="state">The current virtual camera state</param>
        /// <param name="deltaTime">The current applicable deltaTime</param>
        protected override void PostPipelineStageCallback(
            CinemachineVirtualCameraBase vcam,
            CinemachineCore.Stage stage, ref CameraState state, float deltaTime) {
            switch (stage) {
                case CinemachineCore.Stage.Body: {
                    if (this.NoiseCancellation) {
                        // Raycast to establish what we're actually aiming at
                        var player = vcam.Follow;
                        if (player != null) {
                            state.ReferenceLookAt = this.ComputeLookAtPoint(state.GetCorrectedPosition(), player, player.forward);
                            this.AimTarget        = this.ComputeAimTarget(state.ReferenceLookAt, player, out var target);
                            this.AimTargetObject  = target;
                        }
                    }

                    break;
                }
                case CinemachineCore.Stage.Finalize: {
                    if (this.NoiseCancellation) {
                        // Stabilize the LookAt point in the center of the screen
                        var dir = state.ReferenceLookAt - state.GetFinalPosition();
                        if (dir.sqrMagnitude > 0.01f) {
                            state.RawOrientation        = Quaternion.Slerp(state.RawOrientation, Quaternion.LookRotation(dir, state.ReferenceUp), this.noiseCancellationLerp);
                            state.OrientationCorrection = Quaternion.Slerp(state.OrientationCorrection, Quaternion.identity, this.noiseCancellationLerp);
                        }
                    }
                    else {
                        // Raycast to establish what we're actually aiming at.
                        // In this case we do it without cancelling the noise.
                        var player = vcam.Follow;
                        if (player != null) {
                            state.ReferenceLookAt = this.ComputeLookAtPoint(
                                state.GetCorrectedPosition(), player, state.GetCorrectedOrientation() * Vector3.forward);
                            this.AimTarget       = this.ComputeAimTarget(state.ReferenceLookAt, player, out var target);
                            this.AimTargetObject = target;
                        }
                    }

                    break;
                }
            }
        }

        private Vector3 ComputeLookAtPoint(Vector3 camPos, Transform player, Vector3 fwd) {
            // We don't want to hit targets behind the player
            var aimDistance       = this.AimDistance;
            var playerOrientation = player.rotation;
            var playerPosLocal    = Quaternion.Inverse(playerOrientation) * (player.position - camPos);
            if (playerPosLocal.z > 0) {
                camPos      += fwd * playerPosLocal.z;
                aimDistance -= playerPosLocal.z;
            }

            aimDistance = Mathf.Max(1, aimDistance);

            if (Physics.Raycast(camPos, fwd, out var hit, aimDistance, AimLayerMask)) {
                var distanceToPoint = Vector3.Distance(camPos, hit.point);
                if (distanceToPoint > this.WallCloseDistance) {
                    return hit.point;
                }
            }

            return camPos + fwd * aimDistance;
        }

        private Vector3 ComputeAimTarget(Vector3 cameraLookAt, Transform player, out GameObject targetObj) {
            // Adjust for actual player aim target (may be different due to offset)
            var playerPos = player.position;
            var dir       = cameraLookAt - playerPos;

            if (Physics.Raycast(playerPos, dir, out var hit, dir.magnitude + 0.1f, AimLayerMask)) {
                targetObj = hit.transform.gameObject;                
                return hit.point;
            }

            targetObj = null;
            return cameraLookAt;
        }
    }
}
#endif