namespace Game.UI.Controllers.RewardOpening {
    using System;
    using Cysharp.Threading.Tasks;
    using Domain.UserData;
    using Multicast;
    using Shared;
    using Shared.UserProfile.Commands.Rewards;
    using Shared.UserProfile.Data;

    [Serializable, RequireFieldsInit]
    public struct RewardOpeningControllerArgs : IFlowControllerArgs {
    }

    public class RewardOpeningController : FlowController<RewardOpeningControllerArgs> {
        [Inject] private SdUserProfile userProfile;
        [Inject] private GameData      gameData;

        protected override async UniTask OnFlow(Context context) {
            while (this.userProfile.Rewards.IsEmpty == false) {
                var reward = this.userProfile.Rewards.First;

                if (reward.Reward.ItemTypeIs(SharedConstants.LootBoxTypes.CONGRATULATIONS)) {
                    // await context.RootNavigator.PushRewardScreen(reward.Reward);
                }

                var rewardGuid = reward.Guid;

                await context.Server.ExecuteUserProfile(new UserProfileOpenRewardCommand {
                    RewardGuid = rewardGuid,
                }, ServerCallRetryStrategy.RetryWithUserDialog);

                if (this.userProfile.Rewards.Contains(rewardGuid)) {
                    throw new Exception("Unexpectedly failed to claim reward in RewardOpeningFlow");
                }
            }
        }
    }
}