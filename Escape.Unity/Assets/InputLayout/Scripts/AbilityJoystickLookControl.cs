namespace InputLayout.Scripts {
    using UnityEngine;
    using UnityEngine.InputSystem.Layouts;
    using UnityEngine.InputSystem.OnScreen;
  
    public sealed class AbilityJoystickLookControl : OnScreenControl {
        [InputControl(layout = "Vector2")]
        [SerializeField]
        private string _controlPath = "<OnScreenMouse>/delta";

        protected override string controlPathInternal {
            get => _controlPath;
            set => _controlPath = value;
        }

        public void SetDelta(Vector2 delta) {
            SendValueToControl(delta);
        }

        public void ResetDelta() {
            SendValueToControl(Vector2.zero);
        }

        protected override void OnDisable() {
            ResetDelta();
            base.OnDisable();
        }
    }
}
