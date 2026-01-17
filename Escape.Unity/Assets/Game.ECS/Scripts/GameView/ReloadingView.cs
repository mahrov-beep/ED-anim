namespace _Project.Scripts.GameView {
    using Game.ECS.Systems.Player;
    using Multicast;
    using Photon.Deterministic;
    using Quantum;
    using TMPro;
    using UnityEngine;
    using UnityEngine.UI;

    public sealed class ReloadingView : QuantumSceneViewComponent {
        [Header("UI")]
        [SerializeField] private Image              fillImage;
        [SerializeField] private Image              backgroundImage;
        [SerializeField] private Image              iconImage;
        [SerializeField] private TextMeshProUGUI    timerText;

        [Header("Colors")]
        [SerializeField] private Color readyColor     = Color.white;
        [SerializeField] private Color reloadingColor = Color.yellow;

        [Header("Animation")]
        [SerializeField] private float rotationAngle = 360f;

        private LocalPlayerSystem localPlayerSystem;
        private Vector3           iconDefaultEuler;

        public override void OnActivate(Frame frame) {
            base.OnActivate(frame);

            this.timerText.enabled = false;
            this.localPlayerSystem = App.Get<LocalPlayerSystem>();
            this.iconDefaultEuler  = this.iconImage != null ? this.iconImage.rectTransform.localEulerAngles : Vector3.zero;

            this.ApplyReloadColors(false);
        }

        public override void OnUpdateView() {
            base.OnUpdateView();

            if (localPlayerSystem.HasNotLocalEntityRef(out var localRef)) {
                return;
            }

            UpdateReloading(localRef);
        }

        public unsafe void UpdateReloading(EntityRef entityRef) {
            var f = this.PredictedFrame;

            var weaponOwner = f.GetPointer<Unit>(entityRef);

            if (!weaponOwner->TryGetActiveWeapon(f, out var weapon)) {
                this.ResetUI();
                return;
            }

            if (weapon->IsReloading) {
                this.timerText.enabled = true;

                var reloadTime = weapon->CurrentStats.reloadingTime;
                var normalized = 0f;

                if (reloadTime > FP._0) {
                    FP elapsed = reloadTime - weapon->ReloadingTimer;
                    normalized = Mathf.Clamp01(elapsed.AsFloat / reloadTime.AsFloat);
                }

                if (this.fillImage != null) {
                    this.fillImage.fillAmount = 1f - normalized;
                }

                this.timerText.text = weapon->ReloadingTimer.ToString("0.0") + "s.";

                this.ApplyReloadColors(true);
                this.RotateIcon(normalized);
            }
            else {
                this.ResetUI();
            }
        }

        private void ResetUI() {
            if (this.fillImage != null) {
                this.fillImage.fillAmount = 0f;
            }

            this.timerText.enabled = false;
            this.ApplyReloadColors(false);
            this.ResetIconRotation();
        }

        private void ApplyReloadColors(bool isReloading) {
            var target = isReloading ? this.reloadingColor : this.readyColor;

            ApplyColor(this.backgroundImage, target);
            ApplyColor(this.iconImage, target);
        }

        private void RotateIcon(float normalizedProgress) {
            if (this.iconImage == null) {
                return;
            }

            var rotationOffset = Vector3.forward * (-this.rotationAngle * normalizedProgress);
            this.iconImage.rectTransform.localEulerAngles = this.iconDefaultEuler + rotationOffset;
        }

        private void ResetIconRotation() {
            if (this.iconImage == null) {
                return;
            }

            this.iconImage.rectTransform.localEulerAngles = this.iconDefaultEuler;
        }

        private static void ApplyColor(Image target, Color color) {
            if (target == null) {
                return;
            }

            var preserved = color;
            preserved.a   = target.color.a;
            target.color  = preserved;
        }
    }
}
