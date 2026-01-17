using System;
using UnityEngine;

// ReSharper disable InconsistentNaming

namespace Unity.Cinemachine {
    /// <summary>
    /// Third-person follower, with complex pivoting: horizontal about the origin, 
    /// vertical about the shoulder.  
    /// </summary>
    [SaveDuringPlay]
    [DisallowMultipleComponent]
    [CameraPipeline(CinemachineCore.Stage.Body)]
    public class CinemachineThirdPersonFollowQuantum : CinemachineComponentBase
        , CinemachineFreeLookModifier.IModifierValueSource
        , CinemachineFreeLookModifier.IModifiablePositionDamping
        , CinemachineFreeLookModifier.IModifiableDistance {
        /// <summary>How responsively the camera tracks the target.  Each axis (camera-local) 
        /// can have its own setting.  Value is the approximate time it takes the camera 
        /// to catch up to the target's new position.  Smaller values give a more rigid 
        /// effect, larger values give a squishier one.</summary>
        [Tooltip("How responsively the camera tracks the target.  Each axis (camera-local) "
                 + "can have its own setting.  Value is the approximate time it takes the camera "
                 + "to catch up to the target's new position.  Smaller values give a more "
                 + "rigid effect, larger values give a squishier one")]
        public Vector3 Damping;

        /// <summary>Position of the shoulder pivot relative to the Follow target origin.  
        /// This offset is in target-local space.</summary>
        [Header("Rig")]
        [Tooltip("Position of the shoulder pivot relative to the Follow target origin.  "
                 + "This offset is in target-local space")]
        public Vector3 ShoulderOffset;

        /// <summary>Vertical offset of the hand in relation to the shoulder.  
        /// Arm length will affect the follow target's screen position 
        /// when the camera rotates vertically.</summary>
        [Tooltip("Vertical offset of the hand in relation to the shoulder.  "
                 + "Arm length will affect the follow target's screen position when "
                 + "the camera rotates vertically")]
        public float VerticalArmLength;

        /// <summary>How far behind the hand the camera will be placed.</summary>
        [Tooltip("How far behind the hand the camera will be placed")]
        public float CameraDistance;

        /// <summary>
        /// Holds settings for collision resolution.
        /// </summary>
        [Serializable]
        public struct ObstacleSettings {
            /// <summary>Camera will avoid obstacles on these layers.</summary>
            [Tooltip("Camera will avoid obstacles on these layers")]
            public LayerMask CollisionFilter;

            /// <summary>
            /// Obstacles with this tag will be ignored.  It is a good idea 
            /// to set this field to the target's tag
            /// </summary>
            [TagField]
            [Tooltip("Obstacles with this tag will be ignored.  "
                     + "It is a good idea to set this field to the target's tag")]
            public string IgnoreTag;

            /// <summary>
            /// Specifies how close the camera can get to obstacles
            /// </summary>
            [Tooltip("Specifies how close the camera can get to obstacles")]
            [Range(0, 1)]
            public float CameraRadius;

            /// <summary>
            /// How gradually the camera moves to correct for occlusions.  
            /// Higher numbers will move the camera more gradually.
            /// </summary>
            [Range(0, 10)]
            [Tooltip("How gradually the camera moves to correct for occlusions.  " +
                     "Higher numbers will move the camera more gradually.")]
            public float DampingIntoCollision;

            /// <summary>
            /// How gradually the camera returns to its normal position after having been corrected by the built-in
            /// collision resolution system. Higher numbers will move the camera more gradually back to normal.
            /// </summary>
            [Range(0, 10)]
            [Tooltip("How gradually the camera returns to its normal position after having been corrected by the built-in " +
                     "collision resolution system.  Higher numbers will move the camera more gradually back to normal.")]
            public float DampingFromCollision;

            internal static ObstacleSettings Default => new() {
                CollisionFilter      = 1,
                IgnoreTag            = string.Empty,
                CameraRadius         = 0.2f,
                DampingIntoCollision = 0,
                DampingFromCollision = 0.5f,
            };
        }

        /// <summary>If enabled, camera will be pulled in front of occluding obstacles.</summary>
        public ObstacleSettings AvoidObstacles = ObstacleSettings.Default;

        // State info
        private Vector3 m_PreviousFollowTargetPosition;
        private Vector3 m_DampingCorrection; // this is in local rig space
        private float   m_CamPosCollisionCorrection;

        private void OnValidate() {
            this.Damping.x                           = Mathf.Max(0, this.Damping.x);
            this.Damping.y                           = Mathf.Max(0, this.Damping.y);
            this.Damping.z                           = Mathf.Max(0, this.Damping.z);
            this.AvoidObstacles.CameraRadius         = Mathf.Max(0.001f, this.AvoidObstacles.CameraRadius);
            this.AvoidObstacles.DampingIntoCollision = Mathf.Max(0, this.AvoidObstacles.DampingIntoCollision);
            this.AvoidObstacles.DampingFromCollision = Mathf.Max(0, this.AvoidObstacles.DampingFromCollision);
        }

        private void Reset() {
            this.ShoulderOffset    = new Vector3(0.5f, -0.4f, 0.0f);
            this.VerticalArmLength = 0.4f;
            this.CameraDistance    = 2.0f;
            this.Damping           = new Vector3(0.1f, 0.5f, 0.3f);
            this.AvoidObstacles    = ObstacleSettings.Default;
        }

        float CinemachineFreeLookModifier.IModifierValueSource.NormalizedModifierValue {
            get {
                var up  = this.VirtualCamera.State.ReferenceUp;
                var rot = this.FollowTargetRotation;
                var a   = Vector3.SignedAngle(rot * Vector3.up, up, rot * Vector3.right);
                return Mathf.Clamp(a, -90, 90) / -90;
            }
        }

        Vector3 CinemachineFreeLookModifier.IModifiablePositionDamping.PositionDamping {
            get => this.Damping;
            set => this.Damping = value;
        }

        float CinemachineFreeLookModifier.IModifiableDistance.Distance {
            get => this.CameraDistance;
            set => this.CameraDistance = value;
        }

        /// <summary>True if component is enabled and has a Follow target defined</summary>
        public override bool IsValid => this.enabled && this.FollowTarget != null;

        /// <summary>Get the Cinemachine Pipeline stage that this component implements.
        /// Always returns the Aim stage</summary>
        public override CinemachineCore.Stage Stage {
            get => CinemachineCore.Stage.Body;
        }

        /// <summary>
        /// Report maximum damping time needed for this component.
        /// </summary>
        /// <returns>Highest damping setting in this component</returns>
        public override float GetMaxDampTime() {
            return Mathf.Max(
                Mathf.Max(this.AvoidObstacles.DampingIntoCollision, this.AvoidObstacles.DampingFromCollision),
                Mathf.Max(this.Damping.x, Mathf.Max(this.Damping.y, this.Damping.z)));
        }

        /// <summary>Orients the camera to match the Follow target's orientation</summary>
        /// <param name="curState">The current camera state</param>
        /// <param name="deltaTime">Elapsed time since last frame, for damping calculations.  
        /// If negative, previous state is reset.</param>
        public override void MutateCameraState(ref CameraState curState, float deltaTime) {
            if (this.IsValid) {
                if (!this.VirtualCamera.PreviousStateIsValid) {
                    deltaTime = -1;
                }

                this.PositionCamera(ref curState, deltaTime);
            }
        }

        /// <summary>This is called to notify the user that a target got warped,
        /// so that we can update its internal state to make the camera
        /// also warp seamlessly.</summary>
        /// <param name="target">The object that was warped</param>
        /// <param name="positionDelta">The amount the target's position changed</param>
        public override void OnTargetObjectWarped(Transform target, Vector3 positionDelta) {
            base.OnTargetObjectWarped(target, positionDelta);
            if (target == this.FollowTarget) {
                this.m_PreviousFollowTargetPosition += positionDelta;
            }
        }

        private float preferredSide    = 1f;
        private float preferredSideVel = 0f;

        private void PositionCamera(ref CameraState curState, float deltaTime) {
            var up            = curState.ReferenceUp;
            var targetPos     = this.FollowTargetPosition;
            var targetRot     = this.FollowTargetRotation;
            var targetForward = targetRot * Vector3.forward;
            var heading       = GetHeading(targetRot, up);

            if (deltaTime < 0) {
                // No damping - reset damping state info
                this.m_DampingCorrection         = Vector3.zero;
                this.m_CamPosCollisionCorrection = 0;
            }
            else {
                // Damping correction is applied to the shoulder offset - stretching the rig
                this.m_DampingCorrection += Quaternion.Inverse(heading) * (this.m_PreviousFollowTargetPosition - targetPos);
                this.m_DampingCorrection -= this.VirtualCamera.DetachedFollowTargetDamp(this.m_DampingCorrection, this.Damping, deltaTime);
            }

            this.m_PreviousFollowTargetPosition = targetPos;
            var root = targetPos;

            this.GetRawRigPositions(root, targetRot, heading, 0.0f, out _, out Vector3 handLeft);
            this.GetRawRigPositions(root, targetRot, heading, this.preferredSide, out _, out Vector3 handRight);

            // Place the camera at the correct distance from the hand
            var camPosLeft  = handLeft - (targetForward * (this.CameraDistance - this.m_DampingCorrection.z));
            var camPosRight = handRight - (targetForward * (this.CameraDistance - this.m_DampingCorrection.z));

            // Check if hand is colliding with something, if yes, then move the hand 
            // closer to the player. The radius is slightly enlarged, to avoid problems 
            // next to walls

            var collidedHandLeft  = this.ResolveCollisions(root, handLeft, -1, this.AvoidObstacles.CameraRadius * 1.075f, 0f, out _);
            var collidedHandRight = this.ResolveCollisions(root, handRight, -1, this.AvoidObstacles.CameraRadius * 1.075f, 0f, out _);

            this.ResolveCollisions(collidedHandLeft, camPosLeft, -1, this.AvoidObstacles.CameraRadius * 1.033f, 0f, out var leftCorrection);
            this.ResolveCollisions(collidedHandRight, camPosRight, -1, this.AvoidObstacles.CameraRadius * 1.033f, 0f, out var rightCorrection);

            if (deltaTime > 0) {
                this.preferredSide = Mathf.SmoothDamp(this.preferredSide, 
                    rightCorrection - 0.05f > leftCorrection ? 0 : 1,
                    ref this.preferredSideVel, 1.0f, 10f, deltaTime);
            }

            var camPos = this.ResolveCollisions(collidedHandRight, camPosRight, deltaTime, this.AvoidObstacles.CameraRadius,
                this.m_CamPosCollisionCorrection, out this.m_CamPosCollisionCorrection);

            // Set state
            curState.RawPosition    = camPos;
            curState.RawOrientation = targetRot; // not necessary, but left in to avoid breaking scenes that depend on this

            // Correct the case where by default we're looking at the follow target
            if (!curState.HasLookAt() || curState.ReferenceLookAt.Equals(targetPos)) {
                curState.ReferenceLookAt = camPos + targetRot * new Vector3(0, 0, 3); // so that there's something
            }
        }

        internal static Quaternion GetHeading(Quaternion targetRot, Vector3 up) {
            var targetForward = targetRot * Vector3.forward;
            var planeForward  = Vector3.Cross(up, Vector3.Cross(targetForward.ProjectOntoPlane(up), up));
            if (planeForward.AlmostZero()) {
                planeForward = Vector3.Cross(targetRot * Vector3.right, up);
            }

            return Quaternion.LookRotation(planeForward, up);
        }

        private void GetRawRigPositions(
            Vector3 root, Quaternion targetRot, Quaternion heading, float cameraSide,
            out Vector3 shoulder, out Vector3 hand) {
            var shoulderOffset = this.ShoulderOffset;
            shoulderOffset.x =  Mathf.LerpUnclamped(-shoulderOffset.x, shoulderOffset.x, cameraSide);
            shoulderOffset.x += this.m_DampingCorrection.x;
            shoulderOffset.y += this.m_DampingCorrection.y;
            shoulder         =  root + heading * shoulderOffset;
            hand             =  shoulder + targetRot * new Vector3(0, this.VerticalArmLength, 0);
        }

        private Vector3 ResolveCollisions(
            Vector3 root, Vector3 tip, float deltaTime,
            float cameraRadius, float prevCollisionCorrection, out float nextCollisionCorrection) {
            nextCollisionCorrection = prevCollisionCorrection;

            if (this.AvoidObstacles.CollisionFilter.value == 0) {
                return tip;
            }

            var dir = tip - root;
            var len = dir.magnitude;
            if (len < Epsilon) {
                return tip;
            }

            dir /= len;

            var   result            = tip;
            float desiredCorrection = 0;

            if (RuntimeUtility.SphereCastIgnoreTag(
                    new Ray(root, dir), cameraRadius, out RaycastHit hitInfo,
                    len, this.AvoidObstacles.CollisionFilter, this.AvoidObstacles.IgnoreTag)) {
                var desiredResult = hitInfo.point + hitInfo.normal * cameraRadius;
                desiredCorrection = (desiredResult - tip).magnitude;
            }

            nextCollisionCorrection += deltaTime < 0
                ? desiredCorrection - prevCollisionCorrection
                : Damper.Damp(
                    desiredCorrection - prevCollisionCorrection,
                    desiredCorrection > prevCollisionCorrection ? this.AvoidObstacles.DampingIntoCollision : this.AvoidObstacles.DampingFromCollision,
                    deltaTime);

            // Apply the correction
            if (nextCollisionCorrection > Epsilon) {
                result -= dir * nextCollisionCorrection;
            }

            return result;
        }
    }
}