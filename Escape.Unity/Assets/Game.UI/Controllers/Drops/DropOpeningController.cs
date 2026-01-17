namespace Game.UI.Controllers.Drops {
    using System;
    using Cysharp.Threading.Tasks;
    using Domain.Commands;
    using Domain.UserData;
    using Multicast;
    using Multicast.DropSystem;
    using Shared.UserProfile.Commands.Rewards;

    [Serializable, RequireFieldsInit]
    public struct DropOpeningControllerArgs : IFlowControllerArgs {
    }

    public class DropOpeningController : FlowController<DropOpeningControllerArgs> {
        [Inject] private GameData gameData;
        
        protected override async UniTask OnFlow(Context context) {
            if (this.gameData.Drops.IsEmpty) {
                return;
            }

            var first = this.gameData.Drops.First;

            var drops = DropUtils.GetAllDrops(first.Drop);

            var rewardGuid = Guid.NewGuid().ToString();

            foreach (var drop in drops) {
                var reward = RewardBuildUtilityHelper.FromDrop(drop);

                context.Server.ExecuteUserProfile(new UserProfileQueueDropRewardCommand {
                    Reward     = reward,
                    RewardGuid = rewardGuid,
                }, ServerCallRetryStrategy.RetryWithUserDialog);
            }

            context.Execute(new RemoveDropCommand(first.Guid));
        }
    }
}