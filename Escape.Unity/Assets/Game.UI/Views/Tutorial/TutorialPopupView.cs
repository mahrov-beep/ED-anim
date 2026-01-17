namespace Game.UI.Views.Tutorial {
    using UniMob.UI;
    using Multicast;
    using Shared;

    public class TutorialPopupView : AutoView<ITutorialPopupState> {
        protected override AutoViewVariableBinding[] Variables => new[] {
            this.Variable("tutorial_key", () => this.State.TutorialKey, SharedConstants.Game.Tutorials.FIRST_PLAY),
            this.Variable("tutorial_step", () => this.State.TutorialStep, "DialogGameModesInfo"),
        };

        protected override AutoViewEventBinding[] Events => new[] {
            this.Event("close", () => this.State.Close()),
        };
    }

    public interface ITutorialPopupState : IViewState {
        string TutorialKey  { get; }
        string TutorialStep { get; }

        void Close();
    }
}