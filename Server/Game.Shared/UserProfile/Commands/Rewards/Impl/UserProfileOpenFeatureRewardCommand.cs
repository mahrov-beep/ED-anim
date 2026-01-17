namespace Game.Shared.UserProfile.Commands.Rewards.Impl {
    using System;
    using MessagePack;
    using Multicast;
    using System.Threading.Tasks;
    using Data;
    using Multicast.Numerics;

    [MessagePackObject, RequireFieldsInit]
    public class UserProfileOpenFeatureRewardCommand : IUserProfileServerCommand {
        [Key(0)] public Reward Reward;
    }

    public class UserProfileOpenFeatureRewardCommandHandler : UserProfileServerCommandHandler<UserProfileOpenFeatureRewardCommand> {
        private readonly GameDef gameDef;

        public UserProfileOpenFeatureRewardCommandHandler(GameDef gameDef) {
            this.gameDef = gameDef;
        }

        public override async Task<ServerCommandResult> Execute(UserProfileServerCommandContext context, SdUserProfile gameData, UserProfileOpenFeatureRewardCommand command) {
            var reward = command.Reward;

            if (!reward.ItemTypeIs(SharedConstants.RewardTypes.FEATURE)) {
                return BadRequest("Not a feature drop");
            }

            if (!reward.AmountTypeIs(RewardAmountType.Int)) {
                return BadRequest("Feature drop must be with int amount");
            }

            if (!this.gameDef.Features.TryGet(command.Reward.ItemKey, out var featureDef)) {
                return BadRequest("Feature drop must be with valid feature key");
            }

            gameData.Features.Get(featureDef.key).Unlocked.Value = true;

            return Ok;
        }
    }
}