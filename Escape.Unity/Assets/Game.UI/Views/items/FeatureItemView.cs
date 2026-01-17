namespace Game.UI.Views.items {
    using UniMob.UI;
    using Multicast;
    using Shared;

    public class FeatureItemView : AutoView<IFeatureItemState> {
        protected override AutoViewVariableBinding[] Variables => new[] {
            this.Variable("feature_key", () => this.State.FeatureKey, SharedConstants.Game.Features.GUNSMITH),
        };
    }

    public interface IFeatureItemState : IViewState {
        string FeatureKey { get; }
    }
}