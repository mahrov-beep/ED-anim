namespace Game.UI.Controllers.Features.ExpProgressionRewards {
    using System;
    using System.Collections.Generic;
    using Cysharp.Threading.Tasks;
    using Domain.ExpProgressionRewards;
    using Multicast;
    using Shared;
    using Shared.UserProfile.Commands.Rewards;
    using Sirenix.OdinInspector;

    [Serializable, RequireFieldsInit]
    public struct ExpProgressionRewardsFeatureControllerArgs : IFlowControllerArgs {
    }

    public class ExpProgressionRewardsFeatureController : FlowController<ExpProgressionRewardsFeatureControllerArgs> {
        [Inject] private ExpProgressionRewardsModel expProgressionRewardsModel;

        protected override async UniTask Activate(Context context) {
            ExpProgressionRewardsFeatureEvents.Open.Listen(this.Lifetime, () => this.RequestFlow(this.OpenExpProgressionRewardsScreen));
        }

        protected override async UniTask OnFlow(Context context) {
            if (this.expProgressionRewardsModel.TryGetLevelUp(out _, out _)) {
                await this.OpenExpProgressionRewardsScreen(context);
            }
        }

        private async UniTask OpenExpProgressionRewardsScreen(Context context) {
            await context.RunChild(new ExpProgressionRewardsScreenControllerArgs());
        }

        [Button(ButtonSizes.Large)]
        private void RaiseOpenExpProgressionRewardsScreen() {
            ExpProgressionRewardsFeatureEvents.Open.Raise();
        }

        [Button("Cheat: Add Exp", ButtonSizes.Large)]
        private void CheatAddExp100(int expAmount) {
            this.RequestFlow(async context => {
                await context.Server.ExecuteUserProfile(new UserProfileQueueDropRewardCommand {
                    RewardGuid = Guid.NewGuid().ToString(),
                    Reward     = Multicast.Numerics.Reward.Int(SharedConstants.RewardTypes.EXP, SharedConstants.Game.Exp.MATCH_PLAYED, expAmount),
                }, ServerCallRetryStrategy.Throw);
            });
        }

        // [ShowInInspector, TableList(DrawScrollView = false)]
        // private List<ExpProgressionRewardModel> InspectorModels => this.expProgressionRewardsModel.All;
    }
}