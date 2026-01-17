namespace Game.UI.Views.Game.Hud {
    using BrunoMikoski.AnimationSequencer;
    using Multicast;
    using Sirenix.OdinInspector;
    using UnityEngine;
    using UnityEngine.UI;
    using UniMob.UI;
    using UnityEngine.Serialization;

    public class HealthView : AutoView<IHealthState> {
        [SerializeField, Required] 
        private AnimationSequencerController onDamageAnimation, onHealAnimation;
        [SerializeField, Required]
        private Image healthFillImage;
        [SerializeField, Required]
        private Image icon;
        [SerializeField]
        private Color normalHealthColor = new Color(0.6226415f, 0f, 0f);
        [SerializeField]
        private Color knockHealthColor = new Color(1f, 0.6f, 0.2f);

        private float previousDisplayedHealth;

        protected override AutoViewVariableBinding[] Variables => new[] {
            this.Variable("health", () => this.GetDisplayedHealth(), 70),
            this.Variable("max_health", () => this.State.MaxHealth, 100),
            this.Variable("is_knocked", () => this.State.IsKnocked, false),
            this.Variable("is_being_revived", () => this.State.IsBeingRevived, false),
            this.Variable("knock_time_remaining", () => this.State.KnockTimeRemaining, 0f),
            this.Variable("knock_time_total", () => this.State.KnockTimeTotal, 0f),
            this.Variable("revive_progress", () => this.State.ReviveProgress, 0f),
            this.Variable("knock_health", () => this.State.KnockHealth, 0f),
        };

        protected override void Activate() {
            base.Activate();

            previousDisplayedHealth = this.GetDisplayedHealth();
            this.UpdateFillColor();
        }

        protected override void Render() {
            base.Render();

            var displayedHealth = this.GetDisplayedHealth();

            if (previousDisplayedHealth > displayedHealth && !onDamageAnimation.IsPlaying) {
                onDamageAnimation.Rewind();
            }

            if (previousDisplayedHealth < displayedHealth && !onHealAnimation.IsPlaying) {
                onHealAnimation.Rewind();
            }

            previousDisplayedHealth = displayedHealth;

            this.UpdateFillColor();
        }

        private float GetDisplayedHealth() {
            return this.State.IsKnocked ? this.State.KnockHealth : this.State.Health;
        }

        private void UpdateFillColor() {
            if (healthFillImage == null) {
                return;
            }
             
            var currentColor = this.State.IsKnocked ? knockHealthColor : normalHealthColor;
            healthFillImage.color = currentColor;
            icon.color =  currentColor;
        }
    }

    public interface IHealthState : IViewState {
        float Health             { get; }
        float MaxHealth          { get; }
        float KnockHealth        { get; }
        bool  IsKnocked          { get; }
        bool  IsBeingRevived     { get; }
        float KnockTimeRemaining { get; }
        float KnockTimeTotal     { get; }
        float ReviveProgress     { get; }
    }
}