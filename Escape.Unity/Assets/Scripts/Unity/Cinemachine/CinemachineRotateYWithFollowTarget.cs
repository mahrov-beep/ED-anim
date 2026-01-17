using UnityEngine;

namespace Unity.Cinemachine {
    [AddComponentMenu("Cinemachine/Procedural/Rotation Control/Cinemachine Rotate Y With Follow Target")]
    [SaveDuringPlay]
    [DisallowMultipleComponent]
    [CameraPipeline(CinemachineCore.Stage.Aim)]
    [RequiredTarget(RequiredTargetAttribute.RequiredTargets.Tracking)]
    public class CinemachineRotateYWithFollowTarget : CinemachineComponentBase {
        public override bool IsValid => this.enabled && this.FollowTarget != null;

        public override CinemachineCore.Stage Stage => CinemachineCore.Stage.Aim;

        public override void MutateCameraState(ref CameraState curState, float deltaTime) {
            if (!this.IsValid) {
                return;
            }

            var cameraAngles = curState.RawOrientation.eulerAngles;
            var targetAngles = this.FollowTargetRotation.eulerAngles;

            cameraAngles.y = targetAngles.y;

            curState.RawOrientation = Quaternion.Euler(cameraAngles);
            //curState.ReferenceUp    = curState.RawOrientation * Vector3.up;
        }
    }
}