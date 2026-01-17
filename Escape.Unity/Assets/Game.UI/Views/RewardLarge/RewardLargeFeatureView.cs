namespace Game.UI.Views.RewardLarge {
    using UniMob.UI;
    using Multicast;
    using Shared;

    public class RewardLargeFeatureView : AutoView<IRewardLargeFeatureState> {
        protected override AutoViewVariableBinding[] Variables => new[] {
            this.Variable("feature_key", () => this.State.FeatureKey, SharedConstants.Game.Features.GUNSMITH),
        };
    }

    public interface IRewardLargeFeatureState : IViewState {
        string FeatureKey { get; }
    }
}