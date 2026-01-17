namespace Game.UI.Views.Gunsmiths {
    using UniMob.UI;
    using Multicast;
    using Shared;

    public class GunsmithLoadoutLevelBlockerView : AutoView<IGunsmithLoadoutLevelBlockerState> {
        protected override AutoViewVariableBinding[] Variables => new[] {
            this.Variable("thresher_key", () => this.State.ThresherKey, SharedConstants.Game.Threshers.TRADER),
            this.Variable("required_level", () => this.State.RequiredThresherLevel, 7),
        };
    }

    public interface IGunsmithLoadoutLevelBlockerState : IViewState {
        string ThresherKey { get; }

        int RequiredThresherLevel { get; }
    }
}