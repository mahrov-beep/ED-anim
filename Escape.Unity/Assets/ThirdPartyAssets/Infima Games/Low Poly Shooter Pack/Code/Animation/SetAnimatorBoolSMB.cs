namespace InfimaGames.LowPolyShooterPack {
    using UnityEngine;

    public class SetAnimatorBoolSMB : SetAnimatorPropertyBaseSMB<bool> {
        protected override AnimatorControllerParameterType ParameterType => AnimatorControllerParameterType.Bool;

        protected override void SetProperty(Animator animator, string propertyName, bool value) {
            animator.SetBool(propertyName, value);
        }
    }
}