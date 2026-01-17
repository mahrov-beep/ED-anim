namespace Game.UI.Views.GameModes {
    using UniMob.UI;
    using Multicast;
    using Sirenix.OdinInspector;
    using UniMob.UI.Widgets;
    using UnityEngine;

    public class GameModesMenuView : AutoView<IGameModesMenuState> {
        [SerializeField, Required] private ViewPanel detailsPanel;
        [SerializeField, Required] private ViewPanel modesPanel;

        protected override AutoViewVariableBinding[] Variables => new[] {
            this.Variable("can_confirm", () => this.State.CanConfirm, true),
            this.Variable("no_enough_quality", () => this.State.NoEnoughQuality, false),
            this.Variable("no_enough_profile_level", () => this.State.NoEnoughProfileLevel, false),
        };


        protected override AutoViewEventBinding[] Events => new[] {
            this.Event("close", () => this.State.Close()),
            this.Event("confirm", () => this.State.Confirm()),
        };

        protected override void Render() {
            base.Render();

            this.detailsPanel.Render(this.State.Details);
            this.modesPanel.Render(this.State.Modes);
        }
    }

    public interface IGameModesMenuState : IViewState {
        IState Details { get; }
        IState Modes   { get; }

        bool CanConfirm           { get; }
        bool NoEnoughQuality      { get; }
        bool NoEnoughProfileLevel { get; }

        void Close();
        void Confirm();
    }
}