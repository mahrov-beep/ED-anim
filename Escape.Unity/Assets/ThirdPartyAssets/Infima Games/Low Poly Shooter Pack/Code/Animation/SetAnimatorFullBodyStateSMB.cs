namespace InfimaGames.LowPolyShooterPack {
    using UnityEngine;

    public class SetAnimatorFullBodyStateSMB : SetAnimatorPropertyBaseSMB<CharacterFullBodyStates> {
        protected override AnimatorControllerParameterType ParameterType => AnimatorControllerParameterType.Int;

        protected override void SetProperty(Animator animator, string propertyName, CharacterFullBodyStates value) {
            animator.SetInteger(propertyName, (int)value);
        }
    }
}