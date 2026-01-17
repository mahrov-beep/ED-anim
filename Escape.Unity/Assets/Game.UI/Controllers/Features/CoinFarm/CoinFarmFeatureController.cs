namespace Game.UI.Controllers.Features.CoinFarm {
    using System;
    using Cysharp.Threading.Tasks;
    using Domain.CoinFarms;
    using Domain.ExpProgressionRewards;
    using Domain.Features;
    using ExpProgressionRewards;
    using Multicast;
    using Shared;
    using Shared.UserProfile.Commands.CoinFarm;
    using Sirenix.OdinInspector;

    [Serializable, RequireFieldsInit]
    public struct CoinFarmFeatureControllerArgs : IFlowControllerArgs {
    }

    public class CoinFarmFeatureController : FlowController<CoinFarmFeatureControllerArgs> {
        [Inject] private CoinFarmsModel             coinFarmsModel;
        [Inject] private FeaturesModel              featuresModel;
        [Inject] private ExpProgressionRewardsModel expProgressionRewardsModel;

        protected override async UniTask Activate(Context context) {
            CoinFarmFeatureEvents.Collect.Listen(this.Lifetime, args => this.RequestFlow(this.Collect, args, FlowOptions.NowOrNever));
        }

        private async UniTask Collect(Context context, CoinFarmFeatureEvents.CollectArgs args) {
            var coinFarmModel = this.coinFarmsModel.Get(args.CoinFarmKey);

            if (!this.featuresModel.IsFeatureUnlocked(coinFarmModel.LockedByFeatureKey)) {
                if (this.featuresModel.TryGetFeatureUnlockExpProgressionReward(coinFarmModel.LockedByFeatureKey, out var model)) {
                    this.expProgressionRewardsModel.Selected = model;
                    ExpProgressionRewardsFeatureEvents.Open.Raise();
                }

                return;
            }

            await context.Server.ExecuteUserProfile(new UserProfileCoinFarmCollectCommand {
                CoinFarmKey = args.CoinFarmKey,
            }, ServerCallRetryStrategy.RetryWithUserDialog);
        }

        [Button]
        private void RaiseCollectBadges() {
            CoinFarmFeatureEvents.Collect.Raise(new CoinFarmFeatureEvents.CollectArgs {
                CoinFarmKey = SharedConstants.Game.CoinFarms.COIN_FARM_BADGES,
            });
        }
    }
}