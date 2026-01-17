using InfimaGames.LowPolyShooterPack;
using Sirenix.OdinInspector;
using UnityEngine;

public class CharacterFootPoseCorrection : MonoBehaviour {
    [SerializeField, Required] private CharacterBehaviour characterBehaviour;

    [SerializeField, Range(0, 1f)] private float footCorrectionWeight = 1f;

    [SerializeField]
    private float stepInnerRadius = 0.1f;

    [SerializeField]
    private float stepOuterRadius = 0.3f;

    [SerializeField]
    private AnimationCurve stepHeightCurve = AnimationCurve.Constant(0, 1, 0);

    [SerializeField]
    private float stepHeight = 0.1f;

    [SerializeField, MinValue(0.01f)]
    private float stepDuration = 0.1f;

    private FootCorrectionData leftFoot, rightFoot;

    private void Start() {
        var animator = this.characterBehaviour.GetCharacterAnimator();

        this.leftFoot = new FootCorrectionData {
            AvatarIKGoal = AvatarIKGoal.LeftFoot,
            FootBone     = animator.GetBoneTransform(HumanBodyBones.LeftFoot),
        };
        this.rightFoot = new FootCorrectionData {
            AvatarIKGoal = AvatarIKGoal.RightFoot,
            FootBone     = animator.GetBoneTransform(HumanBodyBones.RightFoot),
        };
    }

    private void OnAnimatorIK(int layerIndex) {
        if (layerIndex != 0) {
            return;
        }

        var animator = this.characterBehaviour.GetCharacterAnimator();
        var weight   = this.footCorrectionWeight * animator.GetFloat(AHashes.FootIk);

        if (this.characterBehaviour.GetFullBodyState() != CharacterFullBodyStates.Default) {
            weight = 0f;
        }

        this.PerformFootCorrection(animator, ref this.leftFoot, weight, canStep: this.rightFoot.StepProgress01 == null);
        this.PerformFootCorrection(animator, ref this.rightFoot, weight, canStep: this.leftFoot.StepProgress01 == null);
    }

    private void PerformFootCorrection(Animator animator, ref FootCorrectionData data, float weight, bool canStep) {
        var targetFootPos = data.FootBone.position;
        var targetFootRot = animator.GetIKRotation(data.AvatarIKGoal);

        // smoothly reset step animation when disabled
        data.StablePosition = Vector3.Lerp(targetFootPos, data.StablePosition, weight);
        data.StableRotation = Quaternion.Slerp(targetFootRot, data.StableRotation, weight);

        // Initialize
        {
            if (!data.Initialized) {
                data.Initialized    = true;
                data.StablePosition = targetFootPos;
                data.StableRotation = targetFootRot;
            }
        }

        // Start step animation if required and possible or if correction is disabled by weight
        {
            if (data.StepProgress01 == null) {
                var dist = Vector3.Distance(this.transform.position, data.StablePosition);
                var wantStep = dist < this.stepInnerRadius ||
                               dist > this.stepOuterRadius ||
                               Vector3.Distance(targetFootPos, data.StablePosition) > 0.1f;

                if (canStep && wantStep) {
                    data.StepProgress01     = 0f;
                    data.StepPositionOffset = data.StablePosition - targetFootPos;
                    data.StepRotationOffset = Quaternion.Inverse(targetFootRot) * data.StableRotation;
                }
            }
        }

        // Play step animation
        {
            if (data.StepProgress01 is { } stepProgress) {
                var lerp   = stepProgress + Time.deltaTime / this.stepDuration;
                var height = Vector3.up * this.stepHeightCurve.Evaluate(lerp) * this.stepHeight;

                var posOffset = Vector3.Lerp(data.StepPositionOffset, Vector3.zero, lerp) + height;
                var rotOffset = Quaternion.Slerp(data.StepRotationOffset, Quaternion.identity, lerp);

                animator.SetIKPosition(data.AvatarIKGoal, targetFootPos + posOffset);
                animator.SetIKRotation(data.AvatarIKGoal, targetFootRot * rotOffset);

                data.StepProgress01 = lerp;

                if (lerp > 1f) {
                    data.StepProgress01 = null;
                    data.StablePosition = targetFootPos;
                    data.StableRotation = targetFootRot;
                }
            }
            else {
                animator.SetIKPosition(data.AvatarIKGoal, data.StablePosition);
                animator.SetIKRotation(data.AvatarIKGoal, data.StableRotation);
            }
        }

        animator.SetIKPositionWeight(data.AvatarIKGoal, weight);
        animator.SetIKRotationWeight(data.AvatarIKGoal, weight);
    }

    private struct FootCorrectionData {
        public Transform    FootBone;
        public AvatarIKGoal AvatarIKGoal;
        public bool         Initialized;

        public Vector3    StablePosition;
        public Quaternion StableRotation;

        public float?     StepProgress01;
        public Vector3    StepPositionOffset;
        public Quaternion StepRotationOffset;
    }
}