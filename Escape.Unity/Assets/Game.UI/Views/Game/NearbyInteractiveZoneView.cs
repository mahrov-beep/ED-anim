namespace Game.UI.Views.Game {
    using UniMob.UI;
    using Multicast;

    public class NearbyInteractiveZoneView : AutoView<INearbyInteractiveZoneState> {
        protected override AutoViewVariableBinding[] Variables => new[] {
            this.Variable("remaining_time", () => this.State.RemainingTime, 4f),
            this.Variable("total_time", () => this.State.TotalTime, 5f),
        };
    }

    public interface INearbyInteractiveZoneState : IViewState {
        float RemainingTime { get; }
        float TotalTime     { get; }
    }
}