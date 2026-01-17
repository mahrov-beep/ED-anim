namespace InputLayout.Scripts {
    using Game.ECS.Systems.Player;
    using Game.Services.Photon;
    using Multicast;
    using Quantum;
    using Sirenix.OdinInspector;
    using UnityEngine;
    using UnityEngine.EventSystems;
    using UnityEngine.UI;

    public sealed class JumpButton : ScreenButton {
        [Header("UI")]
        [SerializeField, Required] private Image background;
        [SerializeField, Required] private Image icon;

        [Header("Colors")]
        [SerializeField] private Color availableColor = Color.white;
        [SerializeField] private Color jumpingColor   = Color.yellow;

        private PhotonService     photonService;
        private LocalPlayerSystem localPlayerSystem;
        private bool              lastJumpState;
        private bool              initializedState;

        protected override void OnDisable() {
            SendValueToControl(0f);
            initializedState = false;
        }

        private void Start() {
            photonService     = App.Get<PhotonService>();
            localPlayerSystem = App.Get<LocalPlayerSystem>();

            UpdateVisualState(force: true);
        }

        private void Update() {
            UpdateVisualState();
        }

        private unsafe void UpdateVisualState(bool force = false) {
            var isJumping = EvaluateJumpState();

            if (!force && initializedState && isJumping == lastJumpState) {
                return;
            }

            initializedState = true;
            lastJumpState    = isJumping;

            var targetColor = isJumping ? jumpingColor : availableColor;

            ApplyColor(background, targetColor);
            ApplyColor(icon, targetColor);
        }

        private unsafe bool EvaluateJumpState() {
            if (photonService == null || localPlayerSystem == null) {
                return false;
            }

            if (!photonService.TryGetPredicted(out var frame)) {
                return false;
            }

            if (localPlayerSystem.HasNotLocalEntityRef(out var localRef)) {
                return false;
            }

            if (!frame.TryGetPointer(localRef, out KCC* kcc)) {
                return false;
            }

            return !kcc->Data.IsGrounded;
        }

        public override void OnPointerDown(PointerEventData eventData) {
            base.OnPointerDown(eventData);
        }

        public override void OnPointerUp(PointerEventData eventData) {
            base.OnPointerUp(eventData);
        }

        private static void ApplyColor(Image target, Color baseColor) {
            if (target == null) {
                return;
            }

            var color = baseColor;
            color.a     = target.color.a;
            target.color = color;
        }
    }
}
