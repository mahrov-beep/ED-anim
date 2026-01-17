namespace Game.UI.Views.World {
    using UniMob.UI;
    using Multicast;
    using Sirenix.OdinInspector;
    using UnityEngine;
    using UnityEngine.UI;

    [RequireComponent(typeof(WorldView))]
    public class UnitHealthBarView : AutoView<IUnitHealthBarState> {
        [SerializeField, Required] private WorldView worldView = default;

        [SerializeField] private Vector3 worldOffset = Vector3.zero;

        [SerializeField] private SlowDebuffView slowDebuffView;

        [SerializeField, Required] private CanvasGroup alphaCanvasGroup;
        [SerializeField, Required] private Image healthFillImage;
        [SerializeField] private Color normalHealthColor = new Color(1f, 0.16470589f, 0f);
        [SerializeField] private Color knockHealthColor = new Color(1f, 0.6f, 0.2f);

        protected override AutoViewVariableBinding[] Variables => new[] {
            this.Variable("health", () => this.GetDisplayedHealth(), 10),            
            this.Variable("max_health", () => this.State.MaxHealth, 100),
            this.Variable("is_dead", () => this.State.IsDead, false),
            this.Variable("nick_name", () => this.State.NickName, "Player#000000000"),
            this.Variable("alpha", () => this.State.Alpha, 1f),
            this.Variable("is_knocked", () => this.State.IsKnocked, false),
            this.Variable("knock_health", () => this.State.KnockHealth, 0f),
        };

        protected override void Activate() {
            base.Activate();

            this.worldView.SetTarget(() => this.State.WorldPos + this.worldOffset);
            UpdateFillColor();
        }

        protected override void Deactivate() {
            this.worldView.SetTarget(null);

            base.Deactivate();
        }

        protected override void Render() {
            base.Render();

            slowDebuffView.Render(State.SlowDebuffState);
            this.alphaCanvasGroup.alpha = State.Alpha;

            UpdateFillColor();
        }

        private float GetDisplayedHealth() {
            return this.State.IsKnocked ? this.State.KnockHealth : this.State.Health;
        }

        private void UpdateFillColor() {
            if (healthFillImage == null) {
                return;
            }

            healthFillImage.color = this.State.IsKnocked ? knockHealthColor : normalHealthColor;
        }
    }

    public interface IUnitHealthBarState : IViewState {
        ISlowDebuffViewState SlowDebuffState { get; }

        Vector3 WorldPos { get; }

        string NickName { get; }

        float Health    { get; }
        float MaxHealth { get; }
        float KnockHealth { get; }
        bool  IsKnocked   { get; }
        bool  IsBeingRevived { get; }
        float Alpha     { get; }

        bool IsDead { get; }
    }
}