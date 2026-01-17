namespace Game.UI.Views.Tutorial {
    using UniMob.UI;
    using Multicast;

    public class TutorialView : AutoView<ITutorialState> {
        protected override AutoViewVariableBinding[] Variables => new[] {
            this.Variable("tutorial_enabled", () => this.State.TutorialEnabled, true),
        };

        // protected override AutoViewEventBinding[] Events => new[] {
        // };
    }

    public interface ITutorialState : IViewState {
        bool TutorialEnabled { get; }
    }
}