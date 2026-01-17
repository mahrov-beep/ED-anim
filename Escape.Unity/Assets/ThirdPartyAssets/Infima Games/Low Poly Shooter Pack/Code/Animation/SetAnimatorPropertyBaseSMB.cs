namespace InfimaGames.LowPolyShooterPack {
    using System.Collections.Generic;
    using System.Linq;
    using Sirenix.OdinInspector;
    using UnityEngine;

    public abstract class SetAnimatorPropertyBaseSMB<TValue> : StateMachineBehaviour {
        [SerializeField]
        [HorizontalGroup("T", Width = 70), LabelWidth(40), PropertyOrder(1)]
        private int loops;
        
        [SerializeField, Range(0, 1)]
        [HorizontalGroup("T"), PropertyOrder(0)]
        private float timeNormalized = 1;

        [SerializeField]
        [BoxGroup("Prop", false), HorizontalGroup("Prop/Line"), PropertyOrder(-10), HideLabel]
#if UNITY_EDITOR
        [ValueDropdown(nameof(EnumerateAnimatorProperties))]
        [ValidateInput(nameof(ValidateAnimatorProperty))]
#endif
        private string property;

        [SerializeField]
        [HorizontalGroup("Prop/Line"), PropertyOrder(-9)]
        private TValue value;

        private bool wasSet;

        protected abstract AnimatorControllerParameterType ParameterType { get; }

        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
            base.OnStateEnter(animator, stateInfo, layerIndex);

            this.wasSet = false;
        }

        public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
            base.OnStateUpdate(animator, stateInfo, layerIndex);

            if (stateInfo.normalizedTime < this.loops) {
                return;
            }

            if (stateInfo.normalizedTime - this.loops >= this.timeNormalized) {
                this.SetIfNeeded(animator);
            }
        }

        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
            base.OnStateExit(animator, stateInfo, layerIndex);

            this.SetIfNeeded(animator);
        }

        private void SetIfNeeded(Animator animator) {
            if (this.wasSet) {
                return;
            }

            this.wasSet = true;
            this.SetProperty(animator, this.property, this.value);
        }

        protected abstract void SetProperty(Animator animator, string propertyName, TValue value);

#if UNITY_EDITOR

        private bool ValidateAnimatorProperty(string propertyName, ref string error) {
            if (Application.isPlaying) {
                return true;
            }
            
            var path     = UnityEditor.AssetDatabase.GetAssetPath(this);
            var animator = UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEditor.Animations.AnimatorController>(path);
            foreach (var parameter in animator.parameters) {
                if (parameter.name == propertyName) {
                    if (parameter.type != this.ParameterType) {
                        error = $"Animator property '{propertyName}' type mismatch on {animator.name}: expected={this.ParameterType}, actual={parameter.type}";
                        return false;
                    }

                    return true;
                }
            }

            error = $"Animator property '{propertyName}' does not exist on {animator.name}";
            return false;
        }

        private IEnumerable<ValueDropdownItem<string>> EnumerateAnimatorProperties() {
            var path     = UnityEditor.AssetDatabase.GetAssetPath(this);
            var animator = UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEditor.Animations.AnimatorController>(path);
            return animator.parameters
                .Where(p => p.type == this.ParameterType)
                .Select(p => new ValueDropdownItem<string>(p.name, p.name));
        }
#endif
    }
}