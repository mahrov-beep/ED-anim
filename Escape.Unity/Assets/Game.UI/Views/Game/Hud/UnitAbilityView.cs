namespace Game.UI.Views.Game {
    using Multicast;
    using Shared;
    using TMPro;
    using UniMob.UI;
    using UnityEngine;
    using static Shared.SharedConstants.Game.Items;

    public sealed class UnitAbilityView : AutoView<IUnitAbilityState> {
        [SerializeField] private TextMeshProUGUI timerText;
        protected override AutoViewVariableBinding[] Variables => new[] {
            this.Variable("reloading_progress", () => this.State.ReloadingProgress, 0.7f),
            this.Variable("item_key", () => this.State.ItemKey, ABILITY_GRENADE),
            this.Variable("item_icon", () => {
                var icon = this.State.ItemIcon; 
                return string.IsNullOrEmpty(icon) ? ABILITY_GRENADE : icon;
            }, ABILITY_GRENADE),
        };

        protected override void Activate() {
            base.Activate();
            this.timerText.enabled = false;
        }

        protected override void Render() {
            base.Render();
            var timerTextVisible = State.ReloadingProgress > 0;
            this.timerText.enabled = timerTextVisible;
            if (timerTextVisible) {
                this.timerText.text = ((1-State.ReloadingProgress) * 100).ToString("F0") + "%";
            }
        }
    }

    public interface IUnitAbilityState : IViewState {
        float ReloadingProgress { get; }

        string ItemKey  { get; }
        string ItemIcon { get; }
    }
}