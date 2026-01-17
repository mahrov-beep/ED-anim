namespace Game.Domain.Features {
    using ExpProgressionRewards;
    using Multicast;
    using Shared;
    using Shared.UserProfile.Data;
    using UniMob;

    public class FeaturesModel : Model {
        [Inject] private ExpProgressionRewardsModel expProgressionRewardsModel;
        [Inject] private SdUserProfile              userProfile;

        public FeaturesModel(Lifetime lifetime) : base(lifetime) {
        }

        public bool IsFeatureViewed(string featureKey) {
            return this.userProfile.Features.Get(featureKey).Viewed.Value;
        }

        public bool IsFeatureUnlocked(string featureKey) {
            return this.userProfile.Features.Get(featureKey).Unlocked.Value;
        }

        public bool TryGetFeatureUnlockExpProgressionReward(string featureKey, out ExpProgressionRewardModel result) {
            foreach (var expProgressionRewardModel in this.expProgressionRewardsModel.All) {
                foreach (var reward in expProgressionRewardModel.RewardsPreview) {
                    if (reward.ItemTypeIs(SharedConstants.RewardTypes.FEATURE) && reward.ItemKey == featureKey) {
                        result = expProgressionRewardModel;
                        return true;
                    }
                }
            }

            result = null;
            return false;
        }
    }
}