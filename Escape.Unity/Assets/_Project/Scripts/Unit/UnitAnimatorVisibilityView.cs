namespace _Project.Scripts.Unit {
    using Quantum;
    using Sirenix.OdinInspector;
    using UnityEngine;

    public class UnitAnimatorVisibilityView : QuantumEntityViewComponent {
        [SerializeField, Required] private Animator   animator;
        [SerializeField, Required] private GameObject animatorObject;

        public override void OnUpdateView() {
            base.OnUpdateView();

            var f              = this.VerifiedFrame;
            var isInvisibleBot = f.Has<Bot>(this.EntityRef) && f.Has<BotInvisibleByPlayer>(this.EntityRef);

            var visible = isInvisibleBot == false;

            if (this.animatorObject.activeSelf != visible) {
                this.animatorObject.SetActive(visible);
                this.animator.cullingMode = visible ? AnimatorCullingMode.CullUpdateTransforms : AnimatorCullingMode.CullCompletely;
            }
        }
    }
}