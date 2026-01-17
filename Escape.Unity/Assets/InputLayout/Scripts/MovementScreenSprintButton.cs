namespace InputLayout.Scripts {
    using UnityEngine;
    using UnityEngine.InputSystem.Layouts;
    using UnityEngine.InputSystem.OnScreen;

    public class MovementScreenSprintButton : OnScreenControl {
        [SerializeField, InputControl(layout = "Button")]
        private string _controlPath;

        protected override string controlPathInternal {
            get => _controlPath;
            set => _controlPath = value;
        }

        public void SetSprintActive(bool active) {
            this.SendValueToControl(active ? 1f : 0f);
        }
    }
}