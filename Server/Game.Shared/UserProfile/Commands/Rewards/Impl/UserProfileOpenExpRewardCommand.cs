namespace Game.Shared.UserProfile.Commands.Rewards.Impl {
    using System;
    using MessagePack;
    using Multicast;
    using System.Threading.Tasks;
    using Data;
    using Multicast.Numerics;

    [MessagePackObject, RequireFieldsInit]
    public class UserProfileOpenExpRewardCommand : IUserProfileServerCommand {
        [Key(0)] public Reward Reward;
    }

    public class UserProfileOpenExpRewardCommandHandler : UserProfileServerCommandHandler<UserProfileOpenExpRewardCommand> {
        private readonly GameDef gameDef;

        public UserProfileOpenExpRewardCommandHandler(GameDef gameDef) {
            this.gameDef = gameDef;
        }

        public override async Task<ServerCommandResult> Execute(UserProfileServerCommandContext context, SdUserProfile gameData, UserProfileOpenExpRewardCommand command) {
            var reward = command.Reward;

            if (!reward.ItemTypeIs(SharedConstants.RewardTypes.EXP)) {
                return BadRequest("Not a exp drop");
            }

            if (!reward.AmountTypeIs(RewardAmountType.Int)) {
                return BadRequest("Exp drop must be with int amount");
            }

            gameData.Exp.Value += reward.IntAmount;

            return Ok;
        }
    }
}