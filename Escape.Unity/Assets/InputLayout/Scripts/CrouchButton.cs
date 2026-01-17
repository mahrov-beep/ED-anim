namespace InputLayout.Scripts {
    using Game.ECS.Systems.Player;
    using Game.Services.Photon;
    using Multicast;
    using Quantum;
    using Sirenix.OdinInspector;
    using UnityEngine;
    using UnityEngine.EventSystems;
    using UnityEngine.UI;

    public sealed class CrouchButton : ScreenButton {
        [Header("UI")]
        [SerializeField, Required] private Image background;
        [SerializeField, Required] private Image icon;

        [Header("Colors")]
        [SerializeField] private Color availableColor = Color.white;
        [SerializeField] private Color crouchingColor = Color.yellow;

        private PhotonService     photonService;
        private LocalPlayerSystem localPlayerSystem;
        private bool              lastCrouchState;
        private bool              initializedState;

        protected override void OnDisable() {
            base.OnDisable();
            
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
            var isCrouching = EvaluateCrouchState();

            if (!force && initializedState && isCrouching == lastCrouchState) {
                return;
            }

            initializedState = true;
            lastCrouchState  = isCrouching;

            var targetColor = isCrouching ? crouchingColor : availableColor;
            ApplyColor(background, targetColor);
            ApplyColor(icon, targetColor);
        }

        private unsafe bool EvaluateCrouchState() {
            if (photonService == null || localPlayerSystem == null) {
                return false;
            }

            if (!photonService.TryGetPredicted(out var frame)) {
                return false;
            }

            if (localPlayerSystem.HasNotLocalEntityRef(out var localRef)) {
                return false;
            }

            return CharacterFsm.CurrentStateIs<CharacterStateCrouchIdle>(frame, localRef) ||
                   CharacterFsm.CurrentStateIs<CharacterStateCrouchMove>(frame, localRef);
        }

        public override void OnPointerDown(PointerEventData eventData) {
            base.OnPointerDown(eventData);
        }

        public override void OnPointerUp(PointerEventData eventData) {
            base.OnPointerUp(eventData);
        }

        private static void ApplyColor(Image target, Color color) {
            if (target == null) {
                return;
            }

            color.a        = target.color.a;
            target.color   = color;
        }
    }
}
