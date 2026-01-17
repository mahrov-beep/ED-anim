namespace CodeWriter.ViewBinding.Applicators.Adapters {
    using Sirenix.OdinInspector;
    using UnityEngine;
    using UnityEngine.InputSystem;

    public class InputControlNameAdapter : SingleResultAdapterBase<string, ViewVariableString> {
        [Required]
        public InputActionReference InputActionReference;

        protected override string Adapt() {
            if (Application.isMobilePlatform) {
                return string.Empty;
            }

            for (var i = 0; i < this.InputActionReference.action.bindings.Count; i++) {
                var binding = this.InputActionReference.action.bindings[i];

                if (binding.effectivePath.StartsWith("<Keyboard>") ||
                    binding.effectivePath == "OneModifier") {
                    return " [" + this.InputActionReference.action.GetBindingDisplayString(i) + "] ";
                }
            }

            return string.Empty;
        }
    }
}