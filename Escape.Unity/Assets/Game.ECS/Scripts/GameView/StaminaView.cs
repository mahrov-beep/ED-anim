namespace _Project.Scripts.GameView {
    using System;
    using Game.ECS.Systems.Player;
    using Multicast;
    using Quantum;
    using Sirenix.OdinInspector;
    using UnityEngine;
    using UnityEngine.UI;

    public sealed class StaminaView : QuantumSceneViewComponent {
        [Header("References")]
        [SerializeField, Required] private Image bar;
        [SerializeField, Required] private Image icon;
        [SerializeField, Required] private CanvasGroup group;

        [Header("Colors")]
        [SerializeField, Required] private Color colorNormal = new Color(0.8f, 0.9f, 1f, 1f);
        [SerializeField, Required] private Color colorRegenerating        = new Color(0.45f, 1f, 0.6f, 1f);
        [SerializeField, Required] private Color colorDepleted            = new Color(1f, 0.35f, 0.35f, 1f);
        [SerializeField, Required] private Color colorInsufficientToStart = new Color(1f, 0.8f, 0.2f, 1f);
        [SerializeField, Required] private Color colorJumpDenied          = new Color(1f, 0.25f, 0.25f, 1f);

        [Header("Smoothing")]
        [SerializeField, Required] private float fillLerpSpeed = 12f;
        [SerializeField, Required] private float alphaLerpSpeed = 10f;

        [Header("Auto Hide")]
        [SerializeField, Required] private bool autoHideWhenFull = true;
        [SerializeField, Required] private float autoHideDelay = 0.7f;

        [Header("Blink/Pulse")]
        [SerializeField, Required] private float depletedBlinkSpeed = 6f;
        [SerializeField, Required]                private float regeneratingPulseSpeed = 2f;
        [SerializeField, Range(0f, 1f), Required] private float minBlinkAlpha          = 0.35f;

        [Header("Jump Feedback")]
        [SerializeField, Required] private float jumpDeniedBlinkSpeed = 8f;
        [SerializeField, Required] private float jumpDeniedDuration = 0.8f;
        [SerializeField, Required] private float jumpBlinkAlpha   = 0.1f;

        private LocalPlayerSystem        localPlayerSystem;
        private float                    currentFill;
        private float                    hideTimer;
        private float                    jumpDeniedTimer;
        private DispatcherSubscription   jumpDeniedSubscription;

        private void Start() {
            bar.fillAmount = 1;
            group.alpha    = 0;
        }

        public override void OnActivate(Frame f) {
            localPlayerSystem = App.Get<LocalPlayerSystem>();
            jumpDeniedSubscription = QuantumEvent.Subscribe(this, (EventStaminaJumpDenied _) => OnJumpDenied(), onlyIfActiveAndEnabled: true);
        }

        public override void OnDeactivate() {
            QuantumEvent.Unsubscribe(jumpDeniedSubscription);
            jumpDeniedSubscription = default;
            jumpDeniedTimer        = 0f;
        }

        private void FadeOut(float dt) {
            group.alpha = Mathf.Lerp(group.alpha, 0f, dt * alphaLerpSpeed);
        }

        public override unsafe void OnUpdateView() {
            Frame f  = PredictedFrame;
            float dt = Time.unscaledDeltaTime;

            if (localPlayerSystem.HasNotLocalEntityRef(out var localRef)) {
                FadeOut(dt);
                return;
            }

            if (!f.TryGet(localRef, out UnitFeatureSprintWithStamina stamina) ||
                !f.TryGet(localRef, out Unit unit)) {
                FadeOut(dt);
                return;
            }

            var settings = f.FindAsset(unit.Asset).sprintSettings;
            if (!settings.enabled) {
                FadeOut(dt);
                return;
            }

            float target = Mathf.Clamp01(stamina.current.AsFloat / settings.maxStamina.AsFloat);

            if (float.IsNaN(target)) {
                target = 0f;
            }

            if (jumpDeniedTimer > 0f) {
                jumpDeniedTimer = Mathf.Max(0f, jumpDeniedTimer - dt);
            }

            bool jumpDeniedActive    = jumpDeniedTimer > 0f;
            bool isDepleted          = stamina.IsDepleted;
            bool isRegenerating      = stamina.CanRegenerate(settings);
            bool isFull              = stamina.IsFull(settings);
            bool insufficientToStart = !stamina.CanStartSprint(settings);

            currentFill    = Mathf.Lerp(currentFill, target, dt * fillLerpSpeed);
            bar.fillAmount = currentFill;

            Color targetColor;
            float visualAlpha = 1f;

            if (jumpDeniedActive) {
                targetColor = colorJumpDenied;
                float t = 0.5f + 0.5f * Mathf.Sin(Time.unscaledTime * jumpDeniedBlinkSpeed);
                visualAlpha = Mathf.Lerp(jumpBlinkAlpha, 1f, t);
            }
            else
            if (isDepleted) {
                targetColor = colorDepleted;
                float t = 0.5f + 0.5f * Mathf.Sin(Time.unscaledTime * depletedBlinkSpeed);
                visualAlpha = Mathf.Lerp(minBlinkAlpha, 1f, t);
            }
            else if (isRegenerating) {
                targetColor = insufficientToStart ? colorInsufficientToStart : colorRegenerating;
                float t = 0.5f + 0.5f * Mathf.Sin(Time.unscaledTime * regeneratingPulseSpeed);
                visualAlpha = Mathf.Lerp(0.6f, 1f, t);
            }
            else if (insufficientToStart) {
                targetColor = colorInsufficientToStart;
                visualAlpha = 1f;
            }
            else {
                targetColor = colorNormal;
                visualAlpha = 1f;
            }

            bool wantsHide = autoHideWhenFull && !isDepleted && !isRegenerating && isFull;

            if (wantsHide) {
                hideTimer += dt;
            }
            else {
                hideTimer = 0f;
            }

            float targetGroupAlpha = (wantsHide && hideTimer > autoHideDelay) ? 0f : 1f;
            group.alpha = Mathf.Lerp(group.alpha, targetGroupAlpha * visualAlpha, dt * alphaLerpSpeed);

            var barColor = bar.color;
            barColor.r = targetColor.r;
            barColor.g = targetColor.g;
            barColor.b = targetColor.b;
            bar.color  = barColor;

            icon.color = targetColor;
        }

        private void OnJumpDenied() {
            jumpDeniedTimer = jumpDeniedDuration;
        }
    }
}