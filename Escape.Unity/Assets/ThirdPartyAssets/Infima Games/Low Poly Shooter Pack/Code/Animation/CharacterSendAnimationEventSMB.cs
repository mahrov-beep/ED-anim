namespace InfimaGames.LowPolyShooterPack {
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Sirenix.OdinInspector;
    using UnityEngine;

    public class CharacterSendAnimationEventSMB : StateMachineBehaviour {
        [SerializeField]
        [HorizontalGroup("T", Width = 70), LabelWidth(40), PropertyOrder(1)]
        private int loops;

        [SerializeField, Range(0, 1)]
        [HorizontalGroup("T"), PropertyOrder(0)]
        private float sendTimeNormalized = 1;

        [SerializeField, Title("Method Name"), HideLabel]
        [ValueDropdown(nameof(EnumerateMethodNames))]
        [ValidateInput(nameof(ValidateMethodName))]
        private string methodName;

        private bool wasSent;

        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
            base.OnStateEnter(animator, stateInfo, layerIndex);

            this.wasSent = false;
        }

        public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
            base.OnStateUpdate(animator, stateInfo, layerIndex);

            if (stateInfo.normalizedTime < this.loops) {
                return;
            }

            if (stateInfo.normalizedTime - this.loops >= this.sendTimeNormalized) {
                this.SendIfNeeded(animator);
            }
        }

        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
            this.SendIfNeeded(animator);

            base.OnStateExit(animator, stateInfo, layerIndex);
        }

        private void SendIfNeeded(Animator animator) {
            if (this.wasSent) {
                return;
            }

            this.wasSent = true;

            animator.gameObject.SendMessage(this.methodName, SendMessageOptions.RequireReceiver);
        }

        private bool ValidateMethodName(string method, ref string error) {
            if (this.EnumerateMethodNames().All(it => it.Value != method)) {
                error = $"Method with name '{method}' does not exist in {nameof(CharacterAnimationEventHandler)} component";
                return false;
            }

            return true;
        }

        private IEnumerable<ValueDropdownItem<string>> EnumerateMethodNames() {
            return typeof(CharacterAnimationEventHandler).GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                .Where(method => typeof(CharacterAnimationEventHandler).IsAssignableFrom(method.DeclaringType))
                .Where(method => method.GetParameters().Length == 0 && !method.IsSpecialName)
                .Select(method => new ValueDropdownItem<string>($"{method.DeclaringType!.Name}.{method.Name}", method.Name));
        }
    }
}